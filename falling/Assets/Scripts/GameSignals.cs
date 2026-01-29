using UnityEngine;
using UnityEngine.Events;

public class GameSignals : MonoBehaviour
{
    public static event UnityAction GameOver;

    public static void RaiseGameOver()
    {
        GameOver?.Invoke();
    }
}
