using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplate : MonoBehaviour
{
    [Header("Prefabs por lado (salas normales)")]
    public GameObject[] botRooms;   // 1 = Bottom
    public GameObject[] topRooms;   // 2 = Top
    public GameObject[] leftRooms;  // 3 = Left
    public GameObject[] rightRooms; // 4 = Right

    [Header("Tienda (variantes por lado)")]
    public GameObject shopBottom;
    public GameObject shopTop;
    public GameObject shopLeft;
    public GameObject shopRight;

    [Header("Cierre / Lista")]
    [Tooltip("Prefab opcional para tapar (sin spawners).")]
    public GameObject ClosedRoom;
    public List<GameObject> rooms = new List<GameObject>();

    [Header("Generación")]
    [Tooltip("Tope total de salas por piso (incluye la inicial si la agregas a 'rooms').")]
    public int maxRooms = 15;

    [Tooltip("Frecuencia de tienda (3 => 3, 6, 9, 12, 15…)")]
    public int shopEvery = 3;

    [Header("Boss final")]
    [Tooltip("Prefab del boss (con PhotonView y bajo Resources).")]
    public GameObject boss;
    [Tooltip("Offset vertical para apoyar al boss sobre el piso.")]
    public float bossSpawnYOffset = 0.2f;

    // ---------- Estado interno ----------
    int _reserved;          // reservas realizadas (presupuesto)
    int _lastShopTick = -999;
    bool _initialized;
    bool _bossSpawned;

    void Start()
    {
        if (!_initialized) InitBudget();
    }

    void OnValidate()
    {
        if (!_initialized)
            _reserved = Mathf.Clamp(rooms != null ? rooms.Count : 0, 0, maxRooms);
    }

    void InitBudget()
    {
        _reserved = Mathf.Clamp(rooms != null ? rooms.Count : 0, 0, maxRooms);
        _lastShopTick = -999;
        _initialized = true;
        _bossSpawned = false;
    }

    // ---------- API que usan los spawners ----------
    public bool TryConsumeRoomBudget()
    {
        if (!_initialized) InitBudget();
        if (_reserved >= maxRooms) return false;
        _reserved++;
        return true;
    }

    public bool HasBudgetLeft => _reserved < maxRooms;

    public void RegisterRoom(GameObject go)
    {
        if (go && !rooms.Contains(go)) rooms.Add(go);
    }

    public bool ShouldForceShopThisTick()
    {
        if (shopEvery <= 0) return false;
        if (_reserved <= 0) return false;
        if (_reserved % shopEvery != 0) return false;
        if (_lastShopTick == _reserved) return false;
        _lastShopTick = _reserved;
        return true;
    }

    public GameObject GetShopForSide(int openSide)
    {
        switch (openSide)
        {
            case 1: return shopBottom;
            case 2: return shopTop;
            case 3: return shopLeft;
            case 4: return shopRight;
            default: return null;
        }
    }

    public bool CanCloseRoom() => rooms != null && rooms.Count >= 2;

    public void ResetForNewFloor()
    {
        rooms.Clear();
        _reserved = 0;
        _lastShopTick = -999;
        _initialized = true;
        _bossSpawned = false;
    }

    // ---------- Boss en la última room ----------
    public void TrySpawnBossAtRoom(GameObject room)
    {
        if (_bossSpawned) return;
        if (!PhotonNetwork.IsMasterClient) return;
        if (!boss) return;
        if (!room) return;

        // 1) Punto de spawn: si hay un hijo llamado "BossSpawn", úsalo; si no, el centro del piso.
        Transform t = FindChildRecursive(room.transform, "BossSpawn");
        Vector3 pos;

        if (t)
        {
            pos = t.position;
        }
        else
        {
            // Centro del footprint (o de todos los colliders) y apoyado al piso
            if (!TryGetRoomBounds(room, out Bounds b))
                b = new Bounds(room.transform.position, new Vector3(6, 3, 6));

            Vector3 center = b.center;
            // Raycast hacia abajo a la capa RoomFootprint (si existe)
            int footprint = LayerMask.NameToLayer("RoomFootprint");
            int mask = (footprint != -1) ? (1 << footprint) : Physics.AllLayers;

            float floorY = center.y;
            if (Physics.Raycast(center + Vector3.up * 6f, Vector3.down, out RaycastHit hit, 20f, mask, QueryTriggerInteraction.Ignore))
                floorY = hit.point.y;

            pos = new Vector3(center.x, floorY + bossSpawnYOffset, center.z);
        }

        // 2) Instanciar por red (buscando el prefab bajo Resources)
        string[] prefixes = { "", "Prefabs/", "Prefabs/Huichito/", "Huichito/", "Rooms/" };
        for (int i = 0; i < prefixes.Length; i++)
        {
            string key = prefixes[i] + boss.name;
            if (Resources.Load<GameObject>(key) != null)
            {
                PhotonNetwork.InstantiateRoomObject(key, pos, Quaternion.identity);
                _bossSpawned = true;
                return;
            }
        }

        Debug.LogError("[RoomTemplate] No encuentro el boss en Resources: '" + boss.name + "'.");
    }

    // ---------- helpers internos ----------
    bool TryGetRoomBounds(GameObject room, out Bounds bounds)
    {
        int footprintLayer = LayerMask.NameToLayer("RoomFootprint");
        var cols = room.GetComponentsInChildren<Collider>(true);
        bool any = false;
        bounds = new Bounds(room.transform.position, Vector3.zero);

        // Preferir footprint
        for (int i = 0; i < cols.Length; i++)
        {
            var c = cols[i];
            if (!c || !c.enabled) continue;
            if (footprintLayer != -1 && c.gameObject.layer != footprintLayer) continue;
            if (!any) { bounds = c.bounds; any = true; }
            else bounds.Encapsulate(c.bounds);
        }

        // Fallback: todos
        if (!any)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                if (!c || !c.enabled) continue;
                if (!any) { bounds = c.bounds; any = true; }
                else bounds.Encapsulate(c.bounds);
            }
        }
        return any;
    }
    public void ResetBudget(int newMax)
    {
        // Actualiza el tope para la siguiente tanda/piso
        maxRooms = newMax;

        // Limpia y resetea los contadores igual que harías para un piso nuevo
        rooms.Clear();
        _reserved = 0;
        _lastShopTick = -999;
        _initialized = true;
        _bossSpawned = false;
    }

    Transform FindChildRecursive(Transform root, string name)
    {
        if (!root) return null;
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var r = FindChildRecursive(root.GetChild(i), name);
            if (r) return r;
        }
        return null;
    }
}
