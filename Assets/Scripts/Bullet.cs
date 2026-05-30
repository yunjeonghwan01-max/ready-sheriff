using UnityEngine;

/// <summary>
/// 발사된 총알: 수평 직선 이동, Enemy 태그 적(TypeA / TypeB) 타격, 일정 시간 후 자동 파괴.
/// 마지막 1발로 처치 시 Weapon1 즉시 재장전을 지원합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [Header("이동")]
    [Tooltip("총알이 날아가는 속도 (단위/초)")]
    [SerializeField] private float speed = 10f;

    [Header("수명")]
    [Tooltip("생성 후 자동 파괴까지 걸리는 시간(초). 메모리 누수 방지")]
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;

    /// <summary>이 총알이 적에게 줄 데미지</summary>
    private float damage;

    /// <summary>발사 시 탄창에 1발만 남았던 총알인지 (처치 시 즉시 재장전 조건)</summary>
    private bool isLastShot;

    /// <summary>이 총알을 발사한 Weapon1 참조 (InstantReload 호출용)</summary>
    private Weapon1 originWeapon;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Weapon1에서 Instantiate 직후 호출합니다.
    /// </summary>
    public void Setup(float dirX, float weaponDamage, float speedBonus, bool isLastShot, Weapon1 weapon)
    {
        damage = weaponDamage;
        this.isLastShot = isLastShot;
        originWeapon = weapon;

        speed += speedBonus;

        Vector2 velocity = new Vector2(dirX * speed, 0f);
        rb.linearVelocity = velocity;
    }

    /// <summary>
    /// Enemy 태그와 충돌 시 TypeA → 없으면 TypeB 순으로 데미지를 적용합니다.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy"))
            return;

        // 처치 여부 (마지막 1발 즉시 재장전에 사용)
        bool isKilled = false;

        // 1) TypeA 적인지 먼저 확인
        TypeA enemyA = other.GetComponent<TypeA>();
        if (enemyA != null)
        {
            isKilled = enemyA.TakeDamage(damage);
        }
        else
        {
            // 2) TypeA가 없으면 TypeB 적으로 처리
            TypeB enemyB = other.GetComponent<TypeB>();
            if (enemyB != null)
                isKilled = enemyB.TakeDamage(damage);
        }

        // 마지막 1발 + 처치 성공 → TypeA / TypeB 공통으로 즉시 재장전
        if (isLastShot && isKilled && originWeapon != null)
            originWeapon.InstantReload();

        Destroy(gameObject);
    }
}
