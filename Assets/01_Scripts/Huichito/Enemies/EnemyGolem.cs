using Photon.Pun;
using UnityEngine;

public class EnemyGolem : EnemyBase
{
    [Header("Golem (melee pesado)")]
    public float attackDamage = 22f;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.2f;

    float cd;

    protected override void Start()
    {
        base.Start();

        // Ajustes tanque opcionales (puedes tunear en el Inspector)
        var hp = GetComponent<Health>();
        if (hp)
        {
            if (hp.maxHP < 220f) hp.maxHP = 220f;
            if (hp.damageReduction < 0.4f) hp.damageReduction = 0.4f;
            hp.currentHP = hp.maxHP;
        }

        if (moveSpeed > 2.6f) moveSpeed = 2.6f; // golem lento
    }

    void Update()
    {
        // IA solo en el Master y cuando la room está activa
        if (!PhotonNetwork.IsMasterClient || !isActive) return;

        // Buscar/actualizar target
        if (!player) player = FindClosestPlayer();
        if (!player) return;

        LookAtPlayerFlat();

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > Mathf.Max(attackRange, stopDistance))
        {
            // Acercarse hasta quedar en rango
            MoveTowardsPlayer();
            return;
        }

        // Atacar con cooldown
        cd -= Time.deltaTime;
        if (cd <= 0f)
        {
            var net = player.GetComponent<HealthNetwork>();
            if (net != null)
                net.TakeDamageNetwork(attackDamage); // daño aplicado para todos los clientes

            cd = attackCooldown;
        }
    }
}
