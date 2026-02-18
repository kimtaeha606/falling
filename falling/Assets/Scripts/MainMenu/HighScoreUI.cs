using TMPro;
using UnityEngine;

public class HighScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private string prefix = "HighScore: ";
    [SerializeField] private string suffix = "m";

    private void Awake()
    {
        if (highScoreText == null)
        {
            highScoreText = GetComponent<TMP_Text>();
        }

        UpdateText(HighScoreService.SavedHighScore);
    }

    private void OnEnable()
    {
        GameSignals.HighScoreUpdated += HandleHighScoreUpdated;
    }

    private void OnDisable()
    {
        GameSignals.HighScoreUpdated -= HandleHighScoreUpdated;
    }

    private void HandleHighScoreUpdated(float score)
    {
        UpdateText(score);
    }

    private void UpdateText(float score)
    {
        if (highScoreText == null)
        {
            return;
        }

        highScoreText.text = $"{prefix}{Mathf.RoundToInt(score)}{suffix}";
    }
}
