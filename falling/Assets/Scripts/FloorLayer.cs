using UnityEngine;

public class FloorLayer : MonoBehaviour
{
    public enum HoleType
    {
        Static2x2,
        PulseSize,
        VerticalOscillate,
        HorizontalOscillate
    }

    [Header("Grid")]
    [SerializeField] private int size = 5;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float blockHeight = 1f;

    [Header("Hole Toggle")]
    [SerializeField] private bool disableRendererForHole = true;  // 援щ찉?占쎈㈃ 蹂댁씠吏 ?占쎄쾶
    [SerializeField] private bool disableColliderForHole = true;  // 援щ찉?占쎈㈃ 諛잛쓣 ???占쎄쾶

    [Header("Hole Types")]
    [SerializeField] private float pulseAnimationSpeed = 6f;
    [SerializeField] private int pulseMinSize = 1;
    [SerializeField] private int pulseMaxSize = 4;
    [SerializeField] private int moveRangeCells = 2;
    [SerializeField] private float moveStepInterval = 0.2f;
    [SerializeField] private float moveHandoffDuration = 0.08f;

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
    private int baseHoleX = -1;
    private int baseHoleZ = -1;
    private HoleType activeHoleType = HoleType.Static2x2;
    private bool animateHole;
    private float holeAnimTime;
    private Vector2Int movingCurrent = new Vector2Int(-1, -1);
    private Vector2Int movingPrevious = new Vector2Int(-1, -1);
    private int movingDirection = 1;
    private float moveStepTimer;
    private float moveHandoffTimer;
    private bool moveHandoffActive;

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
        var collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        // go.AddComponent<RandomBlockMaterial>(); // Awake?占쎌꽌 ?占쎈뜡 而щ윭

        // (?占쏀깮) ?占쎌씠?占쎈줈??援щ텇?占쎈㈃ Player ?占쎌젙???占쎌닚?占쎌쭚
        // go.layer = LayerMask.NameToLayer("Ground");

