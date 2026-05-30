using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 플레이어 무기: 총알 발사, 탄창(최대 6발), 수동 재장전, TMP UI 표시.
/// Player.cs에서 Fire(directionX), Reload()를 호출합니다.
/// </summary>
public class Weapon1 : MonoBehaviour
{
    [Header("발사 설정")]
    [Tooltip("생성할 총알 프리팹 (Bullet 스크립트가 붙어 있어야 함)")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("총알이 나가는 위치 (보통 총구 Transform)")]
    [SerializeField] private Transform firePoint;

    [Tooltip("연속 발사 사이 최소 간격(초). 0.5면 초당 최대 2발")]
    [SerializeField] private float fireRate = 0.5f;

    [Tooltip("총알 1발당 기본 데미지")]
    [SerializeField] private float baseDamage = 10f;

    [Header("풀 탄창 특수 규칙")]
    [Tooltip("탄창이 가득 찬 상태에서 쏜 첫 발의 데미지 배율 (2 = 2배)")]
    [SerializeField] private float fullAmmoDamageMultiplier = 2f;

    [Tooltip("풀 탄창 첫 발에 더해지는 총알 이동 속도")]
    [SerializeField] private float fullAmmoSpeedBonus = 10f;

    [Header("탄창")]
    [Tooltip("탄창 최대 탄 수 (예비 탄약은 무한, 탄창만 제한)")]
    [SerializeField] private int maxAmmo = 6;

    [Tooltip("재장전에 걸리는 시간(초)")]
    [SerializeField] private float reloadTime = 2f;

    [Header("UI")]
    [Tooltip("탄창 수를 표시할 TextMeshProUGUI (형식: (현재/6))")]
    [SerializeField] private TextMeshProUGUI ammoText;

    /// <summary>현재 탄창에 남은 탄 수</summary>
    private int currentAmmo;

    /// <summary>재장전 코루틴 진행 중 여부. true면 발사 불가</summary>
    private bool isReloading = false;

    /// <summary>마지막 발사 시각 (연사 제한용)</summary>
    private float lastFireTime;

    /// <summary>실행 중인 재장전 코루틴 참조 (중복 실행 방지·정리용)</summary>
    private Coroutine reloadCoroutine;

    private void Start()
    {
        // 게임 시작 시 탄창을 가득 채움
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    /// <summary>
    /// 지정한 방향으로 총알을 발사합니다.
    /// 재장전 중이거나 탄창이 비었으면 발사하지 않습니다.
    /// </summary>
    /// <param name="directionX">수평 방향. 오른쪽=1, 왼쪽=-1</param>
    public void Fire(float directionX)
    {
        // 재장전 중이거나 탄창이 0발이면 발사 불가
        if (isReloading || currentAmmo <= 0)
            return;

        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Weapon1: bulletPrefab 또는 firePoint가 비어 있습니다.");
            return;
        }

        // 연사 속도(쿨타임) 제한
        if (Time.time < lastFireTime + fireRate)
            return;

        lastFireTime = Time.time;

        // ── 특수 규칙 판정 (총알 생성·탄 감소 전) ──
        bool isLastShot = (currentAmmo == 1);

        float finalDamage;
        float speedBonus;

        // 풀 탄창 규칙과 마지막 1발 규칙은 겹치지 않도록 if - else if 처리
        if (currentAmmo == maxAmmo)
        {
            // 만탄(6발) 상태에서 쏘는 첫 발: 데미지 배율 + 속도 보너스
            finalDamage = baseDamage * fullAmmoDamageMultiplier;
            speedBonus = fullAmmoSpeedBonus;
        }
        else if (isLastShot)
        {
            // 탄창에 1발만 남았을 때: 데미지 2배 (속도 보너스 없음)
            finalDamage = baseDamage * 2f;
            speedBonus = 0f;
        }
        else
        {
            finalDamage = baseDamage;
            speedBonus = 0f;
        }

        GameObject bulletObject = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation);

        Bullet bullet = bulletObject.GetComponent<Bullet>();
        if (bullet == null)
        {
            Debug.LogWarning("Weapon1: 총알 프리팹에 Bullet 스크립트가 없습니다.");
            Destroy(bulletObject);
            return;
        }

        // 마지막 총알 여부·무기 참조를 넘겨 처치 시 즉시 재장전에 사용
        bullet.Setup(directionX, finalDamage, speedBonus, isLastShot, this);

        // 발사 성공 후 탄창 1발 소모 및 UI 갱신
        currentAmmo--;
        UpdateAmmoUI();
    }

    /// <summary>
    /// 마지막 총알로 적 처치 시 호출. 쿨타임 없이 탄창을 즉시 가득 채웁니다.
    /// </summary>
    public void InstantReload()
    {
        // 진행 중이던 일반 재장전 코루틴이 있으면 중단
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        isReloading = false;
        currentAmmo = maxAmmo;
        UpdateAmmoUI();
    }

    /// <summary>
    /// 수동 재장전을 시작합니다. Reload 입력으로만 호출됩니다. (자동 장전 없음)
    /// </summary>
    public void Reload()
    {
        // 이미 장전 중이거나 탄창이 가득 차 있으면 무시
        if (isReloading || currentAmmo >= maxAmmo)
            return;

        // 이전 코루틴이 있다면 중단 후 새로 시작 (안전 처리)
        if (reloadCoroutine != null)
            StopCoroutine(reloadCoroutine);

        reloadCoroutine = StartCoroutine(ReloadCoroutine());
    }

    /// <summary>
    /// reloadTime(2초) 대기 후 탄창을 maxAmmo까지 채웁니다.
    /// </summary>
    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        reloadCoroutine = null;

        UpdateAmmoUI();
    }

    /// <summary>
    /// TextMeshPro UI를 "(현재탄창/최대)" 형식으로 갱신합니다.
    /// </summary>
    private void UpdateAmmoUI()
    {
        if (ammoText == null)
            return;

        ammoText.text = $"({currentAmmo}/{maxAmmo})";
    }

    private void OnDisable()
    {
        // 비활성화 시 재장전 코루틴 정리
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
            isReloading = false;
        }
    }
}
