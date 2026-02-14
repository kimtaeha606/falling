using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private int count = 10;
    [SerializeField] private Vector3 startPosition = new Vector3(22.23317f, -49.88329f, 59.60209f);
    [SerializeField] private float stepY = -80f;
    [SerializeField] private Transform wallParent;
    [SerializeField] private Transform player;
    [SerializeField] private float firstRecycleY = -40f;
    [SerializeField] private float recycleStep = 40f;

    private Transform[] walls;
    private float nextRecycleY;

    private void Start()
    {
        if (wallPrefab == null || count <= 0) return;

        nextRecycleY = firstRecycleY;
        walls = new Transform[count];
        int i = 0;
        while (i < count)
        {
            Vector3 pos = startPosition + new Vector3(0f, stepY * i, 0f);
            GameObject wall = Instantiate(wallPrefab, pos, Quaternion.identity);
            if (wallParent != null)
            {
                wall.transform.SetParent(wallParent, worldPositionStays: true);
            }

            walls[i] = wall.transform;
            i++;
        }
    }

    private void Update()
    {
        if (player == null || walls == null || walls.Length == 0) return;
        if (recycleStep <= 0f) return;

        while (player.position.y <= nextRecycleY)
        {
            float highestY = walls[0].position.y;
            float lowestY = walls[0].position.y;
            int highestIndex = 0;

            for (int i = 1; i < walls.Length; i++)
            {
                float y = walls[i].position.y;
                if (y > highestY)
                {
                    highestY = y;
                    highestIndex = i;
                }
                if (y < lowestY)
                {
                    lowestY = y;
                }
            }

            Transform t = walls[highestIndex];
            Vector3 pos = t.position;
            pos.y = lowestY - recycleStep;
            t.position = pos;

            nextRecycleY -= recycleStep;
        }
    }
}
