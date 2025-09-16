// Assets/01_Scripts/BossDemon.cs
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class BossDemon : EnemyBase
{
    [Header("Refs")]
    public Transform muzzle;                // opcional: punto de disparo
    public GameObject projectilePrefab;     // ProjectileSimple con PhotonView (en Resources)
    public GameObject minionPrefab;         // tu Goblin (en Resources)

    [Header("Fases por vida")]
    [Range(0f, 1f)] public float phase2At = 0.70f;   // <= 70% HP -> Fase 2
    [Range(0f, 1f)] public float phase3At = 0.35f;   // <= 35% HP -> Fase 3
    int phase = 1;

    [Header("Disparo apuntado (F1/F2)")]
    public float shootCooldown = 1.25f;
    public float shootDamage = 14f;
    public float shootSpeed = 16f;
    public float shootLife = 4f;

    [Header("Anillo circular (F3)")]
    public float ringCooldown = 2.5f;
    public int ringCountBase = 14;         // nº proyectiles por anillo
    public int ringWaves = 1;              // nº de anillos seguidos
    public float ringYawOffsetPerWave = 12f; // rotación entre anillos

    [Header("Invocación (F2/F3)")]
    public float summonCooldown = 5f;
    public int maxMinionsAlive = 4;
    public float summonRadius = 4f;

    // comportamiento básico
    public float preferMinDist = 6f;
    public float preferMaxDist = 12f;

    readonly List<GameObject> minions = new List<GameObject>();
    float shootCd, summonCd, ringCd;

    protected override void Start()
    {
        base.Start();
        // Si quieres ajustar vida/defensas aquí, usa myHealth (del EnemyBase)
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !isActive) return;
        if (!player) return;

        UpdatePhase();

        // movimiento muy simple para mantener distancia jugable
        float d = Vector3.Distance(transform.position, player.position);
        LookAtPlayerFlat();
        if (d < preferMinDist) MoveAwayFromPlayer();
        else if (d > preferMaxDist) MoveTowardsPlayer();

        shootCd -= Time.deltaTime;
        summonCd -= Time.deltaTime;
        ringCd -= Time.deltaTime;

        // Fase 1+: disparo apuntado
        if (phase >= 1 && shootCd <= 0f)
        {
            FireAimedShot();
            shootCd = shootCooldown;
        }

        // Fase 2+: invocar goblins
        if (phase >= 2)
        {
            minions.RemoveAll(x => x == null);
            if (summonCd <= 0f && minions.Count < maxMinionsAlive)
            {
                SummonOne();
                summonCd = summonCooldown;
            }
        }

        // Fase 3+: anillos circulares
        if (phase >= 3 && ringCd <= 0f)
        {
            FireRingSeries();
            ringCd = ringCooldown;
        }
    }

    void UpdatePhase()
    {
        if (!myHealth || myHealth.maxHP <= 0f) { phase = 1; return; }
        float hp01 = myHealth.currentHP / myHealth.maxHP;
        if (hp01 <= phase3At) phase = 3;
        else if (hp01 <= phase2At) phase = 2;
        else phase = 1;
    }

    // ---------- Ataques ----------
    void FireAimedShot()
    {
        if (!projectilePrefab) return;
        Vector3 origin = muzzle ? muzzle.position : (transform.position + Vector3.up * 1.6f);
        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 dir = (target - origin).normalized;

        GameObject go = NetInstantiate(projectilePrefab, origin, Quaternion.LookRotation(dir));
        var pr = go ? go.GetComponent<ProjectileSimple>() : null;
        if (pr != null)
        {
            pr.damage = shootDamage;
            pr.speed = shootSpeed;
            pr.life = shootLife;
            pr.onlyDamageEnemies = false;
        }
    }

    void FireRingSeries()
    {
        int count = Mathf.Max(6, ringCountBase + Mathf.Max(0, PhotonNetwork.CurrentRoom.PlayerCount - 1) * 2);
        Vector3 origin = muzzle ? muzzle.position : (transform.position + Vector3.up * 1.6f);

        int waves = Mathf.Max(1, ringWaves);
        for (int w = 0; w < waves; w++)
        {
            float offset = w * ringYawOffsetPerWave;
            for (int i = 0; i < count; i++)
            {
                float ang = offset + (360f / count) * i;
                Vector3 dir = Quaternion.Euler(0f, ang, 0f) * Vector3.forward;

                GameObject go = NetInstantiate(projectilePrefab, origin, Quaternion.LookRotation(dir));
                var pr = go ? go.GetComponent<ProjectileSimple>() : null;
                if (pr != null)
                {
                    pr.damage = shootDamage;
                    pr.speed = shootSpeed;
                    pr.life = shootLife;
                    pr.onlyDamageEnemies = false;
                }
            }
        }
    }

    void SummonOne()
    {
        if (!minionPrefab) return;
        Vector2 c = Random.insideUnitCircle.normalized * summonRadius;
        Vector3 pos = transform.position + new Vector3(c.x, 0f, c.y);

        GameObject go = NetInstantiate(minionPrefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        if (go) minions.Add(go);
    }

    // ---------- Net helpers ----------
    GameObject NetInstantiate(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        string[] prefixes = { "", "Prefabs/", "Prefabs/Huichito/", "Huichito/" };
        for (int i = 0; i < prefixes.Length; i++)
        {
            string key = prefixes[i] + prefab.name;
            if (Resources.Load<GameObject>(key) != null)
                return PhotonNetwork.InstantiateRoomObject(key, pos, rot);
        }
        Debug.LogError("[BossDemon] No encuentro en Resources: '" + prefab.name + "'.");
        return null;
    }
}
