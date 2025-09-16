using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnerArea : MonoBehaviourPunCallbacks
{
    [Header("Qué y cuánto (modo clásico)")]
    public GameObject[] enemyPrefabs;
    public int totalToSpawn = 0;

    [Header("Modo progresivo por sala")]
    public bool useProgression = true;
    [Tooltip("Mínimo por sala (todas las rooms)")]
    public int minPerRoom = 0;
    [Tooltip("En la última sala habrá +extra respecto al mínimo (lineal). Ej: 7 => última tendrá min+7)")]
    public int extraPerRoomAtEnd = 0;
    [Tooltip("Límite para no sobrecargar salas pequeñas (0 = sin límite)")]
    public int maxPerRoom = 0;

    [Header("Validación de posición")]
    [Tooltip("Debe incluir SOLO RoomFootprint")]
    public LayerMask floorMask;              // solo RoomFootprint
    [Tooltip("NO debe incluir RoomFootprint")]
    public LayerMask blockMask;              // Walls/Default/etc. (NO RoomFootprint)
    public float minSeparation = 1.0f;
    public float insidePadding = 0.5f;
    public int attemptsPerEnemy = 30;

    [Header("Esperas")]
    public float maxWaitForRooms = 5f;
    public float stableWindow = 0.75f;

    [Header("Debug")]
    public bool verboseLog = false;

    RoomTemplate template;
    readonly List<Vector3> placed = new List<Vector3>();

    void Start()
    {
        if (PhotonNetwork.InRoom) TrySpawn();
    }

    public override void OnJoinedRoom()
    {
        TrySpawn();
    }

    void TrySpawn()
    {
        if (!PhotonNetwork.IsMasterClient) return; // solo master
        template = Object.FindObjectOfType<RoomTemplate>();
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        // (1) Esperar a que la lista de rooms sea estable
        float elapsed = 0f, stable = 0f; int lastCount = -1;
        while (elapsed < maxWaitForRooms)
        {
            if (!template) template = Object.FindObjectOfType<RoomTemplate>();
            int cur = (template && template.rooms != null) ? template.rooms.Count : 0;

            if (cur == lastCount && cur > 0) stable += Time.deltaTime;
            else { stable = 0f; lastCount = cur; }

            if (cur > 0 && stable >= stableWindow) break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // (1.5) Esperar a que no queden RoomSpawner (mapa finalizado)
        float genStable = 0f, genWait = 0f;
        while (genWait < maxWaitForRooms)
        {
            int spawners = Object.FindObjectsOfType<RoomSpawner>(true).Length;
            if (spawners == 0) { genStable += Time.deltaTime; if (genStable >= stableWindow) break; }
            else { genStable = 0f; }
            genWait += Time.deltaTime;
            yield return null;
        }

        // (2) Rooms de trabajo
        List<GameObject> rooms = (template && template.rooms != null && template.rooms.Count > 0)
            ? new List<GameObject>(template.rooms)
            : FindRoomsRuntime();

        if (rooms.Count == 0)
        {
            if (verboseLog) Debug.LogWarning("[EnemySpawn] No hay rooms; aborto.");
            yield break;
        }
        MarkExitRoom(rooms[rooms.Count - 1]);

        // (3) Cuotas por sala
        int spawned = 0, fallidos = 0;

        if (useProgression)
        {
            // progresión lineal: índice 0..n-1
            int n = rooms.Count;
            for (int i = 0; i < n; i++)
            {
                float t = (n <= 1) ? 1f : (float)i / (n - 1); // progreso 0..1
                int quota = minPerRoom + Mathf.RoundToInt(Mathf.Lerp(0f, extraPerRoomAtEnd, t));
                if (maxPerRoom > 0) quota = Mathf.Min(quota, maxPerRoom);

                int placedHere = 0, guard = 0;
                while (placedHere < quota && guard < quota * attemptsPerEnemy)
                {
                    guard++;
                    if (TrySpawnOneInRoom(rooms[i])) { placedHere++; spawned++; }
                    else fallidos++;
                }

                if (verboseLog) Debug.Log($"[EnemySpawn] Room {i + 1}/{n} -> intentado {quota}, colocado {placedHere}.");
                yield return null; // respirar por room
            }
        }
        else
        {
            // modo clásico (por total)
            int guard = 0;
            while (spawned < totalToSpawn && guard < totalToSpawn * attemptsPerEnemy)
            {
                guard++;
                var room = rooms[Random.Range(0, rooms.Count)];
                if (TrySpawnOneInRoom(room)) spawned++;
                else fallidos++;
                if ((spawned & 1) == 0) yield return null;
            }
        }

        if (verboseLog) Debug.Log($"[EnemySpawn] Spawneados {spawned} (fallidos {fallidos}).");
    }

    // ---------- Spawnear 1 en una room ----------
    bool TrySpawnOneInRoom(GameObject room)
    {
        if (!TryGetRandomPointInRoom(room, insidePadding, out Vector3 floorPoint))
            return false;

        if (HasNearby(floorPoint, placed, minSeparation)) return false;

        var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject go = NetInstantiateRoomObject(
            prefab,
            floorPoint + Vector3.up * 2f,
            Quaternion.Euler(0, Random.Range(0f, 360f), 0)
        );
        if (!go) return false;

        SnapToFloor(go, room);
        placed.Add(floorPoint);

        var act = room.GetComponent<RoomActivator>();
        var eb = go.GetComponent<EnemyBase>();
        if (act && eb) act.RegisterEnemy(eb);

        return true;
    }

    // ---------- Búsqueda de rooms / puntos ----------
    List<GameObject> FindRoomsRuntime()
    {
        var set = new HashSet<GameObject>();
        var anchors = Object.FindObjectsOfType<RoomAnchors>(true);
        for (int i = 0; i < anchors.Length; i++)
            set.Add(anchors[i].transform.root.gameObject);
        return new List<GameObject>(set);
    }

    bool TryGetRandomPointInRoom(GameObject room, float padding, out Vector3 pos)
    {
        if (!TryGetRoomBounds(room, out Bounds b)) { pos = Vector3.zero; return false; }

        for (int i = 0; i < attemptsPerEnemy; i++)
        {
            float x = Random.Range(b.min.x + padding, b.max.x - padding);
            float z = Random.Range(b.min.z + padding, b.max.z - padding);
            Vector3 rayStart = new Vector3(x, b.max.y + 5f, z);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 80f, floorMask, QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.transform.IsChildOf(room.transform)) continue;
                pos = hit.point;
                return true;
            }

            // Fallback puntual
            if (i == 10)
            {
                if (Physics.Raycast(rayStart, Vector3.down, out hit, 80f, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.collider.transform.IsChildOf(room.transform) && Vector3.Dot(hit.normal, Vector3.up) > 0.8f)
                    {
                        pos = hit.point;
                        return true;
                    }
                }
            }
        }

        pos = Vector3.zero;
        return false;
    }

    bool TryGetRoomBounds(GameObject room, out Bounds bounds)
    {
        int footprintLayer = LayerMask.NameToLayer("RoomFootprint");
        var cols = room.GetComponentsInChildren<Collider>(true);
        bool any = false;
        bounds = new Bounds(room.transform.position, Vector3.zero);

        for (int i = 0; i < cols.Length; i++)
        {
            var c = cols[i];
            if (!c || !c.enabled) continue;
            if (footprintLayer != -1 && c.gameObject.layer != footprintLayer) continue;
            if (!any) { bounds = c.bounds; any = true; } else bounds.Encapsulate(c.bounds);
        }

        if (!any)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                var c = cols[i];
                if (!c || !c.enabled) continue;
                if (!any) { bounds = c.bounds; any = true; } else bounds.Encapsulate(c.bounds);
            }
        }
        return any;
    }

    bool HasNearby(Vector3 p, List<Vector3> list, float r)
    {
        float r2 = r * r;
        for (int i = 0; i < list.Count; i++)
            if ((list[i] - p).sqrMagnitude < r2) return true;
        return false;
    }

    // ---------- Net + snap ----------
    GameObject NetInstantiateRoomObject(GameObject prefabAsset, Vector3 pos, Quaternion rot)
    {
        string[] prefixes = { "", "Prefabs/", "Prefabs/Huichito/", "Huichito/" };
        for (int i = 0; i < prefixes.Length; i++)
        {
            string key = prefixes[i] + prefabAsset.name;
            if (Resources.Load<GameObject>(key) != null)
                return PhotonNetwork.InstantiateRoomObject(key, pos, rot);
        }
        Debug.LogError("[EnemySpawn] No encuentro en Resources: '" + prefabAsset.name + "'.");
        return null;
    }
    void MarkExitRoom(GameObject room)
    {
        if (!room) return;
        var act = room.GetComponent<RoomActivator>();
        if (!act) act = room.AddComponent<RoomActivator>();
        act.SetExit(true);
    }
    void SnapToFloor(GameObject go, GameObject room)
    {
        int footprint = LayerMask.NameToLayer("RoomFootprint");
        int mask = (footprint != -1) ? (1 << footprint) : Physics.AllLayers;

        float floorY = go.transform.position.y;
        if (Physics.Raycast(go.transform.position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 80f, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform.IsChildOf(room.transform))
                floorY = hit.point.y;
        }

        float offsetY = 0.2f;
        var cc = go.GetComponent<CharacterController>();
        if (cc)
            offsetY = (cc.height * 0.5f) + cc.skinWidth - cc.center.y + 0.01f;
        else
        {
            var cap = go.GetComponent<CapsuleCollider>();
            if (cap)
            {
                float halfHeight = Mathf.Max(cap.height * 0.5f, cap.radius);
                offsetY = halfHeight - cap.center.y + 0.01f;
            }
            else
            {
                var col = go.GetComponent<Collider>();
                if (col) offsetY = col.bounds.extents.y + 0.01f;
            }
        }

        var p = go.transform.position;
        p.y = floorY + offsetY;
        go.transform.position = p;
    }
}
