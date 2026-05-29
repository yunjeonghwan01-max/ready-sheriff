using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D 플레이어 이동·점프·스프라이트 방향 전환.
/// New Input System 이벤트 구독 방식 (Update 폴링 없음).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    [Header("이동")]
    [Tooltip("좌우 이동 속도 (단위/초)")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("점프")]
    [Tooltip("점프 시 위로 가하는 힘")]
    [SerializeField] private float jumpForce = 10f;

    [Header("바닥 감지")]
    [SerializeField] private float groundCheckDistance = 0.05f;  // 발 밑 레이 길이
    [SerializeField] private LayerMask groundLayer;              // 바닥 레이어

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private PlayerInput inputActions;

    private float horizontalInput;  // Move x (-1 ~ 1), y는 무시
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputActions = new PlayerInput();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Default.Move.performed += OnMovePerformed;
        inputActions.Default.Move.canceled += OnMoveCanceled;
        inputActions.Default.Jump.performed += OnJumpPerformed;
        inputActions.Default.Jump.canceled += OnJumpCanceled;
    }

    private void OnDisable()
    {
        inputActions.Default.Move.performed -= OnMovePerformed;
        inputActions.Default.Move.canceled -= OnMoveCanceled;
        inputActions.Default.Jump.performed -= OnJumpPerformed;
        inputActions.Default.Jump.canceled -= OnJumpCanceled;

        inputActions.Disable();
        horizontalInput = 0f;
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();
        ApplyHorizontalMovement();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 move = context.ReadValue<Vector2>();
        horizontalInput = move.x;
        UpdateFacingDirection(horizontalInput);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        horizontalInput = 0f;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!isGrounded)
            return;

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnJumpCanceled(InputAction.CallbackContext context) { }

    /// <summary>입력 x 방향에 따라 스프라이트 좌우 반전</summary>
    private void UpdateFacingDirection(float directionX)
    {
        if (directionX > 0f)
            spriteRenderer.flipX = false;
        else if (directionX < 0f)
            spriteRenderer.flipX = true;
    }

    private void ApplyHorizontalMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;
        rb.linearVelocity = velocity;
    }

    private void UpdateGroundedState()
    {
        Bounds bounds = boxCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance,
            groundLayer);

        isGrounded = hit.collider != null;

#if UNITY_EDITOR
        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
#endif
    }
}
