using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;
    [Range(0f, 0.9f)] public float damageReduction = 0f;

    [Header("Runtime")]
    public float currentHP;
    float invulnerableUntil = 0f; // <-- i-frames

    public System.Action OnDeath;

    void Awake() { currentHP = maxHP; }

    public void TakeDamage(float amount)
    {
        if (Time.time < invulnerableUntil) return; // i-frames activos
        float final = Mathf.Max(0.1f, amount * (1f - damageReduction));
        currentHP = Mathf.Max(0f, currentHP - final);
        if (currentHP <= 0f) Die();
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }

    public void SetInvulnerableFor(float seconds) // <-- llamar al esquivar
    {
        invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);
    }

    void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 p = transform.position + Vector3.up * 2f;
        float t = Application.isPlaying ? currentHP / Mathf.Max(1f, maxHP) : 1f;
        Gizmos.DrawLine(p + Vector3.left * 0.5f, p + Vector3.left * 0.5f + Vector3.right * t);
    }
}
