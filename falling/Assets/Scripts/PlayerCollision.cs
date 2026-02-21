using UnityEngine;
public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private LayerMask obstacleMask = ~0;

    private readonly Collider[] overlapBuffer = new Collider[16];
    private bool isDead;

    public bool IsDead => isDead;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    private void OnEnable()
    {
        // Explicit reset for scene reload / re-enable paths.
        isDead = false;
    }

    private void Update()
    {
        if (isDead || controller == null)
        {
            return;
        }

        CheckObstacleOverlap();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (isDead || other == null)
        {
            return;
        }

        if (other.TryGetComponent<Obstacle>(out _))
        {
            TryDieFromObstacle();
        }
    }

    private void CheckObstacleOverlap()
    {
        Bounds bounds = controller.bounds;
        float radius = Mathf.Max(0.01f, controller.radius * 0.95f);
        Vector3 center = bounds.center;
        Vector3 pointA = new Vector3(center.x, bounds.min.y + radius, center.z);
        Vector3 pointB = new Vector3(center.x, bounds.max.y - radius, center.z);

        int count = Physics.OverlapCapsuleNonAlloc(
            pointA,
            pointB,
            radius,
            overlapBuffer,
            obstacleMask,
            QueryTriggerInteraction.Collide
        );

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapBuffer[i];
            if (col == null || col.transform == transform) continue;

            if (col.TryGetComponent<Obstacle>(out _))
            {
                TryDieFromObstacle();
                break;
            }
        }
    }

    public void TryDieFromObstacle()
    {
        if (isDead)
        {
            return;
        }

        Die();
    }

    void Die()
    {
        isDead = true;
        GameSignals.RaiseGameOver();
    }
}
