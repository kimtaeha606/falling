using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text yText;

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
        if (yText == null)
        {
            return;
        }

        yText.text = $"{y}m";
    }
}
