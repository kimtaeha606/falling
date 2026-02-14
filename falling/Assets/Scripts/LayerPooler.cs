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
    [SerializeField] private float fallSpeed = 80f;   // y ?깆냽 ?숉븯(?덈뙎媛?
    [SerializeField] private float moveSpeed = 48f;   // xz ?깆냽 ?대룞(理쒕?)
    [SerializeField] private float cellSize = 1f;    // FloorLayer? ?숈씪
    [SerializeField] private int gridSize = 5;      // 5x5
    [SerializeField] private float reachableSlack = 0.85f; // ?ъ쑀 怨꾩닔
    [SerializeField] private float minRadiusCells = 1.0f;  // 理쒖냼 諛섍꼍(?덈Т 鍮≪꽭硫?蹂댁젙)


    


    private readonly List<FloorLayer> layers = new();

    // ?꾩옱???⑥닚 ?쒕뜡(?ㅼ쓬 ?④퀎?먯꽌 ?띾룄/?대룞 湲곕컲?쇰줈 援먯껜)
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
    // When: Start()?먯꽌 1??
    // Effects: poolCount留뚰겮 痢??앹꽦 + 珥덇린 諛곗튂 + 援щ찉 ?곸슜
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

        // 以묐났 珥덇린??諛⑹?(?꾩슂 ?놁쑝硫??쒓굅 媛??
        if (layers.Count > 0) return;

        // ?뚮젅?댁뼱 ?쒖옉 y 洹쇱쿂遺???꾨옒濡?源붿븘??
        for (int i = 0; i < poolCount; i++)
        {
            FloorLayer layer = Instantiate(layerPrefab, transform);
            float y = origin.y - i * layerGap;
            layer.transform.position = new Vector3(origin.x, y, origin.z);

            // FloorLayer??Awake?먯꽌 Initialize瑜??섏?留? 紐낆떆?곸쑝濡??몄텧?대룄 ?덉쟾
            layer.Initialize();

            Vector2Int hole = PickNextHole(layer.GridSize);
            layer.ApplyHole2x2(hole.x, hole.y);

            layers.Add(layer);
        }
    }

    // 2) UpdateRecycle
    // When: 留??꾨젅??Update()
    // Effects: 吏?섍컙 痢??뚮젅?댁뼱蹂대떎 異⑸텇??????李얠븘 ?꾨옒濡?蹂대궡 ?ы솢??
    public void UpdateRecycle()
    {
        if (player == null || layers.Count == 0) return;

        float py = player.position.y;

        for (int i = 0; i < layers.Count; i++)
        {
            FloorLayer layer = layers[i];
            if (layer == null) continue;

            // ?뚮젅?댁뼱蹂대떎 ?꾨줈 異⑸텇???щ씪媛?痢?= ?대? 吏?섍컙 痢?
            if (layer.transform.position.y > py + recycleAbovePlayer)
            {
                RecycleLayer(layer);
            }
        }
    }

    // 3) RecycleLayer
    // When: UpdateRecycle?먯꽌 議곌굔 異⑹” ??
    // Effects: 媛???꾨옒痢듬낫?????꾨옒濡??대룞 + ??援щ찉 ?곸슜
    public void RecycleLayer(FloorLayer layer)
    {
        if (layer == null) return;
        if (layers.Count == 0) return;

        float minY = GetMinLayerY();
        float newY = minY - layerGap;

        layer.transform.position = new Vector3(origin.x, newY, origin.z);

        Vector2Int hole = PickNextHole(layer.GridSize);
        layer.ApplyHole2x2(hole.x, hole.y);
        layer.ApplyRandomLayerColor();
    }

    // 4) PickNextHole
    // When: ??痢?珥덇린/?ы솢????援щ찉 諛곗튂???뚮쭏??
    // Return: (x,z) where x=0..gridSize-1, z=0..gridSize-1
    public Vector2Int PickNextHole(int boardSize)
    {
        int sizeLimit = boardSize > 0 ? boardSize : gridSize;
        int max = sizeLimit - 2; // 2x2 hole top-left: 0..size-2

        float H = Mathf.Max(0.01f, layerGap);
        float fs = Mathf.Max(0.01f, fallSpeed);
        float ms = Mathf.Max(0.0f, moveSpeed);

        // 1) ?ㅼ쓬 痢듦퉴吏 嫄몃━???쒓컙(?깆냽)
        float t = H / fs;

        // 2) ?섑룊?쇰줈 ?吏곸씪 ???덈뒗 理쒕? 嫄곕━
        float R = ms * t * reachableSlack;

        // 3) ? 諛섍꼍?쇰줈 蹂??
        float rCells = R / Mathf.Max(0.001f, cellSize);
        rCells = Mathf.Max(rCells, minRadiusCells);

        // 4) ?댁쟾 援щ찉 二쇰? 諛섍꼍 ???꾨낫 ?섏쭛
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

        // 5) ?꾨낫 ?놁쑝硫??꾩껜 ?쒕뜡(?덉쟾?μ튂)
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
    // When: ?ы솢??痢듭쓣 ?대뵒濡?蹂대궪吏 寃곗젙????
    // Return: ?꾩옱 ??먯꽌 媛???꾨옒(媛???묒? y)??痢?y媛?
    public float GetMinLayerY()
    {
        float minY = float.PositiveInfinity;

        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] == null) continue;
            minY = Mathf.Min(minY, layers[i].transform.position.y);
        }

        // ?꾨? null??洹밸떒 耳?댁뒪 諛⑹뼱
        if (float.IsPositiveInfinity(minY))
            minY = origin.y;

        return minY;
    }
}



