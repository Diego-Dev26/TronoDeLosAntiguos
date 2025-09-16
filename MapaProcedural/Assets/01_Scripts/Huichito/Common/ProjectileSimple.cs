using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileSimple : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 12f;
    public float life = 4f;

    [Tooltip("True = daña solo a 'Enemy'. False = daña solo a 'Player'.")]
    public bool onlyDamageEnemies = true;

    void Start() { Destroy(gameObject, life); }

    void Update() { transform.position += transform.forward * speed * Time.deltaTime; }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;

        // filtro de objetivo
        if (onlyDamageEnemies)
        {
            if (!other.CompareTag("Enemy")) return;
        }
        else
        {
            if (!other.CompareTag("Player")) return;
        }

        var hp = other.GetComponentInParent<Health>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
            Destroy(gameObject);
        }
        else
        {
            // chocó con algo sin Health del objetivo esperado
            Destroy(gameObject);
        }
    }
}
