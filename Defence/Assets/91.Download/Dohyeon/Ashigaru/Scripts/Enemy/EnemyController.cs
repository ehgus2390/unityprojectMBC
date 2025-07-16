using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// VR 방어 게임에서 적을 제어하는 스크립트
/// Ashigaru 캐릭터와 함께 성을 향해 이동하는 적을 제어합니다.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("적의 최대 체력")]
    public int maxHealth = 100;

    [Tooltip("적의 이동 속도")]
    public float moveSpeed = 3f;

    [Tooltip("적이 성에 도달했을 때 주는 데미지")]
    public int castleDamage = 10;

    [Header("AI Settings")]
    [Tooltip("목표 성 (성 위치)")]
    public Transform targetCastle;

    [Tooltip("적이 성을 공격할 때의 공격 딜레이")]
    public float attackDelay = 1f;

    [Header("Effects")]
    [Tooltip("적이 데미지를 받았을 때 생성할 이펙트")]
    public GameObject hitEffectPrefab;

    [Tooltip("적이 죽었을 때 생성할 이펙트")]
    public GameObject deathEffectPrefab;

    [Tooltip("적이 성을 공격했을 때 생성할 이펙트")]
    public GameObject castleHitEffectPrefab;

    [Header("Audio")]
    [Tooltip("적이 데미지를 받았을 때 재생할 사운드")]
    public AudioClip hitSound;

    [Tooltip("적이 죽었을 때 재생할 사운드")]
    public AudioClip deathSound;

    [Tooltip("적이 성을 공격했을 때 재생할 사운드")]
    public AudioClip castleHitSound;

    // 내부 변수들
    private int _currentHealth;
    private NavMeshAgent _navAgent;
    private AudioSource audioSource;
    private bool isDead = false;
    private bool hasReachedCastle = false;

    // 이벤트
    public System.Action<EnemyController> OnEnemyDeath;
    public System.Action<EnemyController> OnEnemyReachedCastle;

    void Start()
    {
        InitializeEnemy();
    }

    /// <summary>
    /// 적 초기화
    /// </summary>
    void InitializeEnemy()
    {
        _currentHealth = maxHealth;

        // NavMeshAgent 컴포넌트 확인
        _navAgent = GetComponent<NavMeshAgent>();
        if (_navAgent == null)
        {
            _navAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        // AudioSource 컴포넌트 확인
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // NavMeshAgent 설정
        _navAgent.speed = moveSpeed;
        _navAgent.stoppingDistance = 1f;

        // 목표 성 설정
        if (targetCastle != null)
        {
            _navAgent.SetDestination(targetCastle.position);
        }
        else
        {
            // 씬에서 "Castle" 태그를 가진 게임오브젝트 찾기
            GameObject castle = GameObject.FindGameObjectWithTag("Castle");
            if (castle != null)
            {
                targetCastle = castle.transform;
                _navAgent.SetDestination(targetCastle.position);
            }
        }
    }

    void Update()
    {
        if (isDead || hasReachedCastle) return;

        // 성에 도달했는지 확인
        if (targetCastle != null && Vector3.Distance(transform.position, targetCastle.position) <= _navAgent.stoppingDistance)
        {
            OnReachCastle();
        }
    }

    /// <summary>
    /// CastleWall과 충돌했을 때 호출
    /// </summary>
    /// <param name="other">충돌한 오브젝트</param>
    void OnTriggerEnter(Collider other)
    {
        // CastleWall과 충돌했는지 확인
        if (other.CompareTag("CastleWall") || other.name.Contains("CastleWall"))
        {
            CastleHealth castleHealth = other.GetComponent<CastleHealth>();
            if (castleHealth != null)
            {
                castleHealth.TakeDamage(10);
            }
            Debug.Log("적이 죽음");
            // 적이 죽도록 함
            Die();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        

        

    }

    /// <summary>
    /// 적이 데미지를 받았을 때 호출
    /// </summary>
    /// <param name="damage">받은 데미지량</param>
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        _currentHealth -= damage;

        // 히트 이펙트 생성
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(hitEffect, 2f);
        }

        // 히트 사운드 재생
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 체력이 0 이하가 되면 죽음
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 적이 죽었을 때 호출
    /// </summary>
    void Die()
    {
        if (isDead) return;

        isDead = true;

        // 죽음 이펙트 생성
        if (deathEffectPrefab != null)
        {
            GameObject deathEffect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(deathEffect, 3f);
        }

        // 죽음 사운드 재생
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // AI 비활성화
        if (_navAgent != null)
        {
            _navAgent.enabled = false;
        }

        // 이벤트 호출
        OnEnemyDeath?.Invoke(this);

        // 게임오브젝트 제거 (지연 시간 후)
        Destroy(gameObject, 2f);
    }

    /// <summary>
    /// 적이 성에 도달했을 때 호출
    /// </summary>
    void OnReachCastle()
    {
        if (hasReachedCastle) return;

        hasReachedCastle = true;

        // AI 비활성화
        if (_navAgent != null)
        {
            _navAgent.enabled = false;
        }

        // 성 공격 이펙트 생성
        if (castleHitEffectPrefab != null)
        {
            GameObject castleEffect = Instantiate(castleHitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(castleEffect, 3f);
        }

        // 성 공격 사운드 재생
        if (castleHitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(castleHitSound);
        }

        // 성 체력 감소
        CastleHealth castleHealth = targetCastle.gameObject.GetComponent<CastleHealth>();
        
        if (castleHealth != null)
        {
            castleHealth.TakeDamage(castleDamage);
        }

        // 이벤트 호출
        OnEnemyReachedCastle?.Invoke(this);

        // 게임오브젝트 제거
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// 현재 체력 반환
    /// </summary>
    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    /// <summary>
    /// 최대 체력 반환
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// 체력 비율 반환 (0~1)
    /// </summary>
    public float GetHealthRatio()
    {
        return (float)_currentHealth / maxHealth;
    }

    /// <summary>
    /// 적이 죽었는지 확인
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }

    /// <summary>
    /// 적이 성에 도달했는지 확인
    /// </summary>
    public bool HasReachedCastle()
    {
        return hasReachedCastle;
    }
}