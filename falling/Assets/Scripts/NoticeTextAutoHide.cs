using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class NoticeTextAutoHide : MonoBehaviour
{
    [SerializeField] private float delaySeconds = 6f;
    [SerializeField] private bool disableGameObject = true;

    private TMP_Text tmpText;
    private Coroutine hideRoutine;

    private void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
        }
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        if (delaySeconds > 0f)
        {
            yield return new WaitForSeconds(delaySeconds);
        }
        HideNow();
    }

    private void HideNow()
    {
        if (disableGameObject)
        {
            gameObject.SetActive(false);
            return;
        }

        if (tmpText != null)
        {
            tmpText.enabled = false;
        }
    }
}
