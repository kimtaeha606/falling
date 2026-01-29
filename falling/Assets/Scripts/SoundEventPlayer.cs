using UnityEngine;

public class SoundEventPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource gameOverSource;

    private void OnEnable()
    {
        GameSignals.SoundOn += HandleSoundOn;
        GameSignals.GameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameSignals.SoundOn -= HandleSoundOn;
        GameSignals.GameOver -= HandleGameOver;
    }

    private void HandleSoundOn()
    {
        if (loopSource == null) return;

        loopSource.loop = true;
        if (!loopSource.isPlaying)
        {
            loopSource.Play();
        }
    }

    private void HandleGameOver()
    {
        if (gameOverSource == null) return;

        gameOverSource.loop = false;
        if (!gameOverSource.isPlaying)
        {
            gameOverSource.Play();
        }
    }
}
