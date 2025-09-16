using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(PhotonView))]
public class ShopPlate : MonoBehaviour, IPunObservable
{
    [Header("Item Config")]
    public ItemData item;
    public Transform displayPoint;
    public bool oneTimePurchase = true;

    [Header("Debug")]
    public bool debugLogs = true;

    PhotonView _pv;
    GameObject _displayInstance;
    int _price = -1;
    bool _sold = false;

    public int Price => _price;
    public bool Sold => _sold;

    void Awake()
    {
        _pv = GetComponent<PhotonView>();

        // Collider trigger obligatorio
        var col = GetComponent<Collider>();
        if (col == null) col = gameObject.AddComponent<BoxCollider>();
        col.isTrigger = true;

        // Asegurar Rigidbody KINEMÁTICO en la PLACA (no toca al player)
        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
    void Start()
    {
        if (debugLogs) Debug.Log($"[ShopPlate] Start {name} | Master={PhotonNetwork.IsMasterClient} | Item={(item ? item.key : "NULL")}");

        if (PhotonNetwork.IsMasterClient && item != null && _price < 0)
        {
            _price = item.RollPrice();
            if (debugLogs) Debug.Log($"[ShopPlate] Master set price {_price} for {item.key}");
            _pv.RPC(nameof(RpcSetPrice), RpcTarget.AllBuffered, _price);
        }

        ForceInitVisual();
        ApplySoldVisual();
    }

    void OnTriggerEnter(Collider other)
    {
        if (debugLogs) Debug.Log($"[ShopPlate] OnTriggerEnter by {other.name}, sold={_sold}, item={(item ? item.key : "NULL")}");
        if (_sold || item == null) return;

        if (!other.CompareTag("Player"))
        {
            if (debugLogs) Debug.Log("[ShopPlate] Ignorado: no es Player");
            return;
        }

        var buyerPV = other.GetComponent<PhotonView>();
        if (buyerPV == null)
        {
            if (debugLogs) Debug.LogWarning("[ShopPlate] Player sin PhotonView en el mismo GO.");
            return;
        }
        if (!buyerPV.IsMine)
        {
            if (debugLogs) Debug.Log("[ShopPlate] Ignorado: PhotonView no es mío (no soy el owner).");
            return;
        }

        if (debugLogs) Debug.Log($"[ShopPlate] Solicitando compra al Master. buyerViewId={buyerPV.ViewID}");
        _pv.RPC(nameof(RpcRequestBuy), RpcTarget.MasterClient, buyerPV.ViewID);
    }

    [PunRPC]
    void RpcSetPrice(int price)
    {
        _price = price;
        if (debugLogs) Debug.Log($"[ShopPlate] RpcSetPrice -> {price}");
    }

    [PunRPC]
    void RpcRequestBuy(int buyerViewId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (_sold || item == null || _price < 0)
        {
            if (debugLogs) Debug.Log($"[ShopPlate] Master rechaza: sold={_sold} item={(item ? item.key : "NULL")} price={_price}");
            return;
        }

        var buyerPV = PhotonView.Find(buyerViewId);
        if (buyerPV == null)
        {
            if (debugLogs) Debug.LogWarning("[ShopPlate] Master: buyerPV NULL");
            return;
        }

        var wallet = buyerPV.GetComponent<PlayerWalletPhoton>();
        if (wallet == null)
        {
            if (debugLogs) Debug.LogWarning("[ShopPlate] Master: buyer sin PlayerWalletPhoton");
            return;
        }

        if (wallet.coins >= _price)
        {
            wallet.coins -= _price;
            if (debugLogs) Debug.Log($"[ShopPlate] Master CONFIRMA compra {item.key} por {_price} a {buyerPV.name}");
            _pv.RPC(nameof(RpcConfirmBuy), RpcTarget.AllBuffered, buyerViewId);
        }
        else
        {
            if (debugLogs) Debug.Log($"[ShopPlate] Master: fondos insuficientes ({wallet.coins} < {_price})");
            buyerPV.RPC(nameof(RpcNotifyNotEnoughCoins), buyerPV.Owner, _price, item ? item.displayName : "Item");
        }
    }

    [PunRPC]
    void RpcConfirmBuy(int buyerViewId)
    {
        if (_sold) return;
        _sold = true;
        ApplySoldVisual();

        var buyerPV = PhotonView.Find(buyerViewId);
        if (buyerPV == null) return;

        // ? Buscar el componente de stats aunque esté en un hijo
        var statsComp = buyerPV.GetComponentInChildren<PlayerStatsPhoton>(true);
        PhotonView statsView = null;
        if (statsComp != null) statsView = statsComp.GetComponent<PhotonView>();

        // Fallback: si no hay PV en ese hijo, usar el del player (no ideal, pero evita silencio)
        if (statsView == null) statsView = buyerPV;

        if (!string.IsNullOrEmpty(item.key))
        {
            // ? Enviar el RPC al PhotonView correcto (el del objeto donde vive PlayerStatsPhoton)
            statsView.RPC("RPC_ApplyItemByKey", RpcTarget.All, item.key);
        }

        if (buyerPV.IsMine)
        {
            var hud = FindObjectOfType<PlayerHUDPhoton>();
            if (hud) hud.ShowStatus($"Comprado: {item.displayName}");
        }

        Debug.Log($"[Shop] {buyerPV.gameObject.name} compró {item.displayName} por {_price} monedas.");
    }


    [PunRPC]
    void RpcNotifyNotEnoughCoins(int price, string itemName)
    {
        var hud = FindObjectOfType<PlayerHUDPhoton>();
        if (hud) hud.ShowStatus($"No alcanza: {itemName} ({price}c)");
        if (debugLogs) Debug.Log($"[ShopPlate] Not enough coins for {itemName} ({price})");
    }

    public void ForceInitVisual()
    {
        if (item != null && item.displayPrefab && displayPoint && _displayInstance == null)
            _displayInstance = Instantiate(item.displayPrefab, displayPoint.position, displayPoint.rotation, displayPoint);
    }

    void ApplySoldVisual()
    {
        if (_displayInstance) _displayInstance.SetActive(!_sold);
        var col = GetComponent<Collider>();
        if (col) col.enabled = !_sold || !oneTimePurchase;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_price);
            stream.SendNext(_sold);
        }
        else
        {
            _price = (int)stream.ReceiveNext();
            _sold = (bool)stream.ReceiveNext();
            ApplySoldVisual();
        }
    }
}