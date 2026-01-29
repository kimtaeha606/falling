using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class RandomBlockMaterial : MonoBehaviour
{
    [Header("Random Color")]
    [Tooltip("Random color range (HSV).")]
    [SerializeField] private Vector2 hueRange = new Vector2(0f, 1f);
    [SerializeField] private Vector2 saturationRange = new Vector2(0.5f, 1f);
    [SerializeField] private Vector2 valueRange = new Vector2(0.6f, 1f);

    [Tooltip("If checked, apply on Awake")]
    [SerializeField] private bool applyOnAwake = true;

    private Renderer targetRenderer;
    private Material instanceMaterial;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        if (applyOnAwake) ApplyRandomColor();
    }

    [ContextMenu("Apply Random Color")]
    public void ApplyRandomColor()
    {
        if (targetRenderer == null) return;

        instanceMaterial = targetRenderer.material;

        var color = Random.ColorHSV(
            hueRange.x, hueRange.y,
            saturationRange.x, saturationRange.y,
            valueRange.x, valueRange.y
        );

        if (instanceMaterial.HasProperty("_BaseColor"))
            instanceMaterial.SetColor("_BaseColor", color);
        if (instanceMaterial.HasProperty("_Color"))
            instanceMaterial.SetColor("_Color", color);
    }

    private void OnDestroy()
    {
        if (instanceMaterial == null) return;
#if UNITY_EDITOR
        if (!Application.isPlaying)
            DestroyImmediate(instanceMaterial);
        else
            Destroy(instanceMaterial);
#else
        Destroy(instanceMaterial);
#endif
        instanceMaterial = null;
    }
}
