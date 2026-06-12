using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public Slider healthSlider;
    public Text scoreText;

    private void Update()
    {
        if (GameLogic.Instance == null)
        {
            return;
        }

        if (healthSlider != null)
        {
            healthSlider.value = GameLogic.Instance.HealthNormalized();
        }

        if (scoreText != null)
        {
            scoreText.text = "Score : " + GameLogic.Instance.score;
        }
    }
}
