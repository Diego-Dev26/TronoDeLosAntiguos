using UnityEngine;

public class EnemyGolem : EnemyBase
{
    [Header("Golem")]
    public float attackDamage = 22f;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.2f;
    float cd;

    void Start()
    {
        base.Start();
        // Asegurar valores tanque (puedes ajustar en Inspector)
        var hp = GetComponent<Health>();
        if (hp) { hp.maxHP = Mathf.Max(hp.maxHP, 220f); hp.damageReduction = Mathf.Max(hp.damageReduction, 0.4f); hp.currentHP = hp.maxHP; }
        moveSpeed = Mathf.Min(moveSpeed, 2.6f); // lento
    }

    void Update()
    {
        if (!player) return;

        LookAtPlayerFlat();
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange) MoveTowardsPlayer();
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
