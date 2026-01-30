using UnityEngine;
using UnityEngine.InputSystem;

public class TpsMover : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform cam;
    [SerializeField] private CharacterController controller;

    [Header("Move")]
    [SerializeField] private float speed = 6f;

    [Tooltip("짧�? ??�� Update 관측에 걸리지 ?�으�? 최소 n?�레?��? 마�?�??�력??1�????�서 '?�짝 ?�직????보장")]
    [SerializeField] private int minMoveFramesOnTap = 1;

    [Header("Jump/Gravity")]
    [SerializeField] private float jumpSpeed = 6f;
    [SerializeField] private float fallSpeed = -8f;
    [SerializeField] private float jumpDuration = 0.25f;
    [SerializeField] private float groundedStick = -2f;

    [Header("World Bounds")]
    [SerializeField] private float minX = 0.5f;
    [SerializeField] private float maxX = 9.5f;
    [SerializeField] private float minZ = 0.5f;
    [SerializeField] private float maxZ = 9.5f;

    private Vector2 moveInput;          // ?�재 ?�력 ?�태
    private Vector2 lastNonZeroMove;    // 마�?�?�? ?�력
    private int pendingMoveFrames;      // ??보장 ?�레??카운??
    private float vY;                   // ?�직 ?�도
    private float jumpTimeRemaining;

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        // ?�력??0???�니�?"??보장"???�약
        if (moveInput.sqrMagnitude > 0f)
        {
            lastNonZeroMove = moveInput;
            pendingMoveFrames = Mathf.Max(0, minMoveFramesOnTap);
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (controller == null) return;

        if (context.performed && controller.isGrounded)
        {
            vY = jumpSpeed;
            jumpTimeRemaining = jumpDuration;
        }
    }

    private void Update()
    {
        if (cam == null || controller == null) return;

        // 1) ?�번 ?�레?�에 ?�용???�력 결정 (??보장)
        Vector2 effectiveMove = moveInput;
        if (effectiveMove.sqrMagnitude == 0f && pendingMoveFrames > 0)
        {
            effectiveMove = lastNonZeroMove;
            pendingMoveFrames--;
        }

        // 2) 카메??기�? ?�동 방향
        Vector3 fwd = cam.forward;   fwd.y = 0f;   fwd.Normalize();
        Vector3 right = cam.right;   right.y = 0f; right.Normalize();

        Vector3 moveDir = fwd * effectiveMove.y + right * effectiveMove.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        Vector3 horizontal = moveDir * speed;

        // 3) ???? ?ռ???+ ???
        if (controller.isGrounded)
        {
            if (jumpTimeRemaining <= 0f && vY < 0f) vY = groundedStick;
        }

        if (jumpTimeRemaining > 0f)
        {
            vY = jumpSpeed;
            jumpTimeRemaining -= Time.deltaTime;
        }
        else if (!controller.isGrounded)
        {
            vY = fallSpeed;
        }
        // 4) ?�용
        Vector3 velocity = horizontal + Vector3.up * vY;
        controller.Move(velocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        p.z = Mathf.Clamp(p.z, minZ, maxZ);
        transform.position = p;
    }
}



