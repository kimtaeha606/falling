using UnityEngine;
public class PlayerCollision : MonoBehaviour
{
    private bool isDead;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

        var other = hit.collider;

        if (other.TryGetComponent<SoundTrigger>(out _))
        {
            GameSignals.RaiseSoundOn();
        }

        if (other.TryGetComponent<Obstacle>(out _))
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
