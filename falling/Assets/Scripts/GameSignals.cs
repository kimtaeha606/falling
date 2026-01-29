using UnityEngine;
using UnityEngine.Events;

public class GameSignals : MonoBehaviour
{
    public static event UnityAction GameOver;
    public static event UnityAction<float> PlayerYChanged;
    public static event UnityAction SoundOn;

    public static void RaiseGameOver()
    {
        GameOver?.Invoke();
    }

    public static void RaisePlayerYChanged(float y)
    {
        PlayerYChanged?.Invoke(y);
    }

    public static void RaiseSoundOn()
    {
        SoundOn?.Invoke();
    }
}
