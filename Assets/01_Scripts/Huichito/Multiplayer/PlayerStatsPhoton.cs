using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerStatsPhoton : MonoBehaviour, IPunObservable
{
    [Header("Base")]
    public float baseDamage = 10f;
    public float baseMaxHealth = 100f;
    public float baseMoveSpeed = 5f;
    public float baseAttackSpeed = 1f;

    [Header("Actual")]
    public float damage;
    public float maxHealth;
    public float moveSpeed;
    public float attackSpeed;
    public float currentHealth;

    PhotonView _pv;

    void Awake()
    {
        _pv = GetComponent<PhotonView>();
        ResetToBase();
    }

    public void ResetToBase()
    {
        damage = baseDamage;
        maxHealth = baseMaxHealth;
        moveSpeed = baseMoveSpeed;
        attackSpeed = baseAttackSpeed;
        currentHealth = maxHealth;
    }

    // Aplicar un delta puntual (local)
    public void ApplyDelta(StatDelta d)
    {
        switch (d.stat)
        {
            case StatType.Damage: damage += d.amount; break;
            case StatType.MaxHealth: maxHealth += d.amount; currentHealth = Mathf.Min(currentHealth + d.amount, maxHealth); break;
            case StatType.MoveSpeed: moveSpeed += d.amount; break;
            case StatType.AttackSpeed: attackSpeed += d.amount; break;
        }
    }

    // Aplicar un ItemData localmente (se invoca desde RPC o local)
    public void ApplyItem(ItemData item)
    {
        if (item == null) return;
        for (int i = 0; i < item.deltas.Count; i++)
            ApplyDelta(item.deltas[i]);
    }

    // Tomar daño/curar con autoridad local
    public void TakeDamage(float dmg)
    {
        if (!_pv.IsMine) return;
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Max(0f, dmg), 0f, maxHealth);
    }
    public void Heal(float amount)
    {
        if (!_pv.IsMine) return;
        currentHealth = Mathf.Clamp(currentHealth + Mathf.Max(0f, amount), 0f, maxHealth);
    }

    // === Sincronización ===
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(damage);
            stream.SendNext(maxHealth);
            stream.SendNext(moveSpeed);
            stream.SendNext(attackSpeed);
            stream.SendNext(currentHealth);
        }
        else
        {
            damage = (float)stream.ReceiveNext();
            maxHealth = (float)stream.ReceiveNext();
            moveSpeed = (float)stream.ReceiveNext();
            attackSpeed = (float)stream.ReceiveNext();
            currentHealth = (float)stream.ReceiveNext();
        }
    }

    [PunRPC]
    public void RPC_ApplyItemByKey(string itemKey)
    {
        var item = ItemRegistry.Instance ? ItemRegistry.Instance.Get(itemKey) : null;
        Debug.Log($"[Stats] RPC_ApplyItemByKey('{itemKey}') -> {(item ? item.displayName : "NULL")}");
        ApplyItem(item);
    }

}
