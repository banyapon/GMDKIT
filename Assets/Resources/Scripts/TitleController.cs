using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    public string sceneName = "Game";
    public KeyCode enterKey = KeyCode.Return;
    public Button startButton;

    private void Start()
    {
        if (startButton == null)
        {
            GameObject buttonObject = GameObject.Find("StartButton");
            if (buttonObject != null)
            {
                startButton = buttonObject.GetComponent<Button>();
            }
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveListener(LoadScene);
            startButton.onClick.AddListener(LoadScene);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(enterKey) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            LoadScene();
        }
    }

    public void LoadScene()
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
