using Photon.Pun;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(1000)] // Asegura que se ejecute tarde
public class PlayerClassSelectorSimple : MonoBehaviour
{
    public enum PlayerClass { MeleeRapido, MeleeTanque, Arquero, MagoAOE }

    [Header("Clase a probar")]
    public PlayerClass clase = PlayerClass.Arquero;

    [Header("Refs (auto si conviven en el mismo GO)")]
    public Health health;
    public PlayerControllerSimple controller;
    public PlayerCombatSimple combat;
    public Transform muzzle;                // hijo "Muzzle" (0,1.4,0.9)

    [Header("Prefabs de proyectil")]
    public GameObject projectileSingle;     // ProjectileSimple
    public GameObject projectileAOE;        // opcional (AoE)

    private PlayerClass _lastApplied;
    [SerializeField] private PlayerClassSelectorSimple selector;

    void Reset()
    {
        if (!health) health = GetComponent<Health>();
        if (!controller) controller = GetComponent<PlayerControllerSimple>();
        if (!combat) combat = GetComponent<PlayerCombatSimple>();
        if (!muzzle)
        {
            var m = transform.Find("Muzzle");
            if (m) muzzle = m;
        }
        if (!selector) selector = GetComponent<PlayerClassSelectorSimple>();
    }

    void Awake()
    {
        Reset();
        ApplyClass("Awake");
    }

    void OnEnable()
    {
        // Por si otro script pisa cosas en OnEnable/Start
        ApplyClass("OnEnable");
    }

    void Start()
    {
        // Asegura aplicar una vez más al final del primer frame
        StartCoroutine(ApplyNextFrame());
    }

    System.Collections.IEnumerator ApplyNextFrame()
    {
        yield return null; // espera 1 frame
        ApplyClass("Start->EndOfFrame");
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            Reset();
            ApplyClass("OnValidate(EditMode)");
        }
    }
