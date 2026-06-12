using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour
{
    public static GameLogic Instance { get; private set; }

    public int maxHits = 5;
    public int currentHits;
    public int score;
    public float elapsedTime;
    public string gameOverSceneName = "GameOver";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    public void AddScore(int amount = 1)
    {
        score += Mathf.Max(0, amount);
    }

    public void RegisterPlayerHit(int amount = 1)
    {
        currentHits += Mathf.Max(1, amount);
        if (currentHits >= maxHits)
        {
            GameOver();
        }
    }

    public float HealthNormalized()
    {
        if (maxHits <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01((maxHits - currentHits) / (float)maxHits);
    }

    public void GameOver()
    {
        if (!string.IsNullOrWhiteSpace(gameOverSceneName))
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
