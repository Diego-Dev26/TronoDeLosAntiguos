using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FloorPlate : MonoBehaviourPunCallbacks
{
    public RoomActivator shopRoom;

    void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
        if (!shopRoom) shopRoom = GetComponentInParent<RoomActivator>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!shopRoom) return;

        // Abrir puertas de la tienda para dejarte salir
        if (PhotonNetwork.IsMasterClient)
        {
            var pv = shopRoom.GetComponent<PhotonView>();
            if (pv) pv.RPC("RPC_OpenDoors", RpcTarget.All);
        }

        // (Opcional) Señalar al FloorManager si quieres que la tienda actúe como "Exit" entre tandas
        // if (shopRoom && shopRoom == algunaSalaDeSalida) FloorManager.Instance?.NextBatch(shopRoom);
    }
}
