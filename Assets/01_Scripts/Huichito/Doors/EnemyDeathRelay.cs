using UnityEngine;

public class EnemyDeathRelay : MonoBehaviour
{
    public System.Action OnDied;

    // Llamado desde otro script (Health/Enemy) cuando muere:
    public void NotifyDeath() => OnDied?.Invoke();

    void OnDestroy()
    {
        // Fallback: si el enemigo se destruye sin notificar
        OnDied?.Invoke();
    }
}
