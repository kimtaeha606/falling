using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class StandardCanvasCreator
{
    [MenuItem("Tools/Stat UI Kit/Create Standard Canvases")]
    public static void CreateStandardCanvases()
    {
        // 1. EventSystem 보장
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // 2. UIRoot 생성 또는 찾기
        GameObject uiRoot = GameObject.Find("UIRoot");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UIRoot");
        }

        // 3. Overlay Canvas
        CreateOrFixCanvas(
            uiRoot.transform,
            "Canvas_Overlay",
            RenderMode.ScreenSpaceOverlay,
            sortingOrder: 0,
            isWorld: false
        );

        // 4. Window Canvas
        CreateOrFixCanvas(
            uiRoot.transform,
            "Canvas_Window",
            RenderMode.ScreenSpaceOverlay,
            sortingOrder: 100,
            isWorld: false
        );

        // 5. World Canvas
        CreateOrFixCanvas(
            uiRoot.transform,
            "Canvas_World",
            RenderMode.WorldSpace,
            sortingOrder: 0,
            isWorld: true
        );

        Debug.Log("Standard Canvases created / fixed.");
    }

    private static void CreateOrFixCanvas(
        Transform parent,
        string name,
        RenderMode renderMode,
        int sortingOrder,
        bool isWorld
    )
    {
        GameObject go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name);
            go.transform.SetParent(parent);
        }

        Canvas canvas = GetOrAdd<Canvas>(go);
        CanvasScaler scaler = GetOrAdd<CanvasScaler>(go);
        GetOrAdd<GraphicRaycaster>(go);

        canvas.renderMode = renderMode;
        canvas.sortingOrder = sortingOrder;

        if (isWorld)
        {
            canvas.worldCamera = Camera.main;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }
        else
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null)
            comp = go.AddComponent<T>();
        return comp;
    }
}
