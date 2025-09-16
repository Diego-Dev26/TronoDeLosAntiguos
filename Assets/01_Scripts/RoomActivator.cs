// Assets/01_Scripts/RoomActivator.cs
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomActivator : MonoBehaviourPunCallbacks
{
    [Header("Volumen")]
    public Collider volume;               // si lo dejas vacío se autogenera
    public float boundsPadding = 0.4f;
    public float triggerHeight = 3f;

    [Header("Exit")]
    public bool isExit = false;
    bool exitConsumed = false;

    readonly List<EnemyBase> enemies = new List<EnemyBase>();

    void Reset() { AutoVolume(); }
    void Awake() { if (!volume) AutoVolume(); }

    public void RegisterEnemy(EnemyBase eb) { if (eb && !enemies.Contains(eb)) enemies.Add(eb); }
    public void SetExit(bool v) { isExit = v; }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Encender IA de esta sala
        for (int i = 0; i < enemies.Count; i++) if (enemies[i]) enemies[i].SetActive(true);

        if (isExit && !exitConsumed)
        {
            exitConsumed = true;
            Debug.Log("[RoomActivator] EXIT disparado por " + name);

            if (PhotonNetwork.IsMasterClient)
                FloorManager.Instance?.NextBatch(this);
        }
    }

    // ---- utilidades para el volumen ----
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
