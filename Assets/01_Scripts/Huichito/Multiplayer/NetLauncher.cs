using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetLauncher : MonoBehaviourPunCallbacks
{
    public string playerPrefabName = "Ghurin";

    void Start()
    {
        Debug.Log("[NET] Start -> ConnectUsingSettings");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "0.1";
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        string roomCode = "HUICHO-123"; // pon el código que quieras o léelo de un input
        PhotonNetwork.JoinOrCreateRoom(roomCode,
            new RoomOptions { MaxPlayers = 4 },
            TypedLobby.Default);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogWarning($"[NET] JoinRandomFailed ({returnCode}) '{message}' -> CreateRoom");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[NET] JoinedRoom -> Spawn player");

        // Usa tus puntos de spawn
        Vector3 pos = NetSpawnPoints.GetSpawn();
        Quaternion rot = Quaternion.identity; // o mira hacia tu forward deseado

        PhotonNetwork.Instantiate(playerPrefabName, pos, rot);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"[NET] Disconnected: {cause}");
    }
}
