using System.Collections.Generic;
using UnityEngine;

public class LayerPooler : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private FloorLayer layerPrefab;
    [SerializeField] private Transform player;

    [Header("Pool")]
    [SerializeField] private int poolCount = 4;
    [SerializeField] private float layerGap = 5f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [Header("Recycle")]
    [SerializeField] private float recycleAbovePlayer = 15f;

    [Header("Layer Motion")]
    [SerializeField] private float riseSpeed = 80f;

    [Header("Hole Reachability")]
    [SerializeField] private float moveSpeed = 48f;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int gridSize = 5;
    [SerializeField] private float reachableSlack = 0.85f;
    [SerializeField] private float minRadiusCells = 1.0f;

    private readonly List<FloorLayer> layers = new();

    private int lastHoleX = 4;
    private int lastHoleZ = 4;

    private void Start()
    {
        InitializePool();
    }

    private void Update()
    {
        MoveLayersUp();
        UpdateRecycle();
    }

    public void InitializePool()
    {
        if (layerPrefab == null)
        {
            Debug.LogError("[LayerPooler] layerPrefab is null.", this);
            return;
        }

        if (poolCount <= 0)
        {
            Debug.LogError("[LayerPooler] poolCount must be > 0.", this);
            return;
        }

        if (layerGap <= 0f)
        {
            Debug.LogError("[LayerPooler] layerGap must be > 0.", this);
            return;
        }

        if (layers.Count > 0)
        {
            return;
        }

        for (int i = 0; i < poolCount; i++)
        {
            FloorLayer layer = Instantiate(layerPrefab, transform);
            float y = origin.y - i * layerGap;
            layer.transform.position = new Vector3(origin.x, y, origin.z);

            layer.Initialize();

            Vector2Int hole = PickNextHole(layer.GridSize);
            layer.ApplyHole2x2(hole.x, hole.y);

            layers.Add(layer);
        }
    }

    public void MoveLayersUp()
    {
        if (layers.Count == 0 || riseSpeed <= 0f)
        {
            return;
        }

        float dy = riseSpeed * Time.deltaTime;
        for (int i = 0; i < layers.Count; i++)
        {
            FloorLayer layer = layers[i];
            if (layer == null) continue;

            layer.transform.position += Vector3.up * dy;
        }
    }

    public void UpdateRecycle()
    {
        if (player == null || layers.Count == 0)
        {
            return;
        }

        float py = player.position.y;

        for (int i = 0; i < layers.Count; i++)
        {
            FloorLayer layer = layers[i];
            if (layer == null) continue;

            if (layer.transform.position.y > py + recycleAbovePlayer)
            {
                RecycleLayer(layer);
            }
        }
    }

    public void RecycleLayer(FloorLayer layer)
    {
        if (layer == null || layers.Count == 0)
        {
            return;
        }

        float minY = GetMinLayerY();
        float newY = minY - layerGap;

        layer.transform.position = new Vector3(origin.x, newY, origin.z);

        Vector2Int hole = PickNextHole(layer.GridSize);
        layer.ApplyHole2x2(hole.x, hole.y);
        layer.ApplyRandomLayerColor();
    }

    public Vector2Int PickNextHole(int boardSize)
    {
        int sizeLimit = boardSize > 0 ? boardSize : gridSize;
        int max = sizeLimit - 2;

        float H = Mathf.Max(0.01f, layerGap);
        float fs = Mathf.Max(0.01f, riseSpeed);
        float ms = Mathf.Max(0.0f, moveSpeed);

        float t = H / fs;
        float R = ms * t * reachableSlack;

        float rCells = R / Mathf.Max(0.001f, cellSize);
        rCells = Mathf.Max(rCells, minRadiusCells);

        int minX = Mathf.Clamp(Mathf.FloorToInt(lastHoleX - rCells), 0, max);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(lastHoleX + rCells), 0, max);
        int minZ = Mathf.Clamp(Mathf.FloorToInt(lastHoleZ - rCells), 0, max);
        int maxZ = Mathf.Clamp(Mathf.CeilToInt(lastHoleZ + rCells), 0, max);

        float r2 = rCells * rCells;

        List<Vector2Int> candidates = new List<Vector2Int>(128);
        for (int z = minZ; z <= maxZ; z++)
        for (int x = minX; x <= maxX; x++)
        {
            float dx = x - lastHoleX;
            float dz = z - lastHoleZ;
            if (dx * dx + dz * dz <= r2)
                candidates.Add(new Vector2Int(x, z));
        }

        Vector2Int chosen;
        if (candidates.Count == 0)
            chosen = new Vector2Int(Random.Range(0, max + 1), Random.Range(0, max + 1));
        else
            chosen = candidates[Random.Range(0, candidates.Count)];

        lastHoleX = chosen.x;
        lastHoleZ = chosen.y;
        return chosen;
    }

    public float GetMinLayerY()
    {
        float minY = float.PositiveInfinity;

        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] == null) continue;
            minY = Mathf.Min(minY, layers[i].transform.position.y);
        }

        if (float.IsPositiveInfinity(minY))
            minY = origin.y;

        return minY;
    }
}
