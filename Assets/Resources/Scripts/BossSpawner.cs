using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    public GameObject bossPrefab;
    public bool useScoreTrigger = true;
    public bool useTimeTrigger;
    public int scoreTrigger = 20;
    public float timeTrigger = 30f;

    private bool hasSpawned;

    private void Start()
    {
        if (bossPrefab == null)
        {
            bossPrefab = Resources.Load<GameObject>("boss/boss");
        }
    }

    private void Update()
    {
        if (hasSpawned || GameLogic.Instance == null || bossPrefab == null)
        {
            return;
        }

        bool scoreReady = useScoreTrigger && GameLogic.Instance.score >= scoreTrigger;
        bool timeReady = useTimeTrigger && GameLogic.Instance.elapsedTime >= timeTrigger;

        if (scoreReady || timeReady)
        {
            GameObject bossObject = Instantiate(bossPrefab, transform.position, Quaternion.identity);
            Boss boss = bossObject.GetComponent<Boss>();
            if (boss != null)
            {
                boss.useScoreTrigger = useScoreTrigger;
                boss.useTimeTrigger = useTimeTrigger;
                boss.scoreTrigger = scoreTrigger;
                boss.timeTrigger = timeTrigger;
                boss.maxHealth = 50;
                boss.fixedZPosition = 15f;
            }

            hasSpawned = true;
        }
    }
}
