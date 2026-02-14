using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private ScoreUI scoreUI;

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
