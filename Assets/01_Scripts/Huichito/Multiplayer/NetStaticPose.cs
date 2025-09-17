using Photon.Pun;
using UnityEngine;

public class NetStaticPose : MonoBehaviourPun
{
    [PunRPC]
    void RPC_SetPose(Vector3 p, Quaternion r)
    {
        transform.SetPositionAndRotation(p, r);
    }

    // Llamar solo desde el master después de alinear la sala.
    public void BroadcastPose()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        photonView.RPC("RPC_SetPose", RpcTarget.OthersBuffered,
                       transform.position, transform.rotation);
    }
}
