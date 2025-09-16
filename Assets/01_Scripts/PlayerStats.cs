using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float damage = 10f;
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float attackSpeed = 1f;
    public float currentHealth = 100f;

    public void Apply(ItemData item)
    {
        if (item == null) return;
        for (int i = 0; i < item.deltas.Count; i++)
        {
            var d = item.deltas[i];
            switch (d.stat)
            {
                case StatType.Damage: damage += d.amount; break;
                case StatType.MaxHealth: maxHealth += d.amount; currentHealth = Mathf.Min(currentHealth + d.amount, maxHealth); break;
                case StatType.MoveSpeed: moveSpeed += d.amount; break;
                case StatType.AttackSpeed: attackSpeed += d.amount; break;
            }
        }
    }
}