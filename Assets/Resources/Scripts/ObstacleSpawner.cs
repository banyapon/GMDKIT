using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float minSpawnInterval = 2f;
    public float maxSpawnInterval = 10f;
    public float spawnZ = 18f;
    public float viewportPadding = 0.1f;

    private float nextSpawnTime;

    private void Start()
    {
        if (obstaclePrefab == null)
        {
            obstaclePrefab = Resources.Load<GameObject>("obstacle/obstacle");
        }

        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (obstaclePrefab == null || Time.time < nextSpawnTime)
        {
            return;
        }

        SpawnObstacle();
        ScheduleNextSpawn();
    }

    private void SpawnObstacle()
    {
        if (Camera.main == null)
        {
            return;
        }

        float viewportX = Random.Range(viewportPadding, 1f - viewportPadding);
        float viewportY = Random.Range(viewportPadding, 1f - viewportPadding);
        Vector3 worldPosition = Camera.main.ViewportToWorldPoint(new Vector3(viewportX, viewportY, spawnZ));
        worldPosition.z = spawnZ;
        Instantiate(obstaclePrefab, worldPosition, Quaternion.identity);
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(Mathf.Max(0.1f, minSpawnInterval), Mathf.Max(minSpawnInterval, maxSpawnInterval));
    }
}
