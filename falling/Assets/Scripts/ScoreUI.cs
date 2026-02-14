using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text yText;
    private float currentScore;
    private bool hasScore;

    public float CurrentScore => currentScore;
    public bool HasScore => hasScore;

    private void OnEnable()
    {
        GameSignals.PlayerYChanged += HandlePlayerYChanged;
    }

    private void OnDisable()
    {
        GameSignals.PlayerYChanged -= HandlePlayerYChanged;
    }

    private void HandlePlayerYChanged(float y)
    {
        currentScore = y;
        hasScore = true;

        if (yText == null)
        {
            return;
        }

        yText.text = $"{y}m";
    }
}
