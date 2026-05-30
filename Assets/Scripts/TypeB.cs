using UnityEngine;

/// <summary>
/// TypeB 적: TypeA와 동일한 메커니즘. 이동이 더 빠르고 체력이 더 낮음.
/// Bullet.cs에서 TakeDamage()를 호출합니다.
/// </summary>
public class TypeB : MonoBehaviour
{
    [Header("이동")]
    [Tooltip("왼쪽으로 이동하는 속도 (단위/초). TypeA보다 빠르게 기본 설정")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("체력")]
    [Tooltip("적의 최대 체력. TypeA보다 낮게 기본 설정")]
    [SerializeField] private float hp = 30f;

    /// <summary>
    /// 매 프레임 왼쪽으로 이동합니다.
    /// </summary>
    private void Update()
    {
        transform.Translate(Vector2.left * moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 플레이어와 몸통 박치기 충돌 처리.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌! 플레이어 체력 감소");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 총알 등에서 호출하여 데미지를 적용합니다.
    /// </summary>
    /// <param name="damageAmount">줄 데미지 양</param>
    /// <returns>처치되었으면 true, 살아 있으면 false (마지막 1발 즉시 재장전 연동)</returns>
    public bool TakeDamage(float damageAmount)
    {
        hp -= damageAmount;
        Debug.Log($"TypeB 피격! 남은 체력: {hp}");

        if (hp <= 0f)
        {
            Destroy(gameObject);
            return true;
        }

        return false;
    }
}
