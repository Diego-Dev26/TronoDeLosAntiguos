using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;

    [Header("Stats")]
    public float moveSpeed = 4f;
    public float detectionRange = 15f;
    public float stopDistance = 1.6f; // distancia mínima antes de atacar

    protected Health myHealth;

    protected virtual void Awake()
    {
        myHealth = GetComponent<Health>();
    }

    protected virtual void Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    protected bool PlayerInRange(float range)
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }

    protected void LookAtPlayerFlat()
    {
        if (!player) return;
        Vector3 to = player.position - transform.position;
        to.y = 0;
        if (to.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(to);
    }

    protected void MoveTowardsPlayer()
    {
        if (!player) return;
        Vector3 to = (player.position - transform.position);
        to.y = 0;
        Vector3 dir = to.normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    protected void MoveAwayFromPlayer()
    {
        if (!player) return;
        Vector3 to = (transform.position - player.position);
        to.y = 0;
        Vector3 dir = to.normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
