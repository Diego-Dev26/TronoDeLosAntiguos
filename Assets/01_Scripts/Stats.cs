using UnityEngine;

public enum StatType
{
    Damage,
    MaxHealth,
    MoveSpeed,
    AttackSpeed
}

[System.Serializable]
public class StatDelta
{
    public StatType stat;
    public float amount;
}
