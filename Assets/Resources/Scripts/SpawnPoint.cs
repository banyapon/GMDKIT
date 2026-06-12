using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;
    public bool spawnOnStart = true;
    public GameObject enemyPrefab;

    private float nextSpawnTime;

    private void Start()
    {
        if (enemyPrefab == null)
        {
            enemyPrefab = Resources.Load<GameObject>("enemy/enemy");
        }

        if (spawnOnStart)
        {
            SpawnEnemy();
        }
    }

    private void Update()
    {
        if (enemyPrefab == null)
        {
            return;
        }

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            return;
        }

        GameObject enemyObject = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.followPlayer = Random.value > 0.35f;
        }

        nextSpawnTime = Time.time + Random.Range(Mathf.Max(0.1f, minSpawnInterval), Mathf.Max(minSpawnInterval, maxSpawnInterval));
    }
}
