using UnityEngine;
using TMPro;

public class GameOverScoreUI : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TMP_Text scoreText;

    private void Awake()
    {
        UpdateScoreText();
    }
    private void UpdateScoreText()
    {
        if (scoreText == null)
        {
            return;
        }

        float score = 0f;
        if (scoreManager != null && scoreManager.HasScore)
        {
            score = scoreManager.LastScore;
        }

        scoreText.text = $"{score}m";
    }
}
