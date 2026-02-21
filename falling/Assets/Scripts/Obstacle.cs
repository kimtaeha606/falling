using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public bool instantKill = true;

    private void OnTriggerEnter(Collider other)
    {
        TryKillPlayer(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryKillPlayer(other);
    }

    private void TryKillPlayer(Collider other)
    {
        if (!instantKill || other == null)
        {
            return;
        }

        PlayerCollision playerCollision = other.GetComponentInParent<PlayerCollision>();
        if (playerCollision == null)
        {
            return;
        }

        playerCollision.TryDieFromObstacle();
    }
}
