// Assets/01_Scripts/RoomTemplate.cs
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
    [Tooltip("Cap opcional (tapa): una habitación sin spawners para cerrar una puerta al agotar presupuesto.")]
    public GameObject ClosedRoom;

    [Tooltip("Lista runtime de habitaciones del piso/tanda actual (se rellena en RegisterRoom).")]
    public List<GameObject> rooms = new List<GameObject>();

    [Header("Presupuesto inicial (solo si no lo resetea FloorManager)")]
    [Tooltip("Presupuesto por tanda si nadie lo ajusta. FloorManager normalmente llama ResetBudget().")]
    public int maxRooms = 15;

    [Header("Tienda")]
    [Tooltip("Fuerza una tienda cada N salas colocadas dentro de la tanda actual (N=3 -> 3,6,9,...)")]
    public int shopEvery = 3;

    [Header("Spawns finales (opcional)")]
    public GameObject boss;      // Debe estar en Resources (mismo nombre) y tener PhotonView
    public GameObject enemy;     // Ídem
    public int minRooms = 5;

    // ---------- Estado interno (por tanda/piso) ----------
    int _budgetLeft = 0;     // cuánto queda por colocar en ESTA tanda
    int _placedCount = 0;    // cuántas salas normales ya se colocaron en ESTA tanda
    int _lastShopTick = -999;

    void Awake()
    {
        // Si nadie resetea el presupuesto (p. ej., FloorManager al empezar la tanda),
        // dejamos un presupuesto inicial para que funcione en modo suelto.
        ResetBudget(maxRooms);
    }

    // =====================================================
    // API de presupuesto por tanda (usada por RoomSpawner / FloorManager)
    // =====================================================

    /// <summary>Resetea presupuesto para una nueva tanda/piso. Limpia contadores y la lista de rooms.</summary>
    public void ResetBudget(int newMax)
    {
        _budgetLeft = Mathf.Max(0, newMax);
        _placedCount = 0;
        _lastShopTick = -999;

        // Limpia solo la lista (FloorManager debería destruir objetos si corresponde)
        rooms.Clear();
    }

    /// <summary>Alias cómodo si prefieres llamarlo así desde otros sitios.</summary>
    public void ResetForNewFloor()
    {
        ResetBudget(maxRooms);
    }

    /// <summary>Intenta consumir 1 de presupuesto para colocar una sala normal.</summary>
    public bool TryConsumeRoomBudget()
    {
        if (_budgetLeft <= 0) return false;
        _budgetLeft--;
        _placedCount++;
        return true;
    }

    /// <summary>¿Queda presupuesto en la tanda actual?</summary>
    public bool HasBudgetLeft => _budgetLeft > 0;

    /// <summary>Cuando instancias una sala (normal/tienda/cap), llama a esto para llevar la lista al día.</summary>
    public void RegisterRoom(GameObject go)
    {
        if (go == null) return;
        if (!rooms.Contains(go)) rooms.Add(go);
    }

    // =====================================================
    // Lógica de tienda
    // =====================================================

    /// <summary>Devuelve true si en esta colocación debe ir una tienda (según shopEvery).</summary>
    public bool ShouldForceShopThisTick()
    {
        if (shopEvery <= 0) return false;
        if (_placedCount <= 0) return false;
        if (_placedCount % shopEvery != 0) return false;
        if (_lastShopTick == _placedCount) return false; // evita doble disparo en el mismo tick
        _lastShopTick = _placedCount;
        return true;
    }

    /// <summary>Devuelve la variante de tienda correcta para el lado que está abierto.</summary>
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

    // =====================================================
    // Utilidades varias
    // =====================================================

    /// <summary>¿Es “seguro” cerrar (p. ej., para OnTriggerEnter de dos spawners que chocan)?</summary>
    public bool CanCloseRoom()
    {
        return rooms != null && rooms.Count >= minRooms;
    }

    /// <summary>Spawns finales de ejemplo (si quieres lanzar jefe/enemigos cuando acabas un piso).</summary>
    public void SpawnFinals()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (rooms == null || rooms.Count == 0) return;

        // Boss en la última
        if (boss)
            PhotonNetwork.InstantiateRoomObject(boss.name, rooms[rooms.Count - 1].transform.position, Quaternion.identity);

        // Mobs en el resto
        if (enemy)
        {
            for (int i = 0; i < rooms.Count - 1; i++)
                PhotonNetwork.InstantiateRoomObject(enemy.name, rooms[i].transform.position, Quaternion.identity);
        }
    }
}
