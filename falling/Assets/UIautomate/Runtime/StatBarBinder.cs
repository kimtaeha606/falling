using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class StatBarBinder : MonoBehaviour
{
    [Header("Stat Bar Binder")]
    [SerializeField] private Image fillImage;          // 자동(가능하면)
    [SerializeField] private Component source;         // 드래그
    [SerializeField] private string eventName;         // 드롭다운 선택 결과
    [SerializeField] private string unityEventFieldName; // UnityEvent field name (float or float,float)
    [SerializeField] private string normalizedGetter;  // 선택된 이벤트 없을 때 폴링용(선택)

    private Delegate _handler;
    private EventInfo _eventInfo;
    private MethodInfo _getter;
    private object _unityEventInstance;
    private Delegate _unityEventHandler;
    private MethodInfo _unityEventRemove;

    // 0) Fill 자동 할당(가능하면)
    private void Reset()
    {
        AutoAssignFill();
    }

    private void OnValidate()
    {
        if (fillImage == null) AutoAssignFill();
        if (fillImage != null) fillImage.type = Image.Type.Filled;
    }

    private void AutoAssignFill()
    {
        // 본인 또는 자식 중 첫 Image 찾기
        fillImage = GetComponent<Image>();
        if (fillImage == null) fillImage = GetComponentInChildren<Image>(true);
    }

    private void OnEnable()
    {
        TryBindRuntime();
    }

    private void OnDisable()
    {
        TryUnbindRuntime();
    }

    private void Update()
    {
        // 이벤트가 없거나 바인딩 실패한 경우 폴링(선택)
        if (_eventInfo == null && _unityEventInstance == null && _getter != null)
        {
            float v = (float)_getter.Invoke(source, null);
            ApplyNormalized(v);
        }
    }

    private void TryBindRuntime()
    {
        if (fillImage == null || source == null) return;

        // 0) UnityEvent binding
        if (!string.IsNullOrEmpty(unityEventFieldName))
        {
            var field = source.GetType().GetField(unityEventFieldName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (field != null && TryBindUnityEvent(field))
            {
                return;
            }
        }

        // 1) 이벤트 바인딩 우선
        if (!string.IsNullOrEmpty(eventName))
        {
            _eventInfo = source.GetType().GetEvent(eventName,
                BindingFlags.Instance | BindingFlags.Public);

            if (_eventInfo != null)
            {
                // 지원 시그니처:
                // (float normalized)
                // (float current, float max)
                var handlerMethod = GetType().GetMethod(nameof(OnStatChanged_Normalized),
                    BindingFlags.Instance | BindingFlags.NonPublic);

                var invoke = _eventInfo.EventHandlerType?.GetMethod("Invoke");
                var pars = invoke?.GetParameters();

                if (pars != null && pars.Length == 1 && pars[0].ParameterType == typeof(float))
                {
                    // event Action<float>
                    _handler = Delegate.CreateDelegate(_eventInfo.EventHandlerType, this, handlerMethod);
                    _eventInfo.AddEventHandler(source, _handler);
                    return;
                }

                if (pars != null && pars.Length == 2 &&
                    pars[0].ParameterType == typeof(float) &&
                    pars[1].ParameterType == typeof(float))
                {
                    // event Action<float,float> => current, max
                    handlerMethod = GetType().GetMethod(nameof(OnStatChanged_CurrentMax),
                        BindingFlags.Instance | BindingFlags.NonPublic);

                    _handler = Delegate.CreateDelegate(_eventInfo.EventHandlerType, this, handlerMethod);
                    _eventInfo.AddEventHandler(source, _handler);
                    return;
                }

                // 시그니처가 안 맞으면 이벤트는 사용 불가 처리
                _eventInfo = null;
                _handler = null;
            }
        }

        // 2) 이벤트가 없으면(또는 실패) Getter 폴링
        if (!string.IsNullOrEmpty(normalizedGetter))
        {
            _getter = source.GetType().GetMethod(normalizedGetter,
                BindingFlags.Instance | BindingFlags.Public);

            if (_getter != null && _getter.ReturnType == typeof(float) && _getter.GetParameters().Length == 0)
            {
                float v = (float)_getter.Invoke(source, null);
                ApplyNormalized(v);
            }
        }
    }

    private bool TryBindUnityEvent(FieldInfo field)
    {
        var fieldType = field.FieldType;
        if (!fieldType.IsGenericType) return false;

        var def = fieldType.GetGenericTypeDefinition();
        var args = fieldType.GetGenericArguments();

        if (def == typeof(UnityEvent<>))
        {
            if (args.Length != 1 || args[0] != typeof(float)) return false;

            var instance = field.GetValue(source);
            if (instance == null) return false;

            var addMethod = fieldType.GetMethod("AddListener");
            var removeMethod = fieldType.GetMethod("RemoveListener");
            if (addMethod == null || removeMethod == null) return false;

            var handler = new UnityAction<float>(OnStatChanged_Normalized);
            addMethod.Invoke(instance, new object[] { handler });

            _unityEventInstance = instance;
            _unityEventHandler = handler;
            _unityEventRemove = removeMethod;
            return true;
        }

        if (def == typeof(UnityEvent<,>))
        {
            if (args.Length != 2 || args[0] != typeof(float) || args[1] != typeof(float)) return false;

            var instance = field.GetValue(source);
            if (instance == null) return false;

            var addMethod = fieldType.GetMethod("AddListener");
            var removeMethod = fieldType.GetMethod("RemoveListener");
            if (addMethod == null || removeMethod == null) return false;

            var handler = new UnityAction<float, float>(OnStatChanged_CurrentMax);
            addMethod.Invoke(instance, new object[] { handler });

            _unityEventInstance = instance;
            _unityEventHandler = handler;
            _unityEventRemove = removeMethod;
            return true;
        }

        return false;
    }

    private void TryUnbindRuntime()
    {
        if (_eventInfo != null && _handler != null && source != null)
        {
            _eventInfo.RemoveEventHandler(source, _handler);
        }

        if (_unityEventInstance != null && _unityEventHandler != null && _unityEventRemove != null)
        {
            _unityEventRemove.Invoke(_unityEventInstance, new object[] { _unityEventHandler });
        }

        _eventInfo = null;
        _handler = null;
        _getter = null;
        _unityEventInstance = null;
        _unityEventHandler = null;
        _unityEventRemove = null;
    }

    private void OnStatChanged_Normalized(float normalized)
    {
        ApplyNormalized(normalized);
    }

    private void OnStatChanged_CurrentMax(float current, float max)
    {
        float n = (max <= 0f) ? 0f : current / max;
        ApplyNormalized(n);
    }

    private void ApplyNormalized(float normalized)
    {
        if (fillImage == null) return;
        normalized = Mathf.Clamp01(normalized);
        fillImage.fillAmount = normalized;
    }

    // Editor가 바인드 결과를 넣는 용도(버튼 눌렀을 때)
    public void Editor_SetBinding(Component newSource, string newEventName, string newUnityEventName, Image newFill, string getterName)
    {
        source = newSource;
        eventName = newEventName;
        unityEventFieldName = newUnityEventName;
        fillImage = newFill;
        normalizedGetter = getterName;
    }
}
