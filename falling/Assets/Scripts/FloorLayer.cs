using UnityEngine;

public class FloorLayer : MonoBehaviour
{
    [Header("Grid")]
    [SerializeField] private int size = 10;
    [SerializeField] private float cellSize = 1f;

    // blocks[x,z]
    private GameObject[,] blocks;
    private bool initialized;

    // 현재 구멍(2x2)의 좌상단 셀(디버그/조회용)
    private int holeX = -1;
    private int holeZ = -1;

    // 1) Initialize
    // When: Awake()에서 1회 또는 풀에서 생성 직후 1회
    public void Initialize()
    {
        if (initialized) return;

        blocks = new GameObject[size, size];

        // 전제: 자식 이름이 Block_x_z 형식이거나, 위치 기반으로 매핑 가능해야 함.
        // 여기서는 이름 파싱 방식(가장 단순) 사용.
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i);
            var parts = t.name.Split('_');

            if (parts.Length == 3 &&
                int.TryParse(parts[1], out int x) &&
                int.TryParse(parts[2], out int z) &&
                x >= 0 && x < size &&
                z >= 0 && z < size)
            {
                blocks[x, z] = t.gameObject;
            }
        }

        // 누락 검사(디버그용)
        for (int z = 0; z < size; z++)
        for (int x = 0; x < size; x++)
        {
            if (blocks[x, z] == null)
            {
                Debug.LogWarning(
                    $"[FloorLayer] Missing block at ({x},{z}). " +
                    $"Check child naming: Block_x_z (e.g., Block_3_7).",
                    this);
            }
        }

        initialized = true;
    }

    private void Awake()
    {
        Initialize();
    }

    // 2) ResetAllBlocksActive
    // When: 층 재활용 직후, 새 구멍 적용 전에 호출
    public void ResetAllBlocksActive()
    {
        if (!initialized) Initialize();

        for (int z = 0; z < size; z++)
        for (int x = 0; x < size; x++)
        {
            var b = blocks[x, z];
            if (b != null) b.SetActive(true);
        }

        holeX = -1;
        holeZ = -1;
    }

    // 3) ApplyHole2x2
    // When: 층 배치 직후, 또는 재활용 직후(Reset 후) 호출
    // Input: 2x2 구멍의 "좌상단" 셀 좌표 (x,z)  -> 유효 범위: 0..size-2
    public bool ApplyHole2x2(int x, int z)
    {
        if (!initialized) Initialize();

        if (x < 0 || z < 0 || x > size - 2 || z > size - 2)
        {
            Debug.LogWarning($"[FloorLayer] ApplyHole2x2 out of range: ({x},{z})", this);
            return false;
        }

        ResetAllBlocksActive();

        SetCellActiveSafe(x, z, false);
        SetCellActiveSafe(x + 1, z, false);
        SetCellActiveSafe(x, z + 1, false);
        SetCellActiveSafe(x + 1, z + 1, false);

        holeX = x;
        holeZ = z;

        return true;
    }

    private void SetCellActiveSafe(int x, int z, bool active)
    {
        if (x < 0 || x >= size || z < 0 || z >= size) return;
        var b = blocks[x, z];
        if (b != null) b.SetActive(active);
    }

    // 6) CellToWorldCenter
    // When: 트리거 배치, 디버그 표시, 목표점 계산 등
    // Return: 해당 셀의 "중앙" 월드 좌표
    public Vector3 CellToWorldCenter(int x, int z)
    {
        // 범위 밖이면 가장 가까운 셀로 clamp (실수 방지)
        x = Mathf.Clamp(x, 0, size - 1);
        z = Mathf.Clamp(z, 0, size - 1);

        // FloorLayer의 원점이 (0,0,0)이고 블록이 localPosition = (x*cellSize, 0, z*cellSize) 라는 전제
        Vector3 localCenter = new Vector3((x + 0.5f) * cellSize, 0f, (z + 0.5f) * cellSize);
        return transform.TransformPoint(localCenter);
    }
}