        return go;
    }

    private void Awake()
    {
        EnsureKinematicRigidbody();
        Initialize();
    }

    private void EnsureKinematicRigidbody()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Update()
    {
        if (!animateHole || baseHoleX < 0 || baseHoleZ < 0) return;

        if (activeHoleType == HoleType.PulseSize)
        {
            holeAnimTime += Time.deltaTime;
        }
        else
        {
            UpdateMovingHoleAnimation(Time.deltaTime);
        }

        ApplyHoleMask();
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

        SetAllBlocksActive();

        holeX = -1;
        holeZ = -1;
        baseHoleX = -1;
        baseHoleZ = -1;
        animateHole = false;
        movingCurrent = new Vector2Int(-1, -1);
        movingPrevious = new Vector2Int(-1, -1);
        moveHandoffActive = false;
    }

    private void SetAllBlocksActive()
    {
        for (int z = 0; z < size; z++)
        for (int x = 0; x < size; x++)
        {
            if (disableColliderForHole && cols[x, z] != null) cols[x, z].enabled = true;
            if (disableRendererForHole && rens[x, z] != null) rens[x, z].enabled = true;
        }
    }

    // 3) ApplyHole2x2: 援щ찉 2x2 ?占쎌슜
    public bool ApplyHole2x2(int x, int z)
    {
        return ApplyHole(x, z, HoleType.Static2x2);
    }

    public bool ApplyHole(int x, int z, HoleType holeType)
    {
        if (!initialized) Initialize();

        if (x < 0 || z < 0 || x > size - 2 || z > size - 2)
        {
            Debug.LogWarning($"[FloorLayer] ApplyHole out of range: ({x},{z})", this);
            return false;
        }

        baseHoleX = x;
        baseHoleZ = z;
        activeHoleType = holeType;
        animateHole = holeType != HoleType.Static2x2;
        holeAnimTime = 0f;
        movingDirection = 1;
        moveStepTimer = 0f;
        moveHandoffTimer = 0f;
        moveHandoffActive = false;
        movingCurrent = new Vector2Int(baseHoleX, baseHoleZ);
        movingPrevious = new Vector2Int(-1, -1);

        ApplyHoleMask();
        return true;
    }

    private void ApplyHoleMask()
    {
        SetAllBlocksActive();

        switch (activeHoleType)
        {
            case HoleType.PulseSize:
                ApplyPulseHole();
                break;
            case HoleType.VerticalOscillate:
                ApplyMovingHole();
                break;
            case HoleType.HorizontalOscillate:
                ApplyMovingHole();
                break;
            default:
                ApplySquareHole(baseHoleX, baseHoleZ, 2);
                break;
        }
    }

    private void ApplyPulseHole()
    {
        int minSize = Mathf.Clamp(pulseMinSize, 1, size);
        int maxSize = Mathf.Clamp(pulseMaxSize, minSize, size);
        float t = (Mathf.Sin(holeAnimTime * pulseAnimationSpeed) + 1f) * 0.5f;
        int holeSize = Mathf.RoundToInt(Mathf.Lerp(minSize, maxSize, t));

        float cx = baseHoleX + 0.5f;
        float cz = baseHoleZ + 0.5f;
        int startX = Mathf.RoundToInt(cx - holeSize * 0.5f);
        int startZ = Mathf.RoundToInt(cz - holeSize * 0.5f);

        ApplySquareHole(startX, startZ, holeSize);
    }

    private void ApplyMovingHole()
    {
        if (movingCurrent.x < 0 || movingCurrent.y < 0)
            movingCurrent = new Vector2Int(baseHoleX, baseHoleZ);

        ApplySquareHole(movingCurrent.x, movingCurrent.y, 2);

        if (moveHandoffActive && movingPrevious.x >= 0 && movingPrevious.y >= 0)
            ApplySquareHole(movingPrevious.x, movingPrevious.y, 2, false);
    }

    private void UpdateMovingHoleAnimation(float deltaTime)
    {
        if (moveHandoffActive)
        {
            moveHandoffTimer += deltaTime;
            if (moveHandoffTimer >= Mathf.Max(0f, moveHandoffDuration))
            {
                moveHandoffActive = false;
                movingPrevious = new Vector2Int(-1, -1);
            }
        }

        float interval = Mathf.Max(0.01f, moveStepInterval);
        moveStepTimer += deltaTime;
        while (moveStepTimer >= interval)
        {
            moveStepTimer -= interval;
            AdvanceMovingHoleOneStep();
        }
    }

    private void AdvanceMovingHoleOneStep()
    {
        int range = Mathf.Max(0, moveRangeCells);
        int maxStart = size - 2;
        if (maxStart < 0) return;

        int minX = Mathf.Clamp(baseHoleX, 0, maxStart);
        int maxX = minX;
        int minZ = Mathf.Clamp(baseHoleZ, 0, maxStart);
        int maxZ = minZ;

        if (activeHoleType == HoleType.HorizontalOscillate)
        {
            minX = Mathf.Clamp(baseHoleX - range, 0, maxStart);
            maxX = Mathf.Clamp(baseHoleX + range, 0, maxStart);
        }
        else if (activeHoleType == HoleType.VerticalOscillate)
        {
            minZ = Mathf.Clamp(baseHoleZ - range, 0, maxStart);
            maxZ = Mathf.Clamp(baseHoleZ + range, 0, maxStart);
        }

        Vector2Int current = movingCurrent;
        if (current.x < 0 || current.y < 0)
            current = new Vector2Int(minX, minZ);

        Vector2Int next = current;
        if (activeHoleType == HoleType.HorizontalOscillate)
        {
            int candX = current.x + movingDirection;
            if (candX < minX || candX > maxX)
            {
                movingDirection *= -1;
                candX = current.x + movingDirection;
            }
            next.x = Mathf.Clamp(candX, minX, maxX);
            next.y = Mathf.Clamp(current.y, minZ, maxZ);
        }
        else if (activeHoleType == HoleType.VerticalOscillate)
        {
            int candZ = current.y + movingDirection;
            if (candZ < minZ || candZ > maxZ)
            {
                movingDirection *= -1;
                candZ = current.y + movingDirection;
            }
            next.x = Mathf.Clamp(current.x, minX, maxX);
            next.y = Mathf.Clamp(candZ, minZ, maxZ);
        }

        if (next != current)
        {
            movingPrevious = current;
            movingCurrent = next;
            moveHandoffActive = true;
            moveHandoffTimer = 0f;
        }
    }

    private void ApplySquareHole(int startX, int startZ, int holeSize, bool updateCurrentState = true)
    {
        int clampedSize = Mathf.Clamp(holeSize, 1, size);
        int maxStart = size - clampedSize;

        startX = Mathf.Clamp(startX, 0, maxStart);
        startZ = Mathf.Clamp(startZ, 0, maxStart);

        for (int dz = 0; dz < clampedSize; dz++)
        for (int dx = 0; dx < clampedSize; dx++)
            SetCellHole(startX + dx, startZ + dz, true);

        if (updateCurrentState)
        {
            holeX = startX;
            holeZ = startZ;
        }
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
