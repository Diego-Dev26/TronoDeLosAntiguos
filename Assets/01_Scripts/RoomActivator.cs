// Assets/01_Scripts/RoomActivator.cs
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomActivator : MonoBehaviourPunCallbacks
{
    [Header("Volumen")]
    public Collider volume;               // si lo dejas vacío se autogenera
    public float boundsPadding = 0.4f;
    public float triggerHeight = 3f;

    [Header("Tipo de sala")]
    [SerializeField] bool isShop = false;
    [SerializeField] bool isExit = false;
    bool exitConsumed = false;

    [Header("Puertas de esta sala")]
    public List<DoorController> doors = new List<DoorController>();

    [Header("Cierre con delay")]
    [Tooltip("Tiempo a esperar después de entrar antes de cerrar las puertas.")]
    public float closeDelay = 0.45f;

    // ---- Estado enemigos ----
    int enemiesAlive = 0;
    bool enteredOnce = false;

    PhotonView pv;

    // ---- Base original ----
    readonly List<EnemyBase> enemies = new List<EnemyBase>();

    void Reset() { AutoVolume(); }
    void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (!volume) AutoVolume();
        if (doors.Count == 0) doors.AddRange(GetComponentsInChildren<DoorController>(true));
    }

    // -------- API pública --------
    public void SetExit(bool v) { isExit = v; }
    public void SetShop(bool v) { isShop = v; }
    public bool IsShop => isShop;

    public void RegisterEnemy(EnemyBase eb)
    {
        if (!eb) return;
        if (!enemies.Contains(eb)) enemies.Add(eb);
        if (PhotonNetwork.IsMasterClient)
        {
            enemiesAlive++;
            var relay = eb.GetComponent<EnemyDeathRelay>();
            if (!relay) relay = eb.gameObject.AddComponent<EnemyDeathRelay>();
            relay.OnDied += OnEnemyDied;
        }
    }

    public void AttachDeathRelay(GameObject enemyGo)
    {
        if (!PhotonNetwork.IsMasterClient || !enemyGo) return;
        var relay = enemyGo.GetComponent<EnemyDeathRelay>();
        if (!relay) relay = enemyGo.AddComponent<EnemyDeathRelay>();
        relay.OnDied += OnEnemyDied;
        enemiesAlive++;
    }

    // -------- Entrada del jugador --------
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (enteredOnce) return; // solo la primera vez que se entra
        enteredOnce = true;

        // Encender IA de esta sala (tu lógica original)
        for (int i = 0; i < enemies.Count; i++) if (enemies[i]) enemies[i].SetActive(true);

        // Si es salida, encadenar tandas
        if (isExit && !exitConsumed)
        {
            exitConsumed = true;
            if (PhotonNetwork.IsMasterClient)
                FloorManager.Instance?.NextBatch(this);
        }

        // Tienda: no cerramos aquí
        if (isShop) return;

        // Cierre simple por delay (solo Master)
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(CloseDoorsAfterDelay());
    }

    // -------- Conteo/limpieza --------
    void OnEnemyDied()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        if (enemiesAlive == 0)
            pv.RPC(nameof(RPC_OpenDoors), RpcTarget.All);
    }

    [PunRPC]
    void RPC_CloseDoors()
    {
        foreach (var d in doors) if (d) d.Close();
    }

    [PunRPC]
    void RPC_OpenDoors()
    {
        foreach (var d in doors) if (d) d.Open();
    }

    IEnumerator CloseDoorsAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        pv.RPC(nameof(RPC_CloseDoors), RpcTarget.All);
    }

    // ---- utilidades para el volumen (tuyas, intactas) ----
    void AutoVolume()
    {
        var col = GetComponent<Collider>();
        if (!col || !col.isTrigger)
        {
            var box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            volume = box;

            Bounds b;
            if (TryGetRoomBounds(out b))
            {
                box.center = transform.InverseTransformPoint(b.center + Vector3.up * (triggerHeight * 0.5f));
                box.size = new Vector3(b.size.x - boundsPadding * 2f, triggerHeight, b.size.z - boundsPadding * 2f);
            }
            else
            {
                box.center = new Vector3(0, triggerHeight * 0.5f, 0);
                box.size = new Vector3(6, triggerHeight, 6);
            }
        }
        else volume = col;
    }

    bool TryGetRoomBounds(out Bounds b)
    {
        int footprintLayer = LayerMask.NameToLayer("RoomFootprint");
        var cols = GetComponentsInChildren<Collider>(true);
        b = new Bounds(transform.position, Vector3.zero);
        bool any = false;

        foreach (var c in cols)
        {
            if (!c || !c.enabled) continue;
            if (footprintLayer != -1 && c.gameObject.layer != footprintLayer) continue;
            if (!any) { b = c.bounds; any = true; } else b.Encapsulate(c.bounds);
        }
        if (!any)
        {
            foreach (var c in cols)
            {
                if (!c || !c.enabled) continue;
                if (!any) { b = c.bounds; any = true; } else b.Encapsulate(c.bounds);
            }
        }
        return any;
    }
}
