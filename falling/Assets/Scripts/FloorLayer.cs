using UnityEngine;

public class FloorLayer : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int size = 5;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float blockHeight = 1f;

    [Header("Hole Toggle")]
    [SerializeField] private bool disableRendererForHole = true;  // 援щ찉?占쎈㈃ 蹂댁씠吏 ?占쎄쾶
    [SerializeField] private bool disableColliderForHole = true;  // 援щ찉?占쎈㈃ 諛잛쓣 ???占쎄쾶

    [Header("Layer Color (Uniform)")]
    [Tooltip("Random color range (HSV) per layer.")]
    [SerializeField] private Vector2 hueRange = new Vector2(0f, 1f);
    [SerializeField] private Vector2 saturationRange = new Vector2(0.5f, 1f);
    [SerializeField] private Vector2 valueRange = new Vector2(0.6f, 1f);

    [Header("Layer Material")]
    [Tooltip("Base material asset to clone at runtime (keeps shader in build).")]
    [SerializeField] private Material baseMaterial;
    [Tooltip("Shader name to force on the cloned material (e.g., URP Lit).")]
    [SerializeField] private string shaderName = "Universal Render Pipeline/Lit";


    // blocks[x,z]
    private GameObject[,] blocks;
    private Collider[,] cols;
    private Renderer[,] rens;
    private Material layerMaterial;

    private bool initialized;

    // ?占쎌옱 援щ찉(2x2)??醫뚯긽???占??占쎈쾭占?議고쉶??
    private int holeX = -1;
    private int holeZ = -1;

    public int GridSize => size;

    // 1) Initialize: (?占쎈━???占쎌씠) size x size 釉붾줉 ?占쎌꽦
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
        // 湲곕낯 ?占쎈툕 ?占쎌꽦(硫붿떆+肄쒕씪?占쎈뜑 ?占쏀븿)
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Block_{x}_{z}";
        go.transform.SetParent(transform, worldPositionStays: false);

        // ?占?以묒븰??諛곗튂(占??占쎌젏 湲곤옙?)
        go.transform.localPosition = new Vector3((x + 0.5f) * cellSize * 2f, 0f, (z + 0.5f) * cellSize * 2f);
        go.transform.localScale = new Vector3(cellSize * 2f, blockHeight, cellSize * 2f);

        // 湲곗〈 ?占쎈━?占쎌뿉 遺숈씠???占쎌꽦 ?占쏀빀
        go.AddComponent<Obstacle>();            // instantKill 湲곕낯 true
        // go.AddComponent<RandomBlockMaterial>(); // Awake?占쎌꽌 ?占쎈뜡 而щ윭

        // (?占쏀깮) ?占쎌씠?占쎈줈??援щ텇?占쎈㈃ Player ?占쎌젙???占쎌닚?占쎌쭚
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
            Material source = baseMaterial;
            if (source == null)
            {
                for (int z = 0; z < size; z++)
                for (int x = 0; x < size; x++)
                {
                    if (rens[x, z] == null) continue;
                    source = rens[x, z].sharedMaterial;
                    break;
                }
            }

            if (source == null)
            {
                Debug.LogError("[FloorLayer] No base material found. Assign baseMaterial in Inspector.", this);
                return;
            }

            layerMaterial = new Material(source);

            if (!string.IsNullOrEmpty(shaderName))
            {
                var shader = Shader.Find(shaderName);
                if (shader != null)
                    layerMaterial.shader = shader;
                else
                    Debug.LogWarning($"[FloorLayer] Shader not found: {shaderName}", this);
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

    // 2) ResetAllBlocksActive: 援щ찉 蹂듦뎄
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

    // 3) ApplyHole2x2: 援щ찉 2x2 ?占쎌슜
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

        Vector3 localCenter = new Vector3((x + 0.5f) * cellSize * 2f, 0f, (z + 0.5f) * cellSize * 2f);
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

