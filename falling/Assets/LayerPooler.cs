using System.Collections.Generic;
using UnityEngine;

public class LayerPooler : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private FloorLayer layerPrefab;
    [SerializeField] private Transform player;

    [Header("Pool")]
    [SerializeField] private int poolCount = 30;
    [SerializeField] private float layerGap = 5f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [Header("Recycle")]
    [SerializeField] private float recycleAbovePlayer = 15f;

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
        if (player == null)
        {
            Debug.LogError("[LayerPooler] player is null.", this);
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
        // NOTE: FloorLayer size=10, 2x2 구멍이면 시작 좌표는 0..8
        // 지금은 단순 랜덤. 다음 단계에서:
        // - lastHole 기반 반경 제한
        // - 플레이어 낙하속도/이동속도 기반 도달가능 범위
        // 로 교체하면 됨.

        int x = Random.Range(0, 9);
        int z = Random.Range(0, 9);

        lastHoleX = x;
        lastHoleZ = z;

        return new Vector2Int(x, z);
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
