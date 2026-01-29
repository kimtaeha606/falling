using UnityEngine;

public class GameOverUIController : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject scoreUI;

    private void OnEnable()
    {
        GameSignals.GameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameSignals.GameOver -= OnGameOver;
    }

    private void OnGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (scoreUI != null)
        {
            scoreUI.SetActive(false);
        }
    }
}
