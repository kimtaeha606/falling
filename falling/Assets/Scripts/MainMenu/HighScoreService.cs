using UnityEngine;

public static class HighScoreService
{
    private const string HighScoreKey = "HighScore";
    private static bool initialized;

    public static float SavedHighScore => PlayerPrefs.GetFloat(HighScoreKey, 0f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        GameSignals.PlayerYChanged += HandlePlayerYChanged;
        initialized = true;
    }

    private static void HandlePlayerYChanged(float score)
    {
        float saved = SavedHighScore;
        if (score >= saved)
        {
            return;
        }

        PlayerPrefs.SetFloat(HighScoreKey, score);
        PlayerPrefs.Save();
        GameSignals.RaiseHighScoreUpdated(score);
    }
}
