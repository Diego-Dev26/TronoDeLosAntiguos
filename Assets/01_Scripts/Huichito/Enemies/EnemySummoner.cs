using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class EnemySummoner : EnemyBase
{
    [Header("Summon")]
    public GameObject minionPrefab;     // Prefab con PhotonView (en Resources)
    public int maxMinions = 3;
    public float summonCooldown = 4f;
    public float summonRadius = 3f;

    [Header("Ranged Cast")]
    public GameObject projectilePrefab; // Prefab con PhotonView (en Resources)
    public Transform muzzle;            // Empty a ~ (0, 1.6, 0.9)
    public float shotDamage = 12f;
    public float shotSpeed = 14f;
    public float shotLife = 4f;
    public float shootCooldown = 1.8f;
    public float shootRange = 14f;

    [Header("Line of Sight (opcional)")]
    public bool requireLineOfSight = false;
    public LayerMask losMask = ~0;

    [Header("Posicionamiento")]
    public float preferredDistance = 10f;

    readonly List<GameObject> activeMinions = new List<GameObject>();
    float cdSummon;
    float cdShoot;

    void Update()
    {
        // IA solo en Master y cuando la room está activa
        if (!PhotonNetwork.IsMasterClient || !isActive) return;

        // Target
        if (!player) player = FindClosestPlayer();
        if (!player) return;

        // Limpieza de minions destruidos
        for (int i = activeMinions.Count - 1; i >= 0; i--)
            if (!activeMinions[i]) activeMinions.RemoveAt(i);

        LookAtPlayerFlat();

        float d = Vector3.Distance(transform.position, player.position);

        // Mantener distancia tipo "kite"
        if (d < preferredDistance * 0.9f) MoveAwayFromPlayer();
        else if (d > preferredDistance * 1.2f) MoveTowardsPlayer();

        // Invocar
        cdSummon -= Time.deltaTime;
        if (cdSummon <= 0f && activeMinions.Count < maxMinions)
        {
            SummonOne();
            cdSummon = summonCooldown;
        }

        // Disparo
        cdShoot -= Time.deltaTime;
        if (projectilePrefab && d <= shootRange && cdShoot <= 0f)
        {
            Vector3 origin = muzzle ? muzzle.position : transform.position + Vector3.up * 1.6f;
            Vector3 target = player.position + Vector3.up * 1.0f;
            Vector3 dir = (target - origin).normalized;

            bool canShoot = true;
            if (requireLineOfSight)
            {
                canShoot = false;
                RaycastHit hit;
                if (Physics.Raycast(origin, dir, out hit, shootRange, losMask, QueryTriggerInteraction.Ignore))
                    canShoot = hit.collider.CompareTag("Player");
#if UNITY_EDITOR
                Debug.DrawRay(origin, dir * shootRange, canShoot ? Color.magenta : Color.gray, 0f, false);
#endif
            }

            if (canShoot)
            {
                Shoot(origin, dir);
                cdShoot = shootCooldown;
            }
        }
    }

    void SummonOne()
    {
        if (!minionPrefab) return;

        // Punto alrededor del invocador
        Vector2 c = Random.insideUnitCircle.normalized * summonRadius;
        Vector3 pos = transform.position + new Vector3(c.x, 0f, c.y);

        // Instanciar por red desde Resources
        // Antes: pos + Vector3.up * 2f
        GameObject go = NetInstantiateRoomObject(minionPrefab, pos + Vector3.up * -1f, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        if (!go) return;

        // Alinear al piso de esta misma room (si existe)
        var room = GetComponentInParent<RoomActivator>() ? GetComponentInParent<RoomActivator>().gameObject : gameObject;
        SnapToFloor(go, room);

        // Marcar y registrar
        go.name = go.name.Replace("(Clone)", "") + " (Minion)";
        activeMinions.Add(go);

        // Ajustes de minion (más débil / un poco más rápido)
        var hp = go.GetComponent<Health>();
        if (hp) { hp.maxHP *= 0.6f; hp.currentHP = hp.maxHP; }

        var gob = go.GetComponent<EnemyGoblin>();
        if (gob) { gob.moveSpeed += 0.5f; gob.attackDamage *= 0.8f; }

        // Registrar con RoomActivator para que se apague/encienda junto a la sala
        var act = GetComponentInParent<RoomActivator>();
        var eb = go.GetComponent<EnemyBase>();
        if (act && eb) act.RegisterEnemy(eb);
    }

    void Shoot(Vector3 origin, Vector3 dir)
    {
        if (!projectilePrefab) return;

        GameObject go = NetInstantiateRoomObject(projectilePrefab, origin, Quaternion.LookRotation(dir));
        if (!go) return;

        var pr = go.GetComponent<ProjectileSimple>();
        if (pr)
        {
            pr.damage = shotDamage;
            pr.speed = shotSpeed;
            pr.life = shotLife;
            pr.onlyDamageEnemies = false; // debe dañar al Player
        }
    }

    // ---------------- Helpers ----------------

    // Carga desde Resources con varios prefijos habituales
    GameObject NetInstantiateRoomObject(GameObject prefabAsset, Vector3 pos, Quaternion rot)
    {
        string[] prefixes = { "", "Prefabs/", "Prefabs/Huichito/", "Huichito/" };
        for (int i = 0; i < prefixes.Length; i++)
        {
            string key = prefixes[i] + prefabAsset.name;
            if (Resources.Load<GameObject>(key) != null)
                return PhotonNetwork.InstantiateRoomObject(key, pos, rot);
        }
        Debug.LogError("[EnemySummoner] No encuentro en Resources: '" + prefabAsset.name + "'.");
        return null;
    }

    // Alinea la base del minion al piso de la room (evita que quede atravesado)
    void SnapToFloor(GameObject go, GameObject room)
    {
        // 1) Máscara primaria: RoomFootprint si existe; si no, AllLayers
        int footprint = LayerMask.NameToLayer("RoomFootprint");
        int primaryMask = (footprint != -1) ? (1 << footprint) : Physics.AllLayers;

        // 2) Raycast hacia abajo con fallback a todas las capas
        RaycastHit hit;
        Vector3 start = go.transform.position + Vector3.up * 6f;
        float floorY = go.transform.position.y;

        bool gotHit = Physics.Raycast(start, Vector3.down, out hit, 200f, primaryMask, QueryTriggerInteraction.Ignore)
                      && hit.collider && hit.collider.transform.IsChildOf(room.transform)
                      && Vector3.Dot(hit.normal, Vector3.up) > 0.65f; // evitar paredes/techos

        if (!gotHit)
        {
            if (Physics.Raycast(start, Vector3.down, out hit, 200f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider && hit.collider.transform.IsChildOf(room.transform)
                    && Vector3.Dot(hit.normal, Vector3.up) > 0.65f)
                {
                    gotHit = true;
                }
            }
        }

        if (gotHit) floorY = hit.point.y;

        // 3) Offset según el collider del minion
        float offsetY = 0.2f;
        var cc = go.GetComponent<CharacterController>();
        if (cc) offsetY = (cc.height * 0.5f) + cc.skinWidth - cc.center.y + 0.01f;
        else
        {
            var cap = go.GetComponent<CapsuleCollider>();
            if (cap)
            {
                float halfHeight = Mathf.Max(cap.height * 0.5f, cap.radius);
                offsetY = halfHeight - cap.center.y + 0.01f;
            }
            else
            {
                var col = go.GetComponent<Collider>();
                if (col) offsetY = col.bounds.extents.y + 0.01f;
            }
        }

        var p = go.transform.position;
        p.y = floorY + offsetY;
        go.transform.position = p;
    }

}
