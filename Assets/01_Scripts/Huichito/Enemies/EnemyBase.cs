using Photon.Pun;
using UnityEngine;

public class EnemyBase : MonoBehaviourPunCallbacks
{
    [Header("Refs")]
    public Transform player;                      // objetivo actual (se autollenará)

    [Header("Stats")]
    public float moveSpeed = 4f;                  // velocidad de desplazamiento
    public float detectionRange = 15f;            // para adquirir/soltar target
    public float stopDistance = 1.6f;             // distancia mínima antes de atacar

    protected Health myHealth;
    protected CharacterController cc;
    protected Rigidbody rb;

    // IA solo corre en el Master
    protected bool IsServer => PhotonNetwork.IsMasterClient;

    // RoomActivator enciende/apaga IA al entrar/salir de la sala
    protected bool isActive = true;
    RoomActivator roomActivator;

    // ------------------------------------------------------------------ //
    // Ciclo de vida
    // ------------------------------------------------------------------ //
    protected virtual void Awake()
    {
        myHealth = GetComponent<Health>();
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;            // evitamos pelear con la física si movemos por transform/CC
    }

    protected virtual void Start()
    {
        // Registro opcional en RoomActivator (si existe en el padre)
        roomActivator = GetComponentInParent<RoomActivator>();
        if (roomActivator) roomActivator.RegisterEnemy(this);
    }

    // Llamado por RoomActivator (solo master normalmente)
    public void SetActive(bool value) => isActive = value;

    void LateUpdate()
    {
        if (!IsServer) return;                    // IA solo en master
        if (!isActive) return;                    // apagados hasta que el jugador entre a la sala

        // Refrescar objetivo cada cierto tiempo o si perdimos referencia
        if (!player || Time.frameCount % 15 == 0)
            player = FindClosestPlayer();
    }

    // ------------------------------------------------------------------ //
    // Utilidades comunes para hijos (Archer/Goblin/Golem/Summoner…)
    // ------------------------------------------------------------------ //

    protected Transform FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform best = null;
        float bestSqr = float.PositiveInfinity;

        for (int i = 0; i < players.Length; i++)
        {
            var go = players[i];
            if (!go) continue;
            float d2 = (go.transform.position - transform.position).sqrMagnitude;
            if (d2 < bestSqr)
            {
                bestSqr = d2;
                best = go.transform;
            }
        }

        // Aplicar rango de detección (opcional)
        if (best && detectionRange > 0f)
        {
            float d = Vector3.Distance(transform.position, best.position);
            if (d > detectionRange) best = null;
        }

        return best;
    }

    protected void LookAtPlayerFlat()
    {
        if (!player) return;
        Vector3 to = player.position - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(to);
    }

    protected void MoveTowardsPlayer()
    {
        if (!player) return;
        Vector3 to = player.position - transform.position;
        to.y = 0f;
        Vector3 dir = to.normalized;
        Vector3 vel = dir * moveSpeed;

        if (cc) cc.Move(vel * Time.deltaTime);
        else transform.position += vel * Time.deltaTime;
    }

    protected void MoveAwayFromPlayer()
    {
        if (!player) return;
        Vector3 to = transform.position - player.position;
        to.y = 0f;
        Vector3 dir = to.normalized;
        Vector3 vel = dir * moveSpeed;

        if (cc) cc.Move(vel * Time.deltaTime);
        else transform.position += vel * Time.deltaTime;
    }

    protected bool PlayerInRange(float range)
    {
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }
}
