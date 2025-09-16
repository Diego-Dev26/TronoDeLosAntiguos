using Photon.Pun;
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
        // IA solo en el Master y cuando la room está activa
        if (!PhotonNetwork.IsMasterClient || !isActive) return;

        // Buscar/actualizar target
        if (!player) player = FindClosestPlayer();
        if (!player) return;

        LookAtPlayerFlat();

        float dist = Vector3.Distance(transform.position, player.position);

        // Acercarse hasta rango de golpe
        if (dist > Mathf.Max(attackRange, stopDistance))
        {
            MoveTowardsPlayer();
            return;
        }

        // Atacar en cooldown
        cd -= Time.deltaTime;
        if (cd <= 0f)
        {
            var net = player.GetComponent<HealthNetwork>();
            if (net != null)
                net.TakeDamageNetwork(attackDamage);  // daño por red para todos

            cd = attackCooldown;
        }
    }
}
