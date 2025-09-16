// Assets/01_Scripts/RoomSpawner.cs
using Photon.Pun;
using UnityEngine;

public class RoomSpawner : MonoBehaviourPunCallbacks
{
    // 1=Bottom, 2=Top, 3=Left, 4=Right  (del spawner actual)
    public int OpenSide;

    RoomTemplate template;
    bool started;
    bool spawned;

    void Start()
    {
        if (PhotonNetwork.InRoom) TryBegin();
    }

    public override void OnJoinedRoom()
    {
        TryBegin();
    }

    void TryBegin()
    {
        if (started) return;
        started = true;

        if (!PhotonNetwork.IsMasterClient) { enabled = false; return; }

        template = FindObjectOfType<RoomTemplate>();
        if (!template)
        {
            Debug.LogError("[Spawner] NO encontré RoomTemplate en escena.");
            enabled = false;
            return;
        }

        Invoke(nameof(Spawn), 0.05f);
    }

    void Spawn()
    {
        if (spawned || template == null) return;

        // 1) ¿Queda presupuesto?
        if (!template.TryConsumeRoomBudget())
        {
            // Sin presupuesto: colocar CAP opcional o cortar
            if (template.ClosedRoom != null)
            {
                var cap = PlaceRoom(template.ClosedRoom);
                if (cap)
                {
                    // Asegúrate de que el cap no tenga spawners
                    RemoveAllChildSpawners(cap);
                    template.RegisterRoom(cap);
                    // Desactivar la puerta por la que entramos
                    DisableBackSpawnerByEntry(cap, GetEntryTransformUsed(cap));
                }
            }
            spawned = true;
            return;
        }

        // 2) Sala normal (o shop si toca)
        GameObject prefab = template.ShouldForceShopThisTick()
            ? template.GetShopForSide(OpenSide)
            : PickFor(OpenSide);

        if (!prefab)
        {
            // No hay prefab para este lado: corta con cap si existe
            if (template.ClosedRoom != null)
            {
                var cap = PlaceRoom(template.ClosedRoom);
                if (cap)
                {
                    RemoveAllChildSpawners(cap);
                    template.RegisterRoom(cap);
                    DisableBackSpawnerByEntry(cap, GetEntryTransformUsed(cap));
                }
            }
            spawned = true;
            return;
        }

        var go = PlaceRoom(prefab);
        if (!go) { spawned = true; return; }

        template.RegisterRoom(go);
        if (!go.GetComponent<RoomActivator>()) go.AddComponent<RoomActivator>();

        // Desactiva el spawner justo de la puerta por la que acabamos de entrar
        DisableBackSpawnerByEntry(go, GetEntryTransformUsed(go));

        // 3) Si no queda presupuesto después de colocar esta sala, corta cualquier expansión
        if (!template.HasBudgetLeft)
            RemoveAllChildSpawners(go);

        spawned = true;
    }

    // ---------- Instanciación + alineación ----------
    GameObject PlaceRoom(GameObject prefab)
    {
        var go = NetInstantiate(prefab, transform.position, prefab.transform.rotation);
        if (!go) return null;

        var anchors = go.GetComponent<RoomAnchors>();
        if (!anchors)
        {
            Debug.LogWarning($"[Spawner] '{go.name}' no tiene RoomAnchors.");
            return go;
        }

        // ⚠️ PUERTA A USAR EN LA SALA NUEVA: la opuesta a la del spawner
        int entrySideInNew = Opposite(OpenSide);
        var entry = anchors.GetEntryForOpenSide(entrySideInNew);
        if (!entry)
        {
            Debug.LogWarning($"[Spawner] '{go.name}' sin ancla para lado {entrySideInNew} (opuesto de {OpenSide}).");
            return go;
        }

        // ROTACIÓN: hacer que ese entry mire hacia -forward del spawner
        Vector3 want = -transform.forward; want.y = 0; want.Normalize();
        Vector3 have = entry.forward; have.y = 0; have.Normalize();
        if (have.sqrMagnitude > 1e-4f && want.sqrMagnitude > 1e-4f)
        {
            float yaw = Vector3.SignedAngle(have, want, Vector3.up);
            go.transform.Rotate(Vector3.up, yaw, Space.World);
        }
        var eul = go.transform.eulerAngles;
        go.transform.rotation = Quaternion.Euler(0f, eul.y, 0f);

        // POSICIÓN: puerta con puerta
        Vector3 delta = transform.position - entry.position;
        go.transform.position += delta;

        // Guarda el entry usado para este go (lo leeremos luego)
        _lastUsedEntry = entry;
        return go;
    }

