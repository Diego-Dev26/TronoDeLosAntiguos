using UnityEngine;

public class EnemyGoblin : EnemyBase
{
    [Header("Melee")]
    public float attackDamage = 10f;
    public float attackRange = 1.7f;
    public float attackCooldown = 1.0f;
    float cd;

    void Update()
    {
        if (!player) return;

        LookAtPlayerFlat();

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            cd -= Time.deltaTime;
            if (cd <= 0f)
            {
                var hp = player.GetComponent<Health>();
                if (hp) hp.TakeDamage(attackDamage);
                cd = attackCooldown;
            }
        }
    }
}
