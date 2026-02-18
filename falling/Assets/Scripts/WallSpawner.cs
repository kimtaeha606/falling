using System.Collections;
using UnityEngine;

public class WallSpawner : MonoBehaviour
{
    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private Vector3 startPosition = new Vector3(22.23317f, -49.88329f, 59.60209f);
    [SerializeField] private float stepY = -80f;
    [SerializeField] private float spawnInterval = 6f;
    [SerializeField] private Transform wallParent;

    private Vector3 nextSpawnPosition;
    private Coroutine spawnRoutine;

    private void Start()
    {
        if (wallPrefab == null)
        {
            return;
        }

        nextSpawnPosition = startPosition;
        SpawnTwoWalls();
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnTwoWalls();
        }
    }

    private void SpawnTwoWalls()
    {
        SpawnSingleWall();
        SpawnSingleWall();
    }

    private void SpawnSingleWall()
    {
        GameObject wall = Instantiate(wallPrefab, nextSpawnPosition, Quaternion.identity);

        if (wallParent != null)
        {
            wall.transform.SetParent(wallParent, worldPositionStays: true);
        }

        nextSpawnPosition.y += stepY;
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }
}
