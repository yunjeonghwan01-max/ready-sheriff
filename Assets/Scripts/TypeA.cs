using UnityEngine;

/// <summary>
/// TypeA 적: 왼쪽 이동, 체력·피격 처리, 플레이어와의 몸통 박치기 충돌.
/// Bullet.cs에서 TakeDamage()를 호출합니다.
/// </summary>
public class TypeA : MonoBehaviour
{
    [Header("이동")]
    [Tooltip("왼쪽으로 이동하는 속도 (단위/초)")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("체력")]
    [Tooltip("적의 최대 체력")]
    [SerializeField] private float hp = 50f;

    /// <summary>
    /// 매 프레임 왼쪽으로 이동합니다. (월드 좌표 기준)
    /// </summary>
    private void Update()
    {
        // Vector2.left = (-1, 0). moveSpeed * Time.deltaTime → 프레임마다 거리 보정
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 트리거 콜라이더와 겹칠 때 호출됩니다. (Is Trigger 사용 시)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어와 몸통 박치기 충돌
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌! 플레이어 체력 감소");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 외부(총알 등)에서 호출하여 데미지를 적용합니다.
    /// </summary>
    /// <param name="damageAmount">줄 데미지 양</param>
    /// <returns>이번 공격으로 적이 처치되었으면 true, 살아 있으면 false</returns>
    public bool TakeDamage(float damageAmount)
    {
        hp -= damageAmount;
        Debug.Log($"TypeA 피격! 남은 체력: {hp}");

        // 체력이 0 이하이면 처치 후 오브젝트 제거
        if (hp <= 0f)
        {
            Destroy(gameObject);
            return true;
        }

        return false;
    }
}
