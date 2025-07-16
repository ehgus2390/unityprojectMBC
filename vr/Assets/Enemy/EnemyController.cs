using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// VR 디펜스 게임의 적군을 제어하는 스크립트
/// Ashigaru 프리팹에 적용하여 성으로 이동하는 적군을 구현합니다.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("적군의 최대 체력")]
    public int maxHealth = 100;
    
    [Tooltip("적군의 이동 속도")]
    public float moveSpeed = 3f;
    
    [Tooltip("적군이 성에 도달했을 때 가하는 데미지")]
    public int castleDamage = 20;
    
    [Header("AI Settings")]
    [Tooltip("목표 지점 (성의 위치)")]
    public Transform targetCastle;
    
    [Tooltip("적군이 성에 도달했을 때의 반응 시간")]
    public float attackDelay = 1f;
    
    [Header("Effects")]
    [Tooltip("적군이 피해를 받았을 때의 이펙트")]
    public GameObject hitEffectPrefab;
    
    [Tooltip("적군이 죽었을 때의 이펙트")]
    public GameObject deathEffectPrefab;
    
    [Tooltip("적군이 성에 도달했을 때의 이펙트")]
    public GameObject castleHitEffectPrefab;
    
    [Header("Audio")]
    [Tooltip("적군이 피해를 받았을 때의 사운드")]
    public AudioClip hitSound;
    
    [Tooltip("적군이 죽었을 때의 사운드")]
    public AudioClip deathSound;
    
    [Tooltip("적군이 성에 도달했을 때의 사운드")]
    public AudioClip castleHitSound;
    
    // 내부 변수들
    private int currentHealth;
    private NavMeshAgent navAgent;
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
    /// 적군 초기화
    /// </summary>
    void InitializeEnemy()
    {
        currentHealth = maxHealth;
        
        // NavMeshAgent 컴포넌트 가져오기
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        // AudioSource 컴포넌트 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // NavMeshAgent 설정
        navAgent.speed = moveSpeed;
        navAgent.stoppingDistance = 1f;
        
        // 목표 지점 설정
        if (targetCastle != null)
        {
            navAgent.SetDestination(targetCastle.position);
        }
        else
        {
            // 씬에서 "Castle" 태그를 가진 오브젝트 찾기
            GameObject castle = GameObject.FindGameObjectWithTag("Castle");
            if (castle != null)
            {
                targetCastle = castle.transform;
                navAgent.SetDestination(targetCastle.position);
            }
        }
    }
    
    void Update()
    {
        if (isDead || hasReachedCastle) return;
        
        // 성에 도달했는지 확인
        if (targetCastle != null && Vector3.Distance(transform.position, targetCastle.position) <= navAgent.stoppingDistance)
        {
            OnReachCastle();
        }
    }
    
    /// <summary>
    /// 적군이 데미지를 받았을 때 호출
    /// </summary>
    /// <param name="damage">받은 데미지량</param>
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        
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
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    /// <summary>
    /// 적군이 죽었을 때 호출
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
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        // 이벤트 호출
        OnEnemyDeath?.Invoke(this);
        
        // 오브젝트 파괴 (지연 시간 후)
        Destroy(gameObject, 2f);
    }
    
    /// <summary>
    /// 적군이 성에 도달했을 때 호출
    /// </summary>
    void OnReachCastle()
    {
        if (hasReachedCastle) return;
        
        hasReachedCastle = true;
        
        // AI 비활성화
        if (navAgent != null)
        {
            navAgent.enabled = false;
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
        
        // 성에 데미지 적용
        CastleHealth castleHealth = targetCastle.GetComponent<CastleHealth>();
        if (castleHealth != null)
        {
            castleHealth.TakeDamage(castleDamage);
        }
        
        // 이벤트 호출
        OnEnemyReachedCastle?.Invoke(this);
        
        // 오브젝트 파괴
        Destroy(gameObject, 1f);
    }
    
    /// <summary>
    /// 현재 체력 반환
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
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
        return (float)currentHealth / maxHealth;
    }
    
    /// <summary>
    /// 적군이 죽었는지 확인
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
    
    /// <summary>
    /// 적군이 성에 도달했는지 확인
    /// </summary>
    public bool HasReachedCastle()
    {
        return hasReachedCastle;
    }
} 