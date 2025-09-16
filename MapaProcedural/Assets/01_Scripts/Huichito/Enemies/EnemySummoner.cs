using UnityEngine;
using System.Collections.Generic;

public class EnemySummoner : EnemyBase
{
    [Header("Summon")]
    public GameObject minionPrefab;  // arrastra tu Goblin.prefab
    public int maxMinions = 3;
    public float summonCooldown = 4f;
    public float summonRadius = 3f;

    [Header("Ranged Cast")]
    public GameObject projectilePrefab; // arrastra tu Projectile prefab
    public Transform muzzle;            // Empty en la "mano/boca" (p.ej. Y=1.6, Z=0.9)
    public float shotDamage = 12f;
    public float shotSpeed = 14f;
    public float shotLife = 4f;
    public float shootCooldown = 1.8f;
    public float shootRange = 14f;
    public bool requireLineOfSight = false; // actívalo cuando todo funcione
    public LayerMask losMask = ~0;

    [Header("Posicionamiento")]
    public float preferredDistance = 10f;

    List<GameObject> activeMinions = new List<GameObject>();
    float cd;        // cooldown de invocación
    float shootCd;   // cooldown de disparo

    void Update()
    {
        if (!player) return;

        // Limpia referencias nulas (minions muertos)
        activeMinions.RemoveAll(x => x == null);

        LookAtPlayerFlat();
        float d = Vector3.Distance(transform.position, player.position);

        // Mantener distancia (kite)
        if (d < preferredDistance * 0.9f) MoveAwayFromPlayer();
        else if (d > preferredDistance * 1.2f) MoveTowardsPlayer();

        // Invocar
        cd -= Time.deltaTime;
        if (cd <= 0f && activeMinions.Count < maxMinions)
        {
            SummonOne();
            cd = summonCooldown;
        }

        // Disparar
        shootCd -= Time.deltaTime;
        if (projectilePrefab && d <= shootRange && shootCd <= 0f)
        {
            Vector3 origin = muzzle ? muzzle.position : transform.position + Vector3.up * 1.6f;
            Vector3 target = player.position + Vector3.up * 1.0f;
            Vector3 dir = (target - origin).normalized;

            bool canShoot = true;
            if (requireLineOfSight)
            {
                canShoot = false;
                if (Physics.Raycast(origin, dir, out RaycastHit hit, shootRange, losMask))
                    canShoot = hit.collider.CompareTag("Player");

#if UNITY_EDITOR
                Debug.DrawRay(origin, dir * shootRange, canShoot ? Color.magenta : Color.gray, 0f, false);
#endif
            }

            if (canShoot)
            {
                Shoot(origin, dir);
                shootCd = shootCooldown;
            }
        }
    }

    void SummonOne()
    {
        if (!minionPrefab) return;
        Vector2 c = Random.insideUnitCircle.normalized * summonRadius;
        Vector3 pos = transform.position + new Vector3(c.x, 0, c.y);

        var go = Instantiate(minionPrefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        go.name = go.name.Replace("(Clone)", "") + " (Minion)";
        activeMinions.Add(go);

        // Debuffs de minion para diferenciarlo
        var hp = go.GetComponent<Health>(); if (hp) { hp.maxHP *= 0.6f; hp.currentHP = hp.maxHP; }
        var gob = go.GetComponent<EnemyGoblin>(); if (gob) { gob.moveSpeed += 0.5f; gob.attackDamage *= 0.8f; }
    }

    void Shoot(Vector3 origin, Vector3 dir)
    {
        var go = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(dir));
        var pr = go.GetComponent<ProjectileSimple>();
        if (pr)
        {
            pr.damage = shotDamage;
            pr.speed = shotSpeed;
            pr.life = shotLife;
            pr.onlyDamageEnemies = false; // para que dañe al Player (no a enemigos)
        }
    }
}
