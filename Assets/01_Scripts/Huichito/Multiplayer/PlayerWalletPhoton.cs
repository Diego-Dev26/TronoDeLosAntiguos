using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerWalletPhoton : MonoBehaviour, IPunObservable
{
    public int coins = 0;

    PhotonView _pv;

    void Awake()
    {
        _pv = GetComponent<PhotonView>();
    }

    public void AddCoins(int amount)
    {
        if (!_pv.IsMine) return;
        coins = Mathf.Max(0, coins + amount);
    }

    public bool TrySpend(int price)
    {
        if (!_pv.IsMine) return false; // autoridad local sobre su billetera
        if (price <= 0) return true;
        if (coins < price) return false;
        coins -= price;
        return true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
            stream.SendNext(coins);
        else
            coins = (int)stream.ReceiveNext();
    }
}
