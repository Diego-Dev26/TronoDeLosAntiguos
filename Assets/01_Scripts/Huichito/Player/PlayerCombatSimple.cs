using UnityEngine;

public class PlayerCombatSimple : MonoBehaviour
{
    [Header("Referencias")]
    public Transform muzzle;                // Empty frente (0,1.4,0.9)
    public GameObject projectilePrefab;     // asigna el prefab

    [Header("Melee")]
    public bool enableMelee = true;         // <- NUEVO
    public float meleeDamage = 20f;
    public float meleeRange = 1.7f;
    public float meleeRadius = 1.0f;
    public float meleeCooldown = 0.5f;
    float meleeCd;

    [Header("Ranged")]
    public bool enableShoot = false;        // <- NUEVO
    public bool fireOnLeftClick = false;    // <- NUEVO (true = dispara con LMB)
    public float projectileDamage = 26f;
    public float projectileSpeed = 22f;
    public float projectileLife = 4f;
    public float shootCooldown = 0.8f;
    float shootCd;

    void Update()
    {
        meleeCd -= Time.deltaTime;
        shootCd -= Time.deltaTime;

        // MELEE en LMB si está habilitado
        if (enableMelee && Input.GetMouseButtonDown(0) && meleeCd <= 0f)
        {
            DoMelee();
            meleeCd = meleeCooldown;
        }

        // DISPARO: LMB o RMB según fireOnLeftClick, si está habilitado
        bool firePressed = fireOnLeftClick ? Input.GetMouseButtonDown(0) : Input.GetMouseButtonDown(1);
        if (enableShoot && firePressed && shootCd <= 0f)
        {
            Shoot();
            shootCd = shootCooldown;
        }
    }

    void DoMelee()
    {
        Vector3 origin = transform.position + transform.forward * 0.9f + Vector3.up * 1.0f;
        var hits = Physics.SphereCastAll(origin, meleeRadius, transform.forward, meleeRange);
        foreach (var h in hits)
        {
            if (!h.collider.CompareTag("Enemy")) continue;
            var hp = h.collider.GetComponentInParent<Health>();
            if (hp) hp.TakeDamage(meleeDamage);
        }
        // Debug.DrawLine(origin, origin + transform.forward * (meleeRange + meleeRadius), Color.red, 0.2f);
    }

    void Shoot()
    {
        if (!projectilePrefab || !muzzle) return;

        // Offset pequeño para que no choque contigo al salir
        Vector3 spawnPos = muzzle.position + muzzle.forward * 0.25f;

        var go = Instantiate(projectilePrefab, spawnPos, transform.rotation);
        var pr = go.GetComponent<ProjectileSimple>();
        if (pr)
        {
            pr.damage = projectileDamage;
            pr.speed = projectileSpeed;
            pr.life = projectileLife;
            pr.onlyDamageEnemies = true;
        }
    }
}
