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

    [Header("Player Motion (Constant)")]
    [SerializeField] private float fallSpeed = 80f;   // y 등속 낙하(절댓값)
    [SerializeField] private float moveSpeed = 12;   // xz 등속 이동(최대)
    [SerializeField] private float cellSize = 1f;    // FloorLayer와 동일
    [SerializeField] private int gridSize = 10;      // 10x10
    [SerializeField] private float reachableSlack = 0.85f; // 여유 계수
    [SerializeField] private float minRadiusCells = 1.0f;  // 최소 반경(너무 빡세면 보정)


    


    private readonly List<FloorLayer> layers = new();

    // 현재는 단순 랜덤(다음 단계에서 속도/이동 기반으로 교체)
    private int lastHoleX = 4;
    private int lastHoleZ = 4;

    private void Start()
    {
        InitializePool();
    }

    private void Update()
    {
        UpdateRecycle();
    }

    // 1) InitializePool
    // When: Start()에서 1회
    // Effects: poolCount만큼 층 생성 + 초기 배치 + 구멍 적용
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

        // 중복 초기화 방지(필요 없으면 제거 가능)
        if (layers.Count > 0) return;

        // 플레이어 시작 y 근처부터 아래로 깔아둠
        for (int i = 0; i < poolCount; i++)
        {
            FloorLayer layer = Instantiate(layerPrefab, transform);
            float y = origin.y - i * layerGap;
            layer.transform.position = new Vector3(origin.x, y, origin.z);

            // FloorLayer는 Awake에서 Initialize를 하지만, 명시적으로 호출해도 안전
            layer.Initialize();

            Vector2Int hole = PickNextHole();
            layer.ApplyHole2x2(hole.x, hole.y);

            layers.Add(layer);
        }
    }

    // 2) UpdateRecycle
    // When: 매 프레임 Update()
    // Effects: 지나간 층(플레이어보다 충분히 위)을 찾아 아래로 보내 재활용
    public void UpdateRecycle()
    {
        if (player == null || layers.Count == 0) return;

        float py = player.position.y;

        for (int i = 0; i < layers.Count; i++)
        {
            FloorLayer layer = layers[i];
            if (layer == null) continue;

            // 플레이어보다 위로 충분히 올라간 층 = 이미 지나간 층
            if (layer.transform.position.y > py + recycleAbovePlayer)
            {
                RecycleLayer(layer);
            }
        }
    }

    // 3) RecycleLayer
    // When: UpdateRecycle에서 조건 충족 시
    // Effects: 가장 아래층보다 더 아래로 이동 + 새 구멍 적용
    public void RecycleLayer(FloorLayer layer)
    {
        if (layer == null) return;
        if (layers.Count == 0) return;

        float minY = GetMinLayerY();
        float newY = minY - layerGap;

        layer.transform.position = new Vector3(origin.x, newY, origin.z);

        Vector2Int hole = PickNextHole();
        layer.ApplyHole2x2(hole.x, hole.y);
    }

    // 4) PickNextHole
    // When: 새 층(초기/재활용)에 구멍 배치할 때마다
    // Return: (x,z) where x=0..8, z=0..8 (2x2 구멍의 좌상단)
    public Vector2Int PickNextHole()
    {
        int max = gridSize - 2; // 2x2 구멍 좌상단: 0..8

        float H = Mathf.Max(0.01f, layerGap);
        float fs = Mathf.Max(0.01f, fallSpeed);
        float ms = Mathf.Max(0.0f, moveSpeed);

        // 1) 다음 층까지 걸리는 시간(등속)
        float t = H / fs;

        // 2) 수평으로 움직일 수 있는 최대 거리
        float R = ms * t * reachableSlack;

        // 3) 셀 반경으로 변환
        float rCells = R / Mathf.Max(0.001f, cellSize);
        rCells = Mathf.Max(rCells, minRadiusCells);

        // 4) 이전 구멍 주변 반경 내 후보 수집
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

        // 5) 후보 없으면 전체 랜덤(안전장치)
        Vector2Int chosen;
        if (candidates.Count == 0)
            chosen = new Vector2Int(Random.Range(0, max + 1), Random.Range(0, max + 1));
        else
            chosen = candidates[Random.Range(0, candidates.Count)];

        lastHoleX = chosen.x;
        lastHoleZ = chosen.y;
        return chosen;
    }

    // 5) GetMinLayerY
    // When: 재활용 층을 어디로 보낼지 결정할 때
    // Return: 현재 풀에서 가장 아래(가장 작은 y)의 층 y값
    public float GetMinLayerY()
    {
        float minY = float.PositiveInfinity;

        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] == null) continue;
            minY = Mathf.Min(minY, layers[i].transform.position.y);
        }

        // 전부 null인 극단 케이스 방어
        if (float.IsPositiveInfinity(minY))
            minY = origin.y;

        return minY;
    }
}
