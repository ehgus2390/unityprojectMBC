using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 오브젝트의 체력 시스템을 관리하는 스크립트
/// 데미지, 힐링, 이벤트 시스템, 이펙트 기능을 제공합니다.
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("오브젝트의 최대 체력")]
    public int maxHealth = 100;

    [Tooltip("오브젝트의 현재 체력")]
    public int currentHealth;

    [Header("Events")]
    [Tooltip("데미지를 받았을 때 호출될 이벤트")]
    public UnityEvent OnDamage;

    [Tooltip("사망했을 때 호출될 이벤트")]
    public UnityEvent OnDeath;

    [Tooltip("체력이 변경되었을 때 호출될 이벤트 (현재 체력을 매개변수로 전달)")]
    public UnityEvent<int> OnHealthChanged;

    [Header("Effects")]
    [Tooltip("데미지를 받았을 때 생성될 이펙트 프리팹")]
    public GameObject damageEffectPrefab;

    [Tooltip("사망했을 때 생성될 이펙트 프리팹")]
    public GameObject deathEffectPrefab;

    [Tooltip("데미지를 받았을 때 재생될 사운드")]
    public AudioClip damageSound;

    [Tooltip("사망했을 때 재생될 사운드")]
    public AudioClip deathSound;

    // 내부 변수들
    /// <summary>사운드 재생을 위한 AudioSource 컴포넌트</summary>
    private AudioSource _audioSource;

    /// <summary>오브젝트가 이미 사망했는지 확인하는 플래그</summary>
    private bool _isDead = false;

    /// <summary>
    /// 스크립트 초기화 시 호출되는 함수
    /// 체력을 최대값으로 설정하고 AudioSource를 준비하며 초기 이벤트를 호출합니다.
    /// </summary>
    void Start()
    {
        currentHealth = maxHealth;
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 초기 체력 변경 이벤트 호출
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// 오브젝트가 데미지를 받았을 때 호출되는 함수
    /// 체력을 감소시키고 이펙트를 생성하며, 체력이 0 이하가 되면 사망 처리합니다.
    /// </summary>
    /// <param name="damage">받을 데미지량</param>
    public void TakeDamage(int damage)
    {
        // 이미 사망한 오브젝트는 데미지를 받지 않음
        if (_isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // 체력이 음수가 되지 않도록 제한

        // 데미지 이펙트 생성
        if (damageEffectPrefab != null)
        {
            GameObject damageEffect = Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
            Destroy(damageEffect, 2f); // 2초 후 이펙트 제거
        }

        // 데미지 사운드 재생
        if (damageSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(damageSound);
        }

        // 이벤트 호출
        OnDamage?.Invoke();
        OnHealthChanged?.Invoke(currentHealth);

        // 체력이 0 이하가 되면 사망
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 오브젝트의 체력을 회복하는 함수
    /// 지정된 양만큼 체력을 증가시키고 최대 체력을 초과하지 않도록 제한합니다.
    /// </summary>
    /// <param name="healAmount">회복할 체력량</param>
    public void Heal(int healAmount)
    {
        // 이미 사망한 오브젝트는 회복할 수 없음
        if (_isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth); // 최대 체력을 초과하지 않도록 제한

        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// 오브젝트의 체력을 직접 설정하는 함수
    /// 0에서 최대 체력 사이의 값으로 제한됩니다.
    /// </summary>
    /// <param name="health">설정할 체력값</param>
    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);

        // 체력이 0 이하가 되고 아직 사망하지 않았다면 사망 처리
        if (currentHealth <= 0 && !_isDead)
        {
            Die();
        }
    }

    /// <summary>
    /// 오브젝트를 사망 처리하는 함수
    /// 이펙트, 사운드, 이벤트를 발생시키고 사망 상태로 변경합니다.
    /// </summary>
    void Die()
    {
        // 이미 사망한 오브젝트는 중복 처리 방지
        if (_isDead) return;
        _isDead = true;

        // 사망 이펙트 생성
        if (deathEffectPrefab != null)
        {
            GameObject deathEffect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(deathEffect, 5f); // 5초 후 이펙트 제거
        }

        // 사망 사운드 재생
        if (deathSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(deathSound);
        }

        // 사망 이벤트 호출
        OnDeath?.Invoke();

        // 오브젝트 파괴 (선택사항 - 주석 처리되어 있음)
        // Destroy(gameObject, 2f);
    }

    /// <summary>
    /// 현재 체력의 백분율을 반환하는 함수
    /// UI 표시나 게임 로직에서 사용됩니다.
    /// </summary>
    /// <returns>체력 백분율 (0.0 ~ 1.0)</returns>
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }

    /// <summary>
    /// 오브젝트가 사망했는지 확인하는 함수
    /// 다른 스크립트에서 사망 상태를 확인할 때 사용됩니다.
    /// </summary>
    /// <returns>사망했으면 true, 아니면 false</returns>
    public bool IsDead()
    {
        return _isDead;
    }

    /// <summary>
    /// 오브젝트의 체력이 최대인지 확인하는 함수
    /// 힐링 로직이나 UI 표시에서 사용됩니다.
    /// </summary>
    /// <returns>체력이 최대이면 true, 아니면 false</returns>
    public bool IsFullHealth()
    {
        return currentHealth >= maxHealth;
    }

    /// <summary>
    /// 오브젝트를 부활시키는 함수
    /// 체력을 최대값으로 회복하고 사망 상태를 해제합니다.
    /// </summary>
    public void Revive()
    {
        if (_isDead)
        {
            _isDead = false;
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth);
        }
    }


    /// <summary>
    /// 오브젝트를 완전히 회복시키는 함수
    /// 체력을 최대값으로 설정합니다 (사망 상태가 아닌 경우).
    /// </summary>
    public void FullHeal()
    {
        if (!_isDead)
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth);
        }
    }

    /// <summary>
    /// 최대 체력을 변경하는 함수
    /// 새로운 최대 체력이 현재 체력보다 작으면 현재 체력도 조정됩니다.
    /// </summary>
    /// <param name="newMaxHealth">새로운 최대 체력</param>
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }
}