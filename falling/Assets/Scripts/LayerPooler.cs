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
    [SerializeField] private float fallSpeed = 80f;   // y ?±ì† ?™í•˜(?ˆëŒ“ê°?
    [SerializeField] private float moveSpeed = 48f;   // xz ?±ì† ?´ë™(ìµœë?)
    [SerializeField] private float cellSize = 1f;    // FloorLayer?€ ?™ì¼
    [SerializeField] private int gridSize = 10;      // 10x10
    [SerializeField] private float reachableSlack = 0.85f; // ?¬ìœ  ê³„ìˆ˜
    [SerializeField] private float minRadiusCells = 1.0f;  // ìµœì†Œ ë°˜ê²½(?ˆë¬´ ë¹¡ì„¸ë©?ë³´ì •)


    


    private readonly List<FloorLayer> layers = new();

    // ?„ì¬???¨ìˆœ ?œë¤(?¤ìŒ ?¨ê³„?ì„œ ?ë„/?´ë™ ê¸°ë°˜?¼ë¡œ êµì²´)
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
    // When: Start()?ì„œ 1??
    // Effects: poolCountë§Œí¼ ì¸??ì„± + ì´ˆê¸° ë°°ì¹˜ + êµ¬ë© ?ìš©
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

        // ì¤‘ë³µ ì´ˆê¸°??ë°©ì?(?„ìš” ?†ìœ¼ë©??œê±° ê°€??
        if (layers.Count > 0) return;

        // ?Œë ˆ?´ì–´ ?œì‘ y ê·¼ì²˜ë¶€???„ë˜ë¡?ê¹”ì•„??
        for (int i = 0; i < poolCount; i++)
        {
            FloorLayer layer = Instantiate(layerPrefab, transform);
            float y = origin.y - i * layerGap;
            layer.transform.position = new Vector3(origin.x, y, origin.z);

            // FloorLayer??Awake?ì„œ Initializeë¥??˜ì?ë§? ëª…ì‹œ?ìœ¼ë¡??¸ì¶œ?´ë„ ?ˆì „
            layer.Initialize();

            Vector2Int hole = PickNextHole();
            layer.ApplyHole2x2(hole.x, hole.y);

            layers.Add(layer);
        }
    }

    // 2) UpdateRecycle
    // When: ë§??„ë ˆ??Update()
    // Effects: ì§€?˜ê°„ ì¸??Œë ˆ?´ì–´ë³´ë‹¤ ì¶©ë¶„??????ì°¾ì•„ ?„ë˜ë¡?ë³´ë‚´ ?¬í™œ??
    public void UpdateRecycle()
    {
        if (player == null || layers.Count == 0) return;

        float py = player.position.y;

        for (int i = 0; i < layers.Count; i++)
        {
            FloorLayer layer = layers[i];
            if (layer == null) continue;

            // ?Œë ˆ?´ì–´ë³´ë‹¤ ?„ë¡œ ì¶©ë¶„???¬ë¼ê°?ì¸?= ?´ë? ì§€?˜ê°„ ì¸?
            if (layer.transform.position.y > py + recycleAbovePlayer)
            {
                RecycleLayer(layer);
            }
        }
    }

    // 3) RecycleLayer
    // When: UpdateRecycle?ì„œ ì¡°ê±´ ì¶©ì¡± ??
    // Effects: ê°€???„ë˜ì¸µë³´?????„ë˜ë¡??´ë™ + ??êµ¬ë© ?ìš©
    public void RecycleLayer(FloorLayer layer)
    {
        if (layer == null) return;
        if (layers.Count == 0) return;

        float minY = GetMinLayerY();
        float newY = minY - layerGap;

        layer.transform.position = new Vector3(origin.x, newY, origin.z);

        Vector2Int hole = PickNextHole();
        layer.ApplyHole2x2(hole.x, hole.y);
        layer.ApplyRandomLayerColor();
    }

    // 4) PickNextHole
    // When: ??ì¸?ì´ˆê¸°/?¬í™œ????êµ¬ë© ë°°ì¹˜???Œë§ˆ??
    // Return: (x,z) where x=0..8, z=0..8 (2x2 êµ¬ë©??ì¢Œìƒ??
    public Vector2Int PickNextHole()
    {
        int max = gridSize - 2; // 2x2 êµ¬ë© ì¢Œìƒ?? 0..8

        float H = Mathf.Max(0.01f, layerGap);
        float fs = Mathf.Max(0.01f, fallSpeed);
        float ms = Mathf.Max(0.0f, moveSpeed);

        // 1) ?¤ìŒ ì¸µê¹Œì§€ ê±¸ë¦¬???œê°„(?±ì†)
        float t = H / fs;

        // 2) ?˜í‰?¼ë¡œ ?€ì§ì¼ ???ˆëŠ” ìµœë? ê±°ë¦¬
        float R = ms * t * reachableSlack;

        // 3) ?€ ë°˜ê²½?¼ë¡œ ë³€??
        float rCells = R / Mathf.Max(0.001f, cellSize);
        rCells = Mathf.Max(rCells, minRadiusCells);

        // 4) ?´ì „ êµ¬ë© ì£¼ë? ë°˜ê²½ ???„ë³´ ?˜ì§‘
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

        // 5) ?„ë³´ ?†ìœ¼ë©??„ì²´ ?œë¤(?ˆì „?¥ì¹˜)
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
    // When: ?¬í™œ??ì¸µì„ ?´ë””ë¡?ë³´ë‚¼ì§€ ê²°ì •????
    // Return: ?„ì¬ ?€?ì„œ ê°€???„ë˜(ê°€???‘ì? y)??ì¸?yê°?
    public float GetMinLayerY()
    {
        float minY = float.PositiveInfinity;

        for (int i = 0; i < layers.Count; i++)
        {
            if (layers[i] == null) continue;
            minY = Mathf.Min(minY, layers[i].transform.position.y);
        }

        // ?„ë? null??ê·¹ë‹¨ ì¼€?´ìŠ¤ ë°©ì–´
        if (float.IsPositiveInfinity(minY))
            minY = origin.y;

        return minY;
    }
}
