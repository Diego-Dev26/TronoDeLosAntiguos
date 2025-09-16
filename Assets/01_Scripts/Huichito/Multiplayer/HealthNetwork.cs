// HealthNetwork.cs
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class HealthNetwork : MonoBehaviourPun
{
    public Health health;
    void Awake() { if (!health) health = GetComponent<Health>(); }

    [PunRPC] public void RPC_TakeDamage(float dmg) { health.TakeDamage(dmg); }
    public void TakeDamageNetwork(float dmg)
    {
        photonView.RPC("RPC_TakeDamage", RpcTarget.AllBuffered, dmg);
    }
}
