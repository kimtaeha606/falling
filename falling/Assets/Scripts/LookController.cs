using UnityEngine;
using UnityEngine.InputSystem;

public class LookController : MonoBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform PlayerRoot;
    [SerializeField] private float sensitivity = 10f;
    [SerializeField] private float pitchMin = -25f;
    [SerializeField] private float pitchMax = 20f;
    

    private Vector2 lookInput;
    private float pitch;
    private float yaw;
    public static event System.Action<bool> OnAimStateChanged;

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }


    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float yawDelta = lookInput.x * sensitivity * Time.deltaTime;
        float pitchDelta = lookInput.y * sensitivity * Time.deltaTime;

        // Invert pitch so mouse up looks up.
        pitch -= pitchDelta;
        pitch = Mathf.Clamp(pitch, pitchMin, pitchMax);

        yaw += yawDelta;

        cameraPivot.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
