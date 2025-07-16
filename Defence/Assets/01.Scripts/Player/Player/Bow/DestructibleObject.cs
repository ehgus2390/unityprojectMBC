using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 파괴 가능한 오브젝트를 관리하는 스크립트
/// 체력 시스템, 파괴 이펙트, 잔해 생성 기능을 제공합니다.
/// </summary>
public class DestructibleObject : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("오브젝트의 최대 체력")]
    public int maxHealth = 100;

    [Tooltip("오브젝트의 현재 체력")]
    public int currentHealth;

    [Header("Effects")]
    [Tooltip("데미지를 받았을 때 생성될 이펙트 프리팹")]
    public GameObject hitEffectPrefab;

    [Tooltip("파괴될 때 생성될 이펙트 프리팹")]
    public GameObject destroyEffectPrefab;

    [Tooltip("데미지를 받았을 때 재생될 사운드")]
    public AudioClip hitSound;

    [Tooltip("파괴될 때 재생될 사운드")]
    public AudioClip destroySound;

    [Header("Destruction")]
    [Tooltip("파괴 시 생성될 잔해 프리팹들의 배열")]
    public GameObject[] debrisPrefabs;

    [Tooltip("파괴 시 생성될 잔해의 개수")]
    public int debrisCount = 5;

    [Tooltip("잔해에 적용될 폭발력의 세기")]
    public float explosionForce = 500f;

    [Tooltip("폭발력이 적용되는 반경")]
    public float explosionRadius = 3f;

    // 내부 변수들
    /// <summary>사운드 재생을 위한 AudioSource 컴포넌트</summary>
    private AudioSource _audioSource;

    /// <summary>오브젝트가 이미 파괴되었는지 확인하는 플래그</summary>
    private bool _isDestroyed = false;

    /// <summary>
    /// 스크립트 초기화 시 호출되는 함수
    /// 체력을 최대값으로 설정하고 AudioSource를 준비합니다.
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 오브젝트가 데미지를 받았을 때 호출되는 함수
    /// 체력을 감소시키고 이펙트를 생성하며, 체력이 0 이하가 되면 파괴합니다.
    /// </summary>
    /// <param name="damage">받을 데미지량</param>
    public void TakeDamage(int damage)
    {
        // 이미 파괴된 오브젝트는 데미지를 받지 않음
        if (_isDestroyed) return;

        currentHealth -= damage;

        // 히트 이펙트 생성
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(hitEffect, 2f); // 2초 후 이펙트 제거
        }

        // 히트 사운드 재생
        if (hitSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(hitSound);
        }

        // 체력이 0 이하가 되면 파괴
        if (currentHealth <= 0)
        {
            DestroyObject();
        }
    }

    /// <summary>
    /// 오브젝트를 파괴하는 함수
    /// 이펙트, 사운드, 잔해 생성을 수행하고 오브젝트를 제거합니다.
    /// </summary>
    void DestroyObject()
    {
        // 이미 파괴된 오브젝트는 중복 처리 방지
        if (_isDestroyed) return;
        _isDestroyed = true;

        // 파괴 이펙트 생성
        if (destroyEffectPrefab != null)
        {
            GameObject destroyEffect = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            Destroy(destroyEffect, 5f); // 5초 후 이펙트 제거
        }

        // 파괴 사운드 재생
        if (destroySound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(destroySound);
        }

        // 잔해 생성
        CreateDebris();

        // 오브젝트 파괴 (0.1초 지연으로 이펙트가 완전히 재생되도록 함)
        Destroy(gameObject, 0.1f);
    }

    /// <summary>
    /// 파괴 시 잔해를 생성하는 함수
    /// 랜덤한 위치와 회전으로 잔해를 생성하고 물리 효과를 적용합니다.
    /// </summary>
    void CreateDebris()
    {
        // 잔해 프리팹이 설정되지 않았으면 생성하지 않음
        if (debrisPrefabs.Length == 0) return;

        for (int i = 0; i < debrisCount; i++)
        {
            // 랜덤한 잔해 프리팹 선택
            GameObject debrisPrefab = debrisPrefabs[Random.Range(0, debrisPrefabs.Length)];

            // 랜덤한 위치에 생성 (오브젝트 중심에서 0.5 유닛 반경 내)
            Vector3 randomPosition = transform.position + Random.insideUnitSphere * 0.5f;
            GameObject debris = Instantiate(debrisPrefab, randomPosition, Random.rotation);

            // 물리 효과 적용
            Rigidbody rb = debris.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // 잔해 자동 파괴 (10초 후)
            Destroy(debris, 10f);
        }
    }

    /// <summary>
    /// 트리거 충돌을 통해 데미지를 받는 함수
    /// "Arrow" 태그를 가진 오브젝트와 충돌 시 데미지를 받습니다.
    /// </summary>
    /// <param name="other">충돌한 다른 Collider</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow") && !_isDestroyed)
        {
            ArrowController arrow = other.GetComponent<ArrowController>();
            if (arrow != null)
            {
                TakeDamage(arrow.damage);
            }
        }
    }

    /// <summary>
    /// 현재 체력 백분율을 반환하는 함수
    /// UI 표시나 게임 로직에서 사용됩니다.
    /// </summary>
    /// <returns>체력 백분율 (0.0 ~ 1.0)</returns>
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    /// <summary>
    /// 오브젝트가 파괴되었는지 확인하는 함수
    /// 다른 스크립트에서 파괴 상태를 확인할 때 사용됩니다.
    /// </summary>
    /// <returns>파괴되었으면 true, 아니면 false</returns>
    public bool IsDestroyed()
    {
        return _isDestroyed;
    }

    /// <summary>
    /// 오브젝트의 체력을 완전히 회복하는 함수
    /// 게임 로직에서 필요할 때 사용됩니다.
    /// </summary>
    public void RestoreHealth()
    {
        if (!_isDestroyed)
        {
            currentHealth = maxHealth;
        }
    }
}