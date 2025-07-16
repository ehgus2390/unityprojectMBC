using UnityEngine;
using System.Collections;

/// <summary>
/// 활쏘기 게임용 타겟 컨트롤러
/// 타겟의 점수, 파괴, 애니메이션을 관리
/// </summary>
public class TargetController : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("타겟의 최대 체력")]
    public int maxHealth = 100;

    [Tooltip("타겟을 맞췄을 때 얻는 점수")]
    public int hitScore = 10;

    [Tooltip("타겟을 파괴했을 때 얻는 추가 점수")]
    public int destroyScore = 50;

    [Header("Target Behavior")]
    [Tooltip("타겟이 움직이는지 여부")]
    public bool isMoving = false;

    [Tooltip("타겟의 이동 속도")]
    public float moveSpeed = 2f;

    [Tooltip("타겟의 이동 범위")]
    public float moveRange = 5f;

    [Tooltip("타겟이 회전하는지 여부")]
    public bool isRotating = false;

    [Tooltip("타겟의 회전 속도")]
    public float rotationSpeed = 30f;

    [Header("Visual Effects")]
    [Tooltip("타겟이 맞았을 때의 이펙트")]
    public GameObject hitEffectPrefab;

    [Tooltip("타겟이 파괴될 때의 이펙트")]
    public GameObject destroyEffectPrefab;

    [Tooltip("타겟의 원래 색상")]
    public Color originalColor = Color.white;

    [Tooltip("타겟이 맞았을 때의 색상")]
    public Color hitColor = Color.red;

    [Header("Audio")]
    [Tooltip("타겟이 맞았을 때의 소리")]
    public AudioClip hitSound;

    [Tooltip("타겟이 파괴될 때의 소리")]
    public AudioClip destroySound;

    // 내부 변수들
    private int _currentHealth;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    private Renderer _renderer;
    private AudioSource _audioSource;
    private bool _isDestroyed = false;

    // 이벤트
    public System.Action<int> OnTargetHit;
    public System.Action OnTargetDestroyed;

    void Start()
    {
        InitializeTarget();
    }

    /// <summary>
    /// 타겟 초기화
    /// </summary>
    void InitializeTarget()
    {
        _currentHealth = maxHealth;
        _startPosition = transform.position;
        _startRotation = transform.rotation;

        // 렌더러 가져오기
        _renderer = GetComponent<Renderer>();
        if (_renderer != null)
        {
            _renderer.material.color = originalColor;
        }

        // AudioSource 설정
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 태그 설정
        gameObject.tag = "Target";
    }

    void Update()
    {
        if (!_isDestroyed)
        {
            UpdateMovement();
            UpdateRotation();
        }
    }

    /// <summary>
    /// 타겟 이동 업데이트
    /// </summary>
    void UpdateMovement()
    {
        if (isMoving)
        {
            float time = Time.time * moveSpeed;
            float offset = Mathf.Sin(time) * moveRange;
            Vector3 newPosition = _startPosition + Vector3.right * offset;
            transform.position = newPosition;
        }
    }

    /// <summary>
    /// 타겟 회전 업데이트
    /// </summary>
    void UpdateRotation()
    {
        if (isRotating)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 타겟이 맞았을 때 호출
    /// </summary>
    /// <param name="damage">받은 데미지</param>
    public void TakeDamage(int damage)
    {
        if (_isDestroyed) return;

        _currentHealth -= damage;

        // 히트 이벤트 호출
        OnTargetHit?.Invoke(damage);

        // 히트 이펙트 생성
        if (hitEffectPrefab != null)
        {
            GameObject hitEffect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(hitEffect, 2f);
        }

        // 히트 사운드 재생
        if (hitSound != null)
        {
            _audioSource.PlayOneShot(hitSound);
        }

        // 시각적 피드백
        StartCoroutine(HitVisualFeedback());

        // 체력이 0 이하가 되면 파괴
        if (_currentHealth <= 0)
        {
            DestroyTarget();
        }
    }

    /// <summary>
    /// 타겟 파괴
    /// </summary>
    void DestroyTarget()
    {
        if (_isDestroyed) return;

        _isDestroyed = true;

        // 파괴 이벤트 호출
        OnTargetDestroyed?.Invoke();

        // 파괴 이펙트 생성
        if (destroyEffectPrefab != null)
        {
            GameObject destroyEffect = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            Destroy(destroyEffect, 3f);
        }

        // 파괴 사운드 재생
        if (destroySound != null)
        {
            _audioSource.PlayOneShot(destroySound);
        }

        // 점수 추가
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddScore(destroyScore);
        }

        // 타겟 비활성화 (이펙트 재생 후)
        StartCoroutine(DestroyAfterDelay(2f));
    }

    /// <summary>
    /// 히트 시각적 피드백
    /// </summary>
    IEnumerator HitVisualFeedback()
    {
        if (_renderer != null)
        {
            Color originalColor = _renderer.material.color;
            _renderer.material.color = hitColor;

            yield return new WaitForSeconds(0.1f);

            _renderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// 지연 후 파괴
    /// </summary>
    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 타겟 리셋
    /// </summary>
    public void ResetTarget()
    {
        _isDestroyed = false;
        _currentHealth = maxHealth;
        transform.position = _startPosition;
        transform.rotation = _startRotation;

        if (_renderer != null)
        {
            _renderer.material.color = originalColor;
        }

        gameObject.SetActive(true);
    }

    /// <summary>
    /// 현재 체력 반환
    /// </summary>
    public int GetCurrentHealth()
    {
        return _currentHealth;
    }

    /// <summary>
    /// 체력 비율 반환 (0-1)
    /// </summary>
    public float GetHealthRatio()
    {
        return (float)_currentHealth / maxHealth;
    }

    /// <summary>
    /// 타겟이 파괴되었는지 확인
    /// </summary>
    public bool IsDestroyed()
    {
        return _isDestroyed;
    }
}