using UnityEngine;

public class FloorLayer : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int size = 10;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float blockHeight = 1f;

    [Header("Hole Toggle")]
    [SerializeField] private bool disableRendererForHole = true;  // 구멍이면 보이지 않게
    [SerializeField] private bool disableColliderForHole = true;  // 구멍이면 밟을 수 없게

    


    // blocks[x,z]
    private GameObject[,] blocks;
    private Collider[,] cols;
    private Renderer[,] rens;

    private bool initialized;

    // 현재 구멍(2x2)의 좌상단 셀(디버그/조회용)
    private int holeX = -1;
    private int holeZ = -1;

    // 1) Initialize: (프리팹 없이) 10x10 블록 생성
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
    }

    private GameObject CreateBlock(int x, int z) 
    {
        // 기본 큐브 생성(메시+콜라이더 포함)
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Block_{x}_{z}";
        go.transform.SetParent(transform, worldPositionStays: false);

        // 셀 중앙에 배치(층 원점 기준)
        go.transform.localPosition = new Vector3((x + 0.5f) * cellSize, 0f, (z + 0.5f) * cellSize);
        go.transform.localScale = new Vector3(cellSize, blockHeight, cellSize);

        // 기존 프리팹에 붙이던 특성 통합
        go.AddComponent<Obstacle>();            // instantKill 기본 true
        go.AddComponent<RandomBlockMaterial>(); // Awake에서 랜덤 컬러

        // (선택) 레이어로도 구분하면 Player 판정이 단순해짐
        // go.layer = LayerMask.NameToLayer("Ground");

        return go;
    }

    private void Awake()
    {
        Initialize();
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

    // 3) ApplyHole2x2: 구멍 2x2 적용
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

    

    

}
