using UnityEngine;

public class EnemyArcher : EnemyBase
{
    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform muzzle;     // asigna un Empty a (0,1.5,0.9)
    public float shotDamage = 14f;
    public float shotSpeed = 16f;
    public float shotLife = 4f;
    public float shootRange = 12f;
    public float keepDistance = 8f;
    public float shootCooldown = 1.5f;
    public bool requireLineOfSight = false; // ponlo en false para prototipo
    public LayerMask losMask = ~0;

    float cd;

    void Update()
    {
        if (!player) return;

        LookAtPlayerFlat();

        float d = Vector3.Distance(transform.position, player.position);

        // Posicionamiento
        if (d < keepDistance * 0.9f) MoveAwayFromPlayer();
        else if (d > keepDistance * 1.2f && d < shootRange) MoveTowardsPlayer();

        // Disparo
        if (d <= shootRange)
        {
            bool canShoot = true;
            Vector3 origin = muzzle ? muzzle.position : transform.position + Vector3.up * 1.4f;
            Vector3 target = player.position + Vector3.up * 1.0f;
            Vector3 dir = (target - origin).normalized;

            if (requireLineOfSight)
            {
                canShoot = false;
                if (Physics.Raycast(origin, dir, out RaycastHit hit, shootRange, losMask))
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
        var go = Object.Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir));
        var pr = go.GetComponent<ProjectileSimple>();
        if (pr)
        {
            pr.damage = shotDamage;
            pr.speed = shotSpeed;
            pr.life = shotLife;
            pr.onlyDamageEnemies = false; // para poder dañar al Player
        }
    }
}
