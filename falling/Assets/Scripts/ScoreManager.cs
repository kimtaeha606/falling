using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private float startScore = 500f;
    [SerializeField] private float fallSpeedPerSecond = 8f;

    private float lastScore;
    private bool hasScore;
    private bool isGameOver;

    public float LastScore => lastScore;
    public bool HasScore => hasScore;

    private void Awake()
    {
        lastScore = startScore;
        hasScore = true;
        GameSignals.RaisePlayerYChanged(lastScore);
    }

    private void OnEnable()
    {
        GameSignals.GameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameSignals.GameOver -= HandleGameOver;
    }

    private void Update()
    {
        if (isGameOver || fallSpeedPerSecond <= 0f)
        {
            return;
        }

        float nextScore = lastScore - fallSpeedPerSecond * Time.deltaTime;
        if (Mathf.Approximately(nextScore, lastScore))
        {
            return;
        }

        lastScore = nextScore;
        GameSignals.RaisePlayerYChanged(lastScore);
    }

    private void HandleGameOver()
    {
        isGameOver = true;
    }
}
