// /Scripts/Enemies/EnemySpawnerArea.cs
using UnityEngine;

public class EnemySpawnerArea : MonoBehaviour
{
    public Vector2 size = new Vector2(20, 20);
    public GameObject[] enemyPrefabs; // arrastra aquí goblin, archer, golem, summoner
    public int totalToSpawn = 8;
    public float y = 1f; // altura sobre el plane

    public bool spawnOnStart = true;

    void Start() { if (spawnOnStart) Spawn(); }

    [ContextMenu("Spawn")]
    public void Spawn()
    {
        for (int i = 0; i < totalToSpawn; i++)
        {
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            Vector2 r = new Vector2(
                Random.Range(-size.x * 0.5f, size.x * 0.5f),
                Random.Range(-size.y * 0.5f, size.y * 0.5f)
            );
            Vector3 pos = transform.position + new Vector3(r.x, y, r.y);
            Instantiate(prefab, pos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.1f, size.y));
    }
}