#endif

    [ContextMenu("Apply Class")]
    public void ApplyClass()
    {
        ApplyClass("ContextMenu");
    }

    void ApplyClass(string source)
    {
        if (!health || !controller || !combat)
        {
            Debug.LogWarning($"[PlayerClassSelector] {name}: faltan refs -> Health:{health != null}, Controller:{controller != null}, Combat:{combat != null}");
            return;
        }

        if (muzzle) combat.muzzle = muzzle;

        switch (clase)
        {
            case PlayerClass.MeleeRapido:
                health.maxHP = 100f; health.damageReduction = 0f; health.currentHP = health.maxHP;
                controller.moveSpeed = 6.5f;

                combat.enableMelee = true;
                combat.meleeDamage = 14f;
                combat.meleeRadius = 1.0f;
                combat.meleeRange = 1.6f;
                combat.meleeCooldown = 0.35f;

                combat.enableShoot = false;
                combat.fireOnLeftClick = false;
                combat.projectilePrefab = null;
                break;

            case PlayerClass.MeleeTanque:
                health.maxHP = 160f; health.damageReduction = 0.2f; health.currentHP = health.maxHP;
                controller.moveSpeed = 4.5f;

                combat.enableMelee = true;
                combat.meleeDamage = 24f;
                combat.meleeRadius = 1.05f;
                combat.meleeRange = 1.7f;
                combat.meleeCooldown = 0.75f;

                combat.enableShoot = false;
                combat.fireOnLeftClick = false;
                combat.projectilePrefab = null;
                break;

            case PlayerClass.Arquero:
                health.maxHP = 100f; health.damageReduction = 0f; health.currentHP = health.maxHP;
                controller.moveSpeed = 6.2f;

                combat.enableMelee = false;          // sin melee
                combat.enableShoot = true;           // dispara
                combat.fireOnLeftClick = true;       // con LMB

                combat.projectilePrefab = projectileSingle;
                combat.projectileDamage = 16f;
                combat.projectileSpeed = 24f;
                combat.projectileLife = 4f;
                combat.shootCooldown = 0.45f;
                break;

            case PlayerClass.MagoAOE:
                health.maxHP = 110f; health.damageReduction = 0f; health.currentHP = health.maxHP;
                controller.moveSpeed = 5.2f;

                combat.enableMelee = false;
                combat.enableShoot = true;
                combat.fireOnLeftClick = true;       // LMB

                combat.projectilePrefab = projectileAOE ? projectileAOE : projectileSingle;
                combat.projectileDamage = 22f;
                combat.projectileSpeed = 16f;
                combat.projectileLife = 4f;
                combat.shootCooldown = 1.0f;
                break;
        }

        LogClase(source);
        _lastApplied = clase;
    }

    void LogClase(string source)
    {
        string projName = combat && combat.projectilePrefab ? combat.projectilePrefab.name : "NULL";
        string muzzleName = combat && combat.muzzle ? combat.muzzle.name : "NULL";
        string meleeInfo = combat && combat.enableMelee ? $"ON dmg:{combat.meleeDamage} cd:{combat.meleeCooldown}" : "OFF";
        string shootInfo = combat && combat.enableShoot ? $"ON prefab:{projName} cd:{combat.shootCooldown} LMB:{combat.fireOnLeftClick}" : "OFF";

        Debug.Log($"[PlayerClassSelector]({source}) GO:{name} Clase:{clase} | HP:{(health ? health.maxHP : 0)} DR:{(health ? health.damageReduction : 0)} | Move:{(controller ? controller.moveSpeed : 0)} | Melee:{meleeInfo} | Shoot:{shootInfo} | Muzzle:{muzzleName}");

        if ((clase == PlayerClass.Arquero || clase == PlayerClass.MagoAOE) && combat.projectilePrefab == null)
            Debug.LogWarning($"[PlayerClassSelector] {name}: Clase {clase} sin projectilePrefab asignado.");
        if ((clase == PlayerClass.Arquero || clase == PlayerClass.MagoAOE) && combat.muzzle == null)
            Debug.LogWarning($"[PlayerClassSelector] {name}: Clase {clase} sin Muzzle asignado.");
    }
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        // 1) recibe la clase que mandó NetLauncher
        string id = null;
        var data = info.photonView.InstantiationData;
        if (data != null && data.Length > 0) id = data[0] as string;

        // 2) si no llegó (caso local), usa PlayerPrefs como respaldo
        if (string.IsNullOrEmpty(id))
            id = PlayerPrefs.GetString("selected_character_id", "MeleeRapido");

        // 3) aplica
        if (!selector) selector = GetComponent<PlayerClassSelectorSimple>();
        if (selector != null)
        {
            selector.clase = Parse(id);
            selector.ApplyClass(); // ← usa tus valores (HP, daño, velocidad, proyectiles, etc.)
            Debug.Log($"[PlayerClassInit] Clase aplicada: {id}");
        }
        else
        {
            Debug.LogWarning("[PlayerClassInit] No encontré PlayerClassSelectorSimple en el prefab.");
        }

        // (Opcional) Si quieres inicializar tus otros scripts de stats según la clase:
        // var stats = GetComponent<PlayerStatsPhoton>();
        // var hp    = GetComponent<Health>();
        // if (stats && hp) { stats.BaseMaxHealth = (int)hp.maxHP; /* etc. */ }
    }

    private PlayerClassSelectorSimple.PlayerClass Parse(string id)
    {
        switch (id)
        {
            case "MeleeRapido": return PlayerClassSelectorSimple.PlayerClass.MeleeRapido;
            case "MeleeTanque": return PlayerClassSelectorSimple.PlayerClass.MeleeTanque;
            case "Arquero": return PlayerClassSelectorSimple.PlayerClass.Arquero;
            case "MagoAOE": return PlayerClassSelectorSimple.PlayerClass.MagoAOE;
            default: return PlayerClassSelectorSimple.PlayerClass.MeleeRapido;
        }
    }
}
