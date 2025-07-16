using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 화살의 XR 상호작용을 담당하는 스크립트
/// XR Socket Interactor와 연동하여 활에 장전될 수 있도록 함
/// </summary>
public class ArrowInteractable : MonoBehaviour
{
    [Header("Arrow Settings")]
    [Tooltip("화살의 데미지")]
    public int damage = 10;

    [Tooltip("화살이 파괴되기까지의 시간")]
    public float destroyDelay = 10f;

    [Header("Physics")]
    [Tooltip("화살의 무게")]
    public float arrowMass = 0.1f;

    [Tooltip("화살의 드래그")]
    public float arrowDrag = 0.1f;

    [Header("Effects")]
    [Tooltip("화살이 맞았을 때의 이펙트")]
    public GameObject hitEffectPrefab;

    [Tooltip("화살이 맞았을 때의 소리")]
    public AudioClip hitSound;

    // 내부 변수들
    private XRGrabInteractable _grabInteractable;
    private Rigidbody _rigidbody;
    private AudioSource _audioSource;
    private bool _hasHit = false;

    void Start()
    {
        InitializeArrow();
    }

    /// <summary>
    /// 화살 초기화
    /// </summary>
    void InitializeArrow()
    {
        // XR Grab Interactable 설정
        _grabInteractable = GetComponent<XRGrabInteractable>();
        if (_grabInteractable == null)
        {
            _grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }

        // Rigidbody 설정
        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        // Rigidbody 속성 설정
        _rigidbody.mass = arrowMass;
        _rigidbody.drag = arrowDrag;
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;

        // AudioSource 설정
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 이벤트 연결
        _grabInteractable.selectEntered.AddListener(OnGrabbed);
        _grabInteractable.selectExited.AddListener(OnReleased);
    }

    /// <summary>
    /// 화살이 잡혔을 때 호출
    /// </summary>
    void OnGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("화살이 잡혔습니다!");

        // 화살이 잡혔을 때 물리 비활성화 (소켓에 들어갈 때까지)
        _rigidbody.isKinematic = true;
        _rigidbody.useGravity = false;
    }

    /// <summary>
    /// 화살이 놓아졌을 때 호출
    /// </summary>
    void OnReleased(SelectExitEventArgs args)
    {
        Debug.Log("화살이 놓아졌습니다!");

        // 화살이 놓아졌을 때 물리 활성화
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
    }

    /// <summary>
    /// 화살이 다른 오브젝트와 충돌했을 때 호출
    /// </summary>
    void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;

        _hasHit = true;

        // 충돌 지점에 이펙트 생성
        if (hitEffectPrefab != null)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            Quaternion hitRotation = Quaternion.LookRotation(collision.contacts[0].normal);
            GameObject hitEffect = Instantiate(hitEffectPrefab, hitPoint, hitRotation);
            Destroy(hitEffect, 3f);
        }

        // 히트 사운드 재생
        if (hitSound != null)
        {
            _audioSource.PlayOneShot(hitSound);
        }

        // 타겟에 데미지 적용
        TargetController target = collision.gameObject.GetComponent<TargetController>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }

        // 화살 파괴
        Destroy(gameObject, destroyDelay);
    }

    /// <summary>
    /// 화살이 화면 밖으로 나갔을 때 자동 파괴
    /// </summary>
    void OnBecameInvisible()
    {
        if (!_hasHit)
        {
            Destroy(gameObject, 2f);
        }
    }

    void OnDestroy()
    {
        // 이벤트 연결 해제
        if (_grabInteractable != null)
        {
            _grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            _grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}