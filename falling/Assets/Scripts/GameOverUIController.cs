using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverUIController : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject scoreUI;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private MonoBehaviour[] disableOnGameOver;

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
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
        {
            playerInput.DeactivateInput();
        }

        if (disableOnGameOver != null)
        {
            for (int i = 0; i < disableOnGameOver.Length; i++)
            {
                if (disableOnGameOver[i] != null)
                {
                    disableOnGameOver[i].enabled = false;
                }
            }
        }

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
