using UnityEngine;

public class FloorLayer : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int size = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float blockHeight = 1f;

    [Header("Hole Toggle")]
    [SerializeField] private bool disableRendererForHole = true;  // 구멍?�면 보이지 ?�게
    [SerializeField] private bool disableColliderForHole = true;  // 구멍?�면 밟을 ???�게

    [Header("Layer Color (Uniform)")]
    [Tooltip("Random color range (HSV) per layer.")]
    [SerializeField] private Vector2 hueRange = new Vector2(0f, 1f);
    [SerializeField] private Vector2 saturationRange = new Vector2(0.5f, 1f);
    [SerializeField] private Vector2 valueRange = new Vector2(0.6f, 1f);

    // blocks[x,z]
    private GameObject[,] blocks;
    private Collider[,] cols;
    private Renderer[,] rens;
    private Material layerMaterial;

    private bool initialized;

    // ?�재 구멍(2x2)??좌상???�(?�버�?조회??
    private int holeX = -1;
    private int holeZ = -1;

    // 1) Initialize: (?�리???�이) 10x10 블록 ?�성
    public void Initialize()
    {
        if (initialized) return;

        blocks = new GameObject[size, size];
        cols   = new Collider[size, size];
        rens   = new Renderer[size, size];

        for (int z = 0; z < size; z++)
        for (int x = 0; x < size; x++)
        {
            var go = CreateBlock(x, z);
            blocks[x, z] = go;
            cols[x, z] = go.GetComponent<Collider>();
            rens[x, z] = go.GetComponent<Renderer>();
        }

        initialized = true;
        ApplyRandomLayerColor();
    }

    private GameObject CreateBlock(int x, int z)
    {
        // 기본 ?�브 ?�성(메시+콜라?�더 ?�함)
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Block_{x}_{z}";
        go.transform.SetParent(transform, worldPositionStays: false);

        // ?� 중앙??배치(�??�점 기�?)
        go.transform.localPosition = new Vector3((x + 0.5f) * cellSize, 0f, (z + 0.5f) * cellSize);
        go.transform.localScale = new Vector3(cellSize, blockHeight, cellSize);

        // 기존 ?�리?�에 붙이???�성 ?�합
        go.AddComponent<Obstacle>();            // instantKill 기본 true
        // go.AddComponent<RandomBlockMaterial>(); // Awake?�서 ?�덤 컬러

        // (?�택) ?�이?�로??구분?�면 Player ?�정???�순?�짐
        // go.layer = LayerMask.NameToLayer("Ground");

        return go;
    }

    private void Awake()
    {
        Initialize();
    }

    public void ApplyRandomLayerColor()
    {
        if (!initialized)
        {
            Initialize();
            return;
        }
        if (rens == null) return;

        if (layerMaterial == null)
        {
            for (int z = 0; z < size; z++)
            for (int x = 0; x < size; x++)
            {
                if (rens[x, z] == null) continue;
                layerMaterial = new Material(rens[x, z].sharedMaterial);
                break;
            }
        }

        if (layerMaterial == null) return;

        var color = Random.ColorHSV(
            hueRange.x, hueRange.y,
            saturationRange.x, saturationRange.y,
            valueRange.x, valueRange.y
        );

        if (layerMaterial.HasProperty("_BaseColor"))
            layerMaterial.SetColor("_BaseColor", color);
        if (layerMaterial.HasProperty("_Color"))
            layerMaterial.SetColor("_Color", color);

        for (int z = 0; z < size; z++)
        for (int x = 0; x < size; x++)
        {
            if (rens[x, z] == null) continue;
            rens[x, z].sharedMaterial = layerMaterial;
        }
    }

    // 2) ResetAllBlocksActive: 구멍 복구
    public void ResetAllBlocksActive()
    {
        if (!initialized) Initialize();

        for (int z = 0; z < size; z++)
        for (int x = 0; x < size; x++)
        {
            if (disableColliderForHole && cols[x, z] != null) cols[x, z].enabled = true;
            if (disableRendererForHole && rens[x, z] != null) rens[x, z].enabled = true;
        }

        holeX = -1;
        holeZ = -1;
    }

    // 3) ApplyHole2x2: 구멍 2x2 ?�용
    public bool ApplyHole2x2(int x, int z)
    {
        if (!initialized) Initialize();

        if (x < 0 || z < 0 || x > size - 2 || z > size - 2)
        {
            Debug.LogWarning($"[FloorLayer] ApplyHole2x2 out of range: ({x},{z})", this);
            return false;
        }

        ResetAllBlocksActive();

        SetCellHole(x, z, true);
        SetCellHole(x + 1, z, true);
        SetCellHole(x, z + 1, true);
        SetCellHole(x + 1, z + 1, true);

        holeX = x;
        holeZ = z;
        return true;
    }

    private void SetCellHole(int x, int z, bool isHole)
    {
        if (x < 0 || x >= size || z < 0 || z >= size) return;

        if (disableColliderForHole && cols[x, z] != null) cols[x, z].enabled = !isHole;
        if (disableRendererForHole && rens[x, z] != null) rens[x, z].enabled = !isHole;
    }

    // 6) CellToWorldCenter
    public Vector3 CellToWorldCenter(int x, int z)
    {
        x = Mathf.Clamp(x, 0, size - 1);
        z = Mathf.Clamp(z, 0, size - 1);

        Vector3 localCenter = new Vector3((x + 0.5f) * cellSize, 0f, (z + 0.5f) * cellSize);
        return transform.TransformPoint(localCenter);
    }
    private void OnDestroy()
    {
        if (layerMaterial == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(layerMaterial);
        else
            Destroy(layerMaterial);
#else
        Destroy(layerMaterial);
#endif
        layerMaterial = null;
    }

}

