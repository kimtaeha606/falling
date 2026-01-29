using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private int count = 10;
    [SerializeField] private Vector3 startPosition = new Vector3(22.23317f, -49.88329f, 59.60209f);
    [SerializeField] private float stepY = -80f;
    [SerializeField] private Transform wallParent;

    private void Start()
    {
        if (wallPrefab == null || count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = startPosition + new Vector3(0f, stepY * i, 0f);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity);
            if (wallParent != null)
            {
                wall.transform.SetParent(wallParent, worldPositionStays: true);
            }
        }
    }
}
