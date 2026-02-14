using UnityEngine;
using UnityEngine.Events;

public class GameSignals : MonoBehaviour
{
    public static event UnityAction GameOver;
    public static event UnityAction<float> PlayerYChanged;
    public static event UnityAction<float> HighScoreUpdated;
    public static event UnityAction SoundOn;

    public static void RaiseGameOver()
    {
        Debug.Log("게임오버 호출됨");
        GameOver?.Invoke();
    }

    public static void RaisePlayerYChanged(float y)
    {
        PlayerYChanged?.Invoke(y);
    }

    public static void RaiseHighScoreUpdated(float score)
    {
        HighScoreUpdated?.Invoke(score);
    }

    public static void RaiseSoundOn()
    {
        SoundOn?.Invoke();
    }
}
