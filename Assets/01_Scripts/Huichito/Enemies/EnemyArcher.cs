using Photon.Pun;
using UnityEngine;

public class EnemyArcher : EnemyBase
{
    [Header("Shooting")]
    public GameObject projectilePrefab;     // Prefab con PhotonView (en Resources)
    public Transform muzzle;                // Empty a ~ (0, 1.4, 0.6) en el modelo
    public float shotDamage = 14f;
    public float shotSpeed = 16f;
    public float shotLife = 4f;
    public float shootRange = 12f;
    public float keepDistance = 8f;
    public float shootCooldown = 1.5f;

    [Header("Line of Sight (opcional)")]
    public bool requireLineOfSight = false; // ponlo en true cuando todo esté OK
    public LayerMask losMask = ~0;

    float cd;

    void Update()
    {
        // IA corre solo en el Master y únicamente cuando la room está activa
        if (!PhotonNetwork.IsMasterClient || !isActive) return;

        // Buscar/actualizar target
        if (!player) player = FindClosestPlayer();
        if (!player) return;

        LookAtPlayerFlat();

        // Posicionamiento: mantener distancia preferida con el jugador
        float d = Vector3.Distance(transform.position, player.position);
        if (d < keepDistance * 0.9f) MoveAwayFromPlayer();
        else if (d > keepDistance * 1.2f && d < shootRange) MoveTowardsPlayer();

        // Disparo si está en rango
        if (d <= shootRange)
        {
            bool canShoot = true;

            Vector3 origin = muzzle ? muzzle.position : transform.position + Vector3.up * 1.4f;
            Vector3 target = player.position + Vector3.up * 1.0f;
            Vector3 dir = (target - origin).normalized;

            if (requireLineOfSight)
            {
                canShoot = false;
                RaycastHit hit;
                if (Physics.Raycast(origin, dir, out hit, shootRange, losMask, QueryTriggerInteraction.Ignore))
                    canShoot = hit.collider.CompareTag("Player");

#if UNITY_EDITOR
                Debug.DrawRay(origin, dir * shootRange, canShoot ? Color.green : Color.red, 0f, false);
#endif
            }

            cd -= Time.deltaTime;
            if (canShoot && cd <= 0f)
            {
                Shoot(origin, dir);
                cd = shootCooldown;
            }
        }
    }

    void Shoot(Vector3 origin, Vector3 dir)
    {
        if (!projectilePrefab) return;

        // Instanciar por red (Resources) para que todos vean el proyectil
        GameObject go = NetInstantiateRoomObject(projectilePrefab, origin, Quaternion.LookRotation(dir));
        if (!go) return;

        var pr = go.GetComponent<ProjectileSimple>();
        if (pr != null)
        {
            pr.damage = shotDamage;
            pr.speed = shotSpeed;
            pr.life = shotLife;
            pr.onlyDamageEnemies = false; // debe dañar al Player
        }
    }

    // Helper local para cargar desde Resources con varios prefijos
    GameObject NetInstantiateRoomObject(GameObject prefabAsset, Vector3 pos, Quaternion rot)
    {
        string[] prefixes = { "", "Prefabs/", "Prefabs/Huichito/", "Huichito/" };
        for (int i = 0; i < prefixes.Length; i++)
        {
            string key = prefixes[i] + prefabAsset.name;
            if (Resources.Load<GameObject>(key) != null)
                return PhotonNetwork.InstantiateRoomObject(key, pos, rot);
        }
        Debug.LogError("[EnemyArcher] No encuentro en Resources: '" + prefabAsset.name + "'.");
        return null;
    }
}
