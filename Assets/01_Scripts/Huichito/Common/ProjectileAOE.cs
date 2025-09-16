using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileAOE : MonoBehaviour
{
    public float speed = 16f;
    public float damage = 22f;
    public float life = 4f;

    [Header("Explosión")]
    public float radius = 2.5f;
    public bool onlyDamageEnemies = true; // true: daña Enemy; false: daña Player

    void Start() { Destroy(gameObject, life); }
    void Update() { transform.position += transform.forward * speed * Time.deltaTime; }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        Explode();
    }

    void Explode()
    {
        foreach (var c in Physics.OverlapSphere(transform.position, radius))
        {
            if (onlyDamageEnemies ? !c.CompareTag("Enemy") : !c.CompareTag("Player")) continue;
            var hp = c.GetComponentInParent<Health>();
            if (hp) hp.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
