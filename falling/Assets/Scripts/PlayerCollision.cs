using UnityEngine;
public class PlayerCollision : MonoBehaviour
{
    private bool isDead;

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (collision.collider.TryGetComponent<Obstacle>(out var obstacle))
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        GameSignals.RaiseGameOver();
    }
}
