// Assets/01_Scripts/FloorManager.cs
using Photon.Pun;
using UnityEngine;

public class FloorManager : MonoBehaviourPunCallbacks
{
    public static FloorManager Instance;

    [Header("Refs")]
    public RoomTemplate template;

    [Header("Batch config")]
    public int roomsPerBatch = 15;     // cuántas salas por tanda
    public float seedForwardOffset = 0.15f;  // empuje fuera de la puerta al crear el semillero

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        if (!template) template = FindObjectOfType<RoomTemplate>();
    }

    /// <summary>
    /// Llamado por una sala con RoomActivator(isExit=true) al entrar el player.
    /// Crea un "semillero" (RoomSpawner) justo fuera de la puerta de salida
    /// y resetea el presupuesto para que se generen otras N salas.
    /// </summary>
    public void NextBatch(RoomActivator exitRoom)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!template || !exitRoom)
        {
            Debug.LogWarning("[Floor] Falta template o exitRoom.");
            return;
        }

        // 1) Resetear presupuesto para una nueva tanda
        template.ResetBudget(roomsPerBatch);

        // 2) Necesitamos saber por qué lado está la PUERTA ABIERTA de la sala de salida
        var anchors = exitRoom.GetComponent<RoomAnchors>();
        if (!anchors)
        {
            Debug.LogWarning("[Floor] ExitRoom sin RoomAnchors.");
            return;
        }

        Transform doorAnchor = anchors.right ?? anchors.left ?? anchors.top ?? anchors.bottom;
        int openSide =
            (anchors.right ? 4 :
            (anchors.left ? 3 :
            (anchors.top ? 2 :
            (anchors.bottom ? 1 : 0))));

        if (doorAnchor == null || openSide == 0)
        {
            Debug.LogWarning("[Floor] ExitRoom sin ancla de puerta (asigna exactamente una).");
            return;
        }

        // 3) Crear un semillero mirando hacia fuera (doorAnchor.forward DEBE apuntar hacia FUERA)
        var seed = new GameObject("SeedSpawner");
        seed.tag = "SpawnPoint"; // opcional, útil para la lógica de choque entre spawners
        seed.transform.position = doorAnchor.position + doorAnchor.forward * seedForwardOffset;
        seed.transform.rotation = Quaternion.LookRotation(doorAnchor.forward, Vector3.up);

        // collider trigger pequeñito para ser consistente con otros spawners
        var box = seed.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(0.2f, 0.2f, 0.2f);

        // (sin rigidbody basta, tu RoomSpawner no lo necesita para funcionar)

        var rs = seed.AddComponent<RoomSpawner>();
        // MUY IMPORTANTE: el nuevo spawner tiene que "esperar" una sala cuya ENTRADA quede cara a cara.
        // Tu RoomSpawner calcula la rotación usando -transform.forward, así que aquí basta con
        // colocar el spawner mirando hacia fuera y setear el "OpenSide" correcto:
        rs.OpenSide = openSide;

        // Lo demás lo hace el RoomSpawner (instancia por red, alinea, etc.)
        Debug.Log("[Floor] NextBatch: presupuesto reseteado y seed colocado. Side=" + openSide);
    }
}
