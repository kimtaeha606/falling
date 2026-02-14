using UnityEngine;
using UnityEngine.InputSystem;

public class TpsMover : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform cam;
    [SerializeField] private CharacterController controller;

    [Header("Move")]
    [SerializeField] private float speed = 6f;

    [Header("Jump/Gravity")]
    [SerializeField] private float jumpSpeed = 6f;
    [SerializeField] private float fallSpeed = -8f;
    [SerializeField] private float jumpDuration = 0.25f;
    [SerializeField] private float groundedStick = -2f;

    

    private Vector2 moveInput;          // ?�재 ?�력 ?�태
    private float vY;                   // ?�직 ?�도
    private float jumpTimeRemaining;

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
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

        // 1) use raw input directly
        Vector2 effectiveMove = moveInput;
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

    
}