    Transform _lastUsedEntry; // cache del entry usado en PlaceRoom
    Transform GetEntryTransformUsed(GameObject go)
    {
        // Si por alguna razón PlaceRoom no lo dejó cacheado, intenta volver a calcularlo
        if (_lastUsedEntry && _lastUsedEntry.gameObject && _lastUsedEntry.root == go.transform.root)
            return _lastUsedEntry;

        var anchors = go.GetComponent<RoomAnchors>();
        if (!anchors) return null;
        int entrySideInNew = Opposite(OpenSide);
        return anchors.GetEntryForOpenSide(entrySideInNew);
    }

    // ---------- Helpers ----------
    GameObject PickFor(int side)
    {
        if (side == 1) return Pick(template.botRooms);
        if (side == 2) return Pick(template.topRooms);
        if (side == 3) return Pick(template.leftRooms);
        if (side == 4) return Pick(template.rightRooms);
        return null;
    }
    static GameObject Pick(GameObject[] list) =>
        (list != null && list.Length > 0) ? list[Random.Range(0, list.Length)] : null;

    static int Opposite(int side) => (side == 1) ? 2 : (side == 2) ? 1 : (side == 3) ? 4 : 3;

    GameObject NetInstantiate(GameObject prefabAsset, Vector3 pos, Quaternion rot)
    {
        string[] prefixes = { "", "Prefabs/", "Prefabs/Huichito/", "Huichito/" };
        foreach (var pre in prefixes)
        {
            string key = pre + prefabAsset.name;
            var probe = Resources.Load<GameObject>(key);
            if (probe) return PhotonNetwork.InstantiateRoomObject(key, pos, rot);
        }
        Debug.LogError($"[Spawner] No encuentro en Resources: '{prefabAsset.name}'.");
        return null;
    }

    // 🔒 Desactiva el spawner más cercano al entry usado (robusto)
    void DisableBackSpawnerByEntry(GameObject roomGO, Transform usedEntry)
    {
        if (!usedEntry) return;
        var spawners = roomGO.GetComponentsInChildren<RoomSpawner>(true);
        RoomSpawner best = null; float bestD = float.MaxValue;
        Vector3 p = usedEntry.position;

        foreach (var sp in spawners)
        {
            if (!sp) continue;
            float d = (sp.transform.position - p).sqrMagnitude;
            if (d < bestD) { bestD = d; best = sp; }
        }
        if (best) best.gameObject.SetActive(false);
    }

    // Deshabilita/Elimina todos los spawners hijos (para caps o corte de expansión)
    void RemoveAllChildSpawners(GameObject roomGO)
    {
        var spawners = roomGO.GetComponentsInChildren<RoomSpawner>(true);
        for (int i = 0; i < spawners.Length; i++)
            if (spawners[i] != null) spawners[i].gameObject.SetActive(false);
    }

    // Evitar duplicados cuando dos spawnpoints se cruzan
    void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (spawned) return;
        if (!other.CompareTag("SpawnPoint")) return;

        var otherSpawner = other.GetComponent<RoomSpawner>();
        if (otherSpawner && !otherSpawner.spawned && !spawned)
        {
            if (template != null && template.ClosedRoom != null && template.CanCloseRoom())
            {
                var cap = PlaceRoom(template.ClosedRoom);
                if (cap)
                {
                    RemoveAllChildSpawners(cap);
                    template.RegisterRoom(cap);
                }
                spawned = true;
            }
        }
    }
}
