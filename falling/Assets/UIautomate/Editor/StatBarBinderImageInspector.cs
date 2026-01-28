#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[CustomEditor(typeof(Image), true)]
[CanEditMultipleObjects]
public class StatBarBinderImageInspector : ImageEditor
{
    private GameObject _sourceGameObject;
    private Component _sourceComponent;
    private string _eventName;
    private string _unityEventName;
    private string _getterName;

    public override void OnInspectorGUI()
    {
        // 기존 Image 인스펙터(기본 UI Image 옵션들)
        base.OnInspectorGUI();

        // 멀티 선택은 일단 제외(원하면 확장 가능)
        if (targets.Length != 1) return;

        var img = (Image)target;
        var go = img.gameObject;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("[ Stat Bar Binder ]", EditorStyles.boldLabel);

        // Fill Image : (자동) -> 현재 Image를 Fill로 쓰는 게 UX상 자연스러움
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.ObjectField("Fill Image", img, typeof(Image), true);
            if (GUILayout.Button("Auto", GUILayout.Width(60)))
            {
                EnsureFilled(img);
            }
        }

        // Source : (드래그)
        // Source GameObject
        _sourceGameObject = (GameObject)EditorGUILayout.ObjectField("Source", _sourceGameObject, typeof(GameObject), true);

        // Component from Source GameObject
        _sourceComponent = DrawComponentDropdown(_sourceGameObject, _sourceComponent);

        // C# Event : (??????) - source ??? ???????? ???
        DrawEventDropdown(_sourceComponent);

        // UnityEvent fields
        DrawUnityEventDropdown(_sourceComponent);

        // Getter (optional)
        DrawGetterDropdown(_sourceComponent);

        using (new EditorGUI.DisabledScope(_sourceComponent == null))
        {
            if (GUILayout.Button("[ Bind ]", GUILayout.Height(28)))
            {
                Bind(go, img, _sourceComponent, _eventName, _unityEventName, _getterName);
            }
        }
    }

    private void EnsureFilled(Image img)
    {
        Undo.RecordObject(img, "Set Image Filled");
        img.type = Image.Type.Filled;
        EditorUtility.SetDirty(img);
    }

    private Component DrawComponentDropdown(GameObject go, Component current)
    {
        if (go == null)
        {
            EditorGUILayout.Popup("Component", 0, new[] { "(None)" });
            return null;
        }

        var comps = go.GetComponents<Component>().Where(c => c != null).ToArray();
        if (comps.Length == 0)
        {
            EditorGUILayout.Popup("Component", 0, new[] { "(No components)" });
            return null;
        }

        string[] names = comps.Select(c => c.GetType().Name).ToArray();
        int idx = Array.IndexOf(comps, current);
        if (idx < 0) idx = 0;

        int newIdx = EditorGUILayout.Popup("Component", idx, names);
        return comps[newIdx];
    }

    private void DrawUnityEventDropdown(Component src)
    {
        string[] options = Array.Empty<string>();

        if (src != null)
        {
            options = src.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(IsSupportedUnityEventField)
                .Select(f => f.Name)
                .OrderBy(n => n)
                .ToArray();
        }

        using (new EditorGUI.DisabledScope(src == null || options.Length == 0))
        {
            int idx = 0;
            if (!string.IsNullOrEmpty(_unityEventName) && options.Length > 0)
                idx = Mathf.Max(0, Array.IndexOf(options, _unityEventName));

            if (options.Length == 0)
            {
                EditorGUILayout.Popup("UnityEvent", 0, new[] { "(No supported UnityEvents)" });
                _unityEventName = "";
            }
            else
            {
                int newIdx = EditorGUILayout.Popup("UnityEvent", idx, options);
                _unityEventName = options[newIdx];
            }
        }
    }

    private bool IsSupportedUnityEventField(FieldInfo field)
    {
        var t = field.FieldType;
        if (!t.IsGenericType) return false;

        var def = t.GetGenericTypeDefinition();
        var args = t.GetGenericArguments();

        if (def == typeof(UnityEvent<>))
        {
            return args.Length == 1 && args[0] == typeof(float);
        }

        if (def == typeof(UnityEvent<,>))
        {
            return args.Length == 2 && args[0] == typeof(float) && args[1] == typeof(float);
        }

        return false;
    }

    private void DrawEventDropdown(Component src)
    {
        string[] options = Array.Empty<string>();

        if (src != null)
        {
            options = src.GetType()
                .GetEvents(BindingFlags.Instance | BindingFlags.Public)
                .Where(IsSupportedEventSignature)
                .Select(e => e.Name)
                .OrderBy(n => n)
                .ToArray();
        }

        using (new EditorGUI.DisabledScope(src == null || options.Length == 0))
        {
            int idx = 0;
            if (!string.IsNullOrEmpty(_eventName) && options.Length > 0)
                idx = Mathf.Max(0, Array.IndexOf(options, _eventName));

            if (options.Length == 0)
            {
                EditorGUILayout.Popup("Event", 0, new[] { "(No supported events)" });
                _eventName = "";
            }
            else
            {
                int newIdx = EditorGUILayout.Popup("Event", idx, options);
                _eventName = options[newIdx];
            }
        }
    }

    private void DrawGetterDropdown(Component src)
    {
        string[] options = Array.Empty<string>();

        if (src != null)
        {
            options = src.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.ReturnType == typeof(float) && m.GetParameters().Length == 0)
                .Select(m => m.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToArray();
        }

        using (new EditorGUI.DisabledScope(src == null || options.Length == 0))
        {
            int idx = 0;
            if (!string.IsNullOrEmpty(_getterName) && options.Length > 0)
                idx = Mathf.Max(0, Array.IndexOf(options, _getterName));

            if (options.Length == 0)
            {
                EditorGUILayout.Popup("Getter (optional)", 0, new[] { "(No float getters)" });
                _getterName = "";
            }
            else
            {
                int newIdx = EditorGUILayout.Popup("Getter (optional)", idx, options);
                _getterName = options[newIdx];
            }
        }
    }

    private bool IsSupportedEventSignature(EventInfo ev)
    {
        // 지원:
        // event Action<float>
        // event Action<float,float>
        var invoke = ev.EventHandlerType?.GetMethod("Invoke");
        if (invoke == null) return false;

        var pars = invoke.GetParameters();
        if (pars.Length == 1 && pars[0].ParameterType == typeof(float)) return true;
        if (pars.Length == 2 && pars[0].ParameterType == typeof(float) && pars[1].ParameterType == typeof(float)) return true;
        return false;
    }

    private void Bind(GameObject go, Image fill, Component src, string eventName, string unityEventName, string getterName)
    {
        // 1) StatBarBinder 자동 추가
        var binder = go.GetComponent<StatBarBinder>();
        if (binder == null)
        {
            Undo.AddComponent<StatBarBinder>(go);
            binder = go.GetComponent<StatBarBinder>();
        }

        // 2) fill을 Filled로 강제(보통 스탯바는 이게 맞음)
        EnsureFilled(fill);

        // 3) 바인더에 값 주입
        Undo.RecordObject(binder, "Bind Stat Bar");
        binder.Editor_SetBinding(src, eventName, unityEventName, fill, getterName);
        EditorUtility.SetDirty(binder);
    }
}
#endif
