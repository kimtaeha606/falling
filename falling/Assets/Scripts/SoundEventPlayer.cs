using UnityEngine;

public class SoundEventPlayer : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip loopClip;      // BGM
    [SerializeField] private AudioClip gameOverClip;  // GameOver SFX

    private AudioSource loopSource;
    private AudioSource gameOverSource;

    private void Awake()
    {
        // BGM용 AudioSource 생성
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.clip = loopClip;
        loopSource.loop = true;
        loopSource.playOnAwake = false;
        loopSource.spatialBlend = 0f; // 2D

        // GameOver용 AudioSource 생성
        gameOverSource = gameObject.AddComponent<AudioSource>();
        gameOverSource.clip = gameOverClip;
        gameOverSource.loop = false;
        gameOverSource.playOnAwake = false;
        gameOverSource.spatialBlend = 0f; // 2D
    }

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
        if (loopClip == null) return;

        if (!loopSource.isPlaying)
            loopSource.Play();
    }

    private void HandleGameOver()
    {
        // BGM 중단
        if (loopSource.isPlaying)
            loopSource.Stop();

        if (gameOverClip == null) return;

        // 항상 처음부터 재생
        gameOverSource.Stop();
        gameOverSource.Play();
    }
}
