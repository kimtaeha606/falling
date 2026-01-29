using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    private float lastY;
    private bool hasLastY;

    public float LastScore => lastY;
    public bool HasScore => hasLastY;

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        float y = player.position.y;
        if (!hasLastY || !Mathf.Approximately(y, lastY))
        {
            lastY = y;
            hasLastY = true;
            GameSignals.RaisePlayerYChanged(y);
        }
    }
}
