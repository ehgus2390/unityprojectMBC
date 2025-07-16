using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화살의 충돌, 파괴, 이펙트, 점수 처리를 담당하는 스크립트
/// 화살이 타겟에 맞았을 때의 모든 로직을 처리합니다.
/// </summary>
public class ArrowController : MonoBehaviour
{
    [Header("Arrow Settings")]
    [Tooltip("화살이 가하는 데미지량")]
    public int damage = 10;

    [Tooltip("화살이 충돌 후 파괴되기까지의 지연 시간 (초)")]
    public float destroyDelay = 5f;

    [Header("Effects")]
    [Tooltip("화살이 타겟에 맞았을 때 생성될 이펙트 프리팹")]
    public GameObject hitEffectPrefab;

    [Tooltip("타겟이 파괴될 때 생성될 이펙트 프리팹")]
    public GameObject destroyEffectPrefab;

    [Tooltip("화살이 타겟에 맞았을 때 재생될 사운드")]
    public AudioClip hitSound;

    [Tooltip("타겟이 파괴될 때 재생될 사운드")]
    public AudioClip destroySound;

    [Header("Score")]
    [Tooltip("타겟을 맞췄을 때 얻는 기본 점수")]
    public int hitScore = 10;

    [Tooltip("타겟을 파괴했을 때 얻는 추가 점수")]
    public int destroyScore = 50;

    // 내부 변수들
    /// <summary>화살이 이미 충돌했는지 확인하는 플래그</summary>
    private bool _hasHit = false;

    /// <summary>사운드 재생을 위한 AudioSource 컴포넌트</summary>
    private AudioSource _audioSource;

    /// <summary>물리 처리를 위한 Rigidbody 컴포넌트</summary>
    private Rigidbody _rb;

    /// <summary>
    /// 스크립트 초기화 시 호출되는 함수
    /// AudioSource와 Rigidbody 컴포넌트를 가져오거나 추가합니다.
    /// </summary>
    void Start()
    {
        // AudioSource 컴포넌트 가져오기 (없으면 추가)
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Rigidbody 컴포넌트 가져오기
        _rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// 화살이 다른 오브젝트와 충돌했을 때 호출되는 함수
    /// 충돌 처리, 이펙트 생성, 점수 처리, 파괴 로직을 수행합니다.
    /// </summary>
    /// <param name="collision">충돌 정보를 담고 있는 Collision 객체</param>
    void OnCollisionEnter(Collision collision)
    {
        // 이미 충돌한 화살은 중복 처리 방지
        if (!_hasHit)
        {
            _hasHit = true;

            // 화살 정지 처리 (물리 효과 비활성화)
            if (_rb != null)
            {
                _rb.isKinematic = true;
            }

            // 충돌 지점에 히트 이펙트 생성
            if (hitEffectPrefab != null)
            {
                Vector3 hitPoint = collision.contacts[0].point; // 충돌 지점
                Quaternion hitRotation = Quaternion.LookRotation(collision.contacts[0].normal); // 충돌 방향
                GameObject hitEffect = Instantiate(hitEffectPrefab, hitPoint, hitRotation);
                Destroy(hitEffect, 3f); // 3초 후 이펙트 제거
            }

            // 히트 사운드 재생
            if (hitSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(hitSound);
            }

            // 점수 처리
            ProcessScore(collision.gameObject);

            // 대상 오브젝트 처리 (데미지, 파괴 등)
            ProcessTarget(collision.gameObject);

            // 화살 파괴 (지연 시간 후)
            Destroy(gameObject, destroyDelay);
        }
    }

    /// <summary>
    /// 충돌한 타겟에 대한 점수 처리를 수행하는 함수
    /// 타겟의 태그에 따라 다른 점수를 부여합니다.
    /// </summary>
    /// <param name="target">충돌한 타겟 오브젝트</param>
    void ProcessScore(GameObject target)
    {
        // 씬에서 ScoreManager 찾기
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            // 타겟의 태그에 따라 다른 점수 부여
            if (target.CompareTag("Enemy"))
            {
                scoreManager.AddScore(hitScore); // 적 타겟
            }
            else if (target.CompareTag("Target"))
            {
                scoreManager.AddScore(hitScore); // 일반 타겟
            }
            else
            {
                scoreManager.AddScore(5); // 기본 점수 (기타 오브젝트)
            }
        }
    }

    /// <summary>
    /// 충돌한 타겟에 대한 데미지 및 파괴 처리를 수행하는 함수
    /// DestructibleObject와 HealthSystem을 지원합니다.
    /// </summary>
    /// <param name="target">충돌한 타겟 오브젝트</param>
    void ProcessTarget(GameObject target)
    {
        // 타겟이 적군인지 확인 (VR 디펜스 게임용)
        EnemyController enemy = target.GetComponent<EnemyController>();
        if (enemy != null)
        {
            // 적군에 데미지 적용
            enemy.TakeDamage(damage);

            // 파괴 이펙트 생성
            if (destroyEffectPrefab != null)
            {
                Vector3 targetPosition = target.transform.position;
                GameObject destroyEffect = Instantiate(destroyEffectPrefab, targetPosition, Quaternion.identity);
                Destroy(destroyEffect, 5f);
            }

            // 파괴 사운드 재생
            if (destroySound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(destroySound);
            }

            // 추가 점수 (파괴 보너스)
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(destroyScore);
            }

            return; // 적군 처리가 완료되었으므로 다른 처리 중단
        }
        //// 타겟이 파괴 가능한 오브젝트인지 확인
        //DestructibleObject destructible = target.GetComponent<DestructibleObject>();
        //if (destructible != null)
        //{
        //    // 데미지 적용
        //    destructible.TakeDamage(damage);

        //    // 파괴 이펙트 생성
        //    if (destroyEffectPrefab != null)
        //    {
        //        Vector3 targetPosition = target.transform.position;
        //        GameObject destroyEffect = Instantiate(destroyEffectPrefab, targetPosition, Quaternion.identity);
        //        Destroy(destroyEffect, 5f);
        //    }

        //    // 파괴 사운드 재생
        //    if (destroySound != null && _audioSource != null)
        //    {
        //        _audioSource.PlayOneShot(destroySound);
        //    }

        //    // 추가 점수 (파괴 보너스)
        //    ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        //    if (scoreManager != null)
        //    {
        //        scoreManager.AddScore(destroyScore);
        //    }
        //}

        // 타겟이 체력 시스템을 가진 오브젝트인지 확인
        HealthSystem healthSystem = target.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.TakeDamage(damage);
        }
    }

    /// <summary>
    /// 화살이 화면 밖으로 나갔을 때 자동으로 파괴하는 함수
    /// 메모리 누수 방지와 성능 최적화를 위해 사용됩니다.
    /// </summary>
    void OnBecameInvisible()
    {
        // 아직 충돌하지 않은 화살만 파괴 (충돌한 화살은 이미 파괴 예정)
        if (!_hasHit)
        {
            Destroy(gameObject, 2f); // 2초 후 파괴
        }
    }
}