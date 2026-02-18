using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButton : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private ScoreUI scoreUI;
    [SerializeField] private Button button;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(LoadMainMenuScene);
            button.onClick.AddListener(LoadMainMenuScene);
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(LoadMainMenuScene);
        }
    }

    public void LoadMainMenuScene()
    {
        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("[MainMenuButton] Main menu scene name is empty.", this);
            return;
        }

        PublishScoreEvent();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void PublishScoreEvent()
    {
        if (scoreUI == null)
        {
            scoreUI = FindObjectOfType<ScoreUI>();
        }

        if (scoreUI != null && scoreUI.HasScore)
        {
            GameSignals.RaisePlayerYChanged(scoreUI.CurrentScore);
        }
    }
}
