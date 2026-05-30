using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 2D 플레이어: 이동, 점프, 방향 전환, 공격 입력 처리.
/// New Input System 이벤트 구독 방식 (Update에서 입력 폴링하지 않음).
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
    [Tooltip("발 밑으로 쏘는 레이의 길이")]
    [SerializeField] private float groundCheckDistance = 0.05f;

    [Tooltip("바닥으로 인식할 레이어")]
    [SerializeField] private LayerMask groundLayer;

    [Header("공격")]
    [Tooltip("자식 오브젝트에 붙은 Weapon1 스크립트 (Inspector에서 드래그 연결)")]
    [SerializeField] private Weapon1 weapon;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private PlayerInput inputActions;

    /// <summary>좌우 반전 시 사용할 x 스케일 절댓값 (Awake에서 저장)</summary>
    private float baseScaleX;

    /// <summary>Move 액션의 x 입력 (-1 ~ 1). y(상하)는 사용하지 않음</summary>
    private float horizontalInput;

    /// <summary>바닥에 닿아 있는지 (무한 점프 방지)</summary>
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        inputActions = new PlayerInput();
        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Default.Move.performed += OnMovePerformed;
        inputActions.Default.Move.canceled += OnMoveCanceled;
        inputActions.Default.Jump.performed += OnJumpPerformed;
        inputActions.Default.Jump.canceled += OnJumpCanceled;
        inputActions.Default.Attack.performed += OnAttackPerformed;
        inputActions.Default.Reload.performed += OnReloadPerformed;
    }

    private void OnDisable()
    {
        inputActions.Default.Move.performed -= OnMovePerformed;
        inputActions.Default.Move.canceled -= OnMoveCanceled;
        inputActions.Default.Jump.performed -= OnJumpPerformed;
        inputActions.Default.Jump.canceled -= OnJumpCanceled;
        inputActions.Default.Attack.performed -= OnAttackPerformed;
        inputActions.Default.Reload.performed -= OnReloadPerformed;

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

    // ─── Move ───────────────────────────────────────

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

    /// <summary>
    /// localScale.x로 플레이어 전체(자식 FirePoint 포함)를 좌우 반전합니다.
    /// </summary>
    private void UpdateFacingDirection(float directionX)
    {
        Vector3 scale = transform.localScale;

        if (directionX > 0f)
            scale.x = baseScaleX;
        else if (directionX < 0f)
            scale.x = -baseScaleX;

        transform.localScale = scale;
    }

    private void ApplyHorizontalMovement()
    {
        Vector2 velocity = rb.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;
        rb.linearVelocity = velocity;
    }

    // ─── Jump ───────────────────────────────────────

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!isGrounded)
            return;

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnJumpCanceled(InputAction.CallbackContext context) { }

    // ─── Attack ─────────────────────────────────────

    /// <summary>
    /// Attack(마우스 좌클릭) 입력 시 호출됩니다.
    /// 현재 바라보는 방향으로 무기 발사를 요청합니다.
    /// </summary>
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (weapon == null)
        {
            Debug.LogWarning("Player: Weapon1이 연결되지 않았습니다. Inspector에서 weapon을 할당하세요.");
            return;
        }

        float facingDirectionX = GetFacingDirectionX();
        weapon.Fire(facingDirectionX);
    }

    // ─── Reload ─────────────────────────────────────

    /// <summary>
    /// Reload 버튼 입력 시 무기 재장전을 요청합니다. (자동 장전 없음)
    /// </summary>
    private void OnReloadPerformed(InputAction.CallbackContext context)
    {
        if (weapon == null)
        {
            Debug.LogWarning("Player: Weapon1이 연결되지 않았습니다. Inspector에서 weapon을 할당하세요.");
            return;
        }

        weapon.Reload();
    }

    /// <summary>
    /// localScale.x 기준 바라보는 방향. x > 0 → 오른쪽(1), x < 0 → 왼쪽(-1)
    /// </summary>
    private float GetFacingDirectionX()
    {
        return transform.localScale.x > 0f ? 1f : -1f;
    }

    // ─── Ground Check ───────────────────────────────

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
