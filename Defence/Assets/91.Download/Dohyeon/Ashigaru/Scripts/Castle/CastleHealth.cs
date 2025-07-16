using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 성의 체력을 관리하고 게임 오버를 처리하는 스크립트
/// </summary>
public class CastleHealth : MonoBehaviour
{
    [Header("Castle Settings")]
    [Tooltip("성의 최대 체력")]
    public int maxHealth = 1000;

    [Tooltip("성의 현재 체력")]
    [SerializeField] private int currentHealth;

    [Header("Events")]
    [Tooltip("성의 체력이 변경되었을 때 호출되는 이벤트")]
    public UnityEvent<int, int> OnHealthChanged; // currentHealth, maxHealth

    [Tooltip("성의 체력이 0이 되었을 때 호출되는 이벤트")]
    public UnityEvent OnGameOver;

    [Header("Effects")]
    [Tooltip("성에 데미지를 받았을 때의 이펙트")]
    public GameObject damageEffectPrefab;

    [Tooltip("성의 체력이 낮을 때의 경고 이펙트")]
    public GameObject warningEffectPrefab;

    [Header("Audio")]
    [Tooltip("성에 데미지를 받았을 때의 사운드")]
    public AudioClip damageSound;

    [Tooltip("게임 오버 사운드")]
    public AudioClip gameOverSound;

    // 내부 변수들
    private AudioSource _audioSource;
    private bool _isGameOver = false;
    private GameObject _warningEffect;

    // 프로퍼티
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public float HealthRatio => (float)currentHealth / maxHealth;
    public bool IsGameOver => _isGameOver;

    void Start()
    {
        InitializeCastle();
    }

    /// <summary>
    /// 성 초기화
    /// </summary>
    void InitializeCastle()
    {
        currentHealth = maxHealth;

        // AudioSource 컴포넌트 가져오기
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 이벤트 호출
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// 성에 데미지를 받았을 때 호출
    /// </summary>
    /// <param name="damage">받은 데미지량</param>
    public void TakeDamage(int damage)
    {
        Debug.Log("성문에 데미지");
        CastleHealthUI castleHealthUI = GetComponent<CastleHealthUI>();
        if (_isGameOver) return;

        currentHealth -= damage;

        // 체력이 0 이하가 되면 0으로 고정
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

       

        // 데미지 이펙트 생성
        if (damageEffectPrefab != null)
        {
            GameObject damageEffect = Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
            Destroy(damageEffect, 2f);
        }

        // 데미지 사운드 재생
        if (damageSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(damageSound);
        }

        // 체력 변경 이벤트 호출
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 체력이 낮을 때 경고 이펙트 표시
        CheckWarningEffect();

        // 체력이 0이 되면 게임 오버
        if (currentHealth <= 0)
        {
            GameOver();
        }
       
    }

    /// <summary>
    /// 성의 체력을 회복
    /// </summary>
    /// <param name="healAmount">회복량</param>
    public void Heal(int healAmount)
    {
        if (_isGameOver) return;

        currentHealth += healAmount;

        // 체력이 최대치를 넘지 않도록 제한
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // 체력 변경 이벤트 호출
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // 경고 이펙트 체크
        CheckWarningEffect();
    }

    /// <summary>
    /// 체력이 낮을 때 경고 이펙트 표시
    /// </summary>
    void CheckWarningEffect()
    {
        float healthRatio = HealthRatio;

        // 체력이 30% 이하일 때 경고 이펙트 표시
        if (healthRatio <= 0.3f && _warningEffect == null && warningEffectPrefab != null)
        {
            _warningEffect = Instantiate(warningEffectPrefab, transform.position, Quaternion.identity);
        }
        // 체력이 30% 이상일 때 경고 이펙트 제거
        else if (healthRatio > 0.3f && _warningEffect != null)
        {
            Destroy(_warningEffect);
            _warningEffect = null;
        }
    }

    /// <summary>
    /// 게임 오버 처리
    /// </summary>
    void GameOver()
    {
        if (_isGameOver) return;

        _isGameOver = true;

        // 게임 오버 사운드 재생
        if (gameOverSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(gameOverSound);
        }

        // 경고 이펙트 제거
        if (_warningEffect != null)
        {
            Destroy(_warningEffect);
            _warningEffect = null;
        }

        // 게임 오버 이벤트 호출
        OnGameOver?.Invoke();

        Debug.Log("게임 오버! 성이 파괴되었습니다.");
    }

    /// <summary>
    /// 성의 체력을 완전히 회복
    /// </summary>
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        CheckWarningEffect();
    }

    /// <summary>
    /// 성의 최대 체력 설정
    /// </summary>
    /// <param name="newMaxHealth">새로운 최대 체력</param>
    public void SetMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void OnDestroy()
    {
        // 경고 이펙트 정리
        if (_warningEffect != null)
        {
            Destroy(_warningEffect);
        }
    }
}