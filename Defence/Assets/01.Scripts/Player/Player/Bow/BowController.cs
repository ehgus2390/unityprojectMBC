using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BowController : MonoBehaviour
{
    [Header("String & Nocking")]
    [SerializeField] private LineRenderer bowStringRenderer; // 활 시위를 표시할 라인 렌더러
    [SerializeField] private Transform stringStartPoint;     // 시위 시작점
    [SerializeField] private Transform stringEndPoint;       // 시위 끝점
    [SerializeField] private XRSocketInteractor nockSocket;  // 화살을 장전할 소켓

    [Header("Arrow")]
    [SerializeField] private GameObject arrowPrefab;         // 화살 프리팹
    [SerializeField] private Transform arrowSpawnPoint;      // 화살 생성 위치
    private IXRSelectInteractable nockedArrow = null; // 장전된 화살
    private bool isArrowNocked = false;               // 화살이 장전되었는지 상태

    [Header("Shooting")]
    [SerializeField] private float shootingForceMultiplier = 20f; // 발사 힘 배수
    [SerializeField] private float maxPullDistance = 0.5f;        // 최대 당김 거리

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = true;         // 디버그 로그 활성화

    private void Awake()
    {
        // XR Socket Interactor의 이벤트에 함수를 연결합니다.
        nockSocket.selectEntered.AddListener(OnArrowNocked);
        nockSocket.selectExited.AddListener(OnArrowRemoved);

        // 초기 활 시위 위치 설정
        ResetBowString();
    }

    private void OnDestroy()
    // 이벤트 제거 시 리스너를 제거합니다.
    {
        nockSocket.selectEntered.RemoveListener(OnArrowNocked);
        nockSocket.selectExited.RemoveListener(OnArrowRemoved);
    }

    // 매 프레임 호출
    void Update()
    {
        if (isArrowNocked && nockedArrow != null)
        {
            // 디버그 로그 추가
            if (enableDebugLogs)
                Debug.Log("Update: 화살이 장전되어 있습니다. 손 위치를 찾습니다.");

            // 화살이 장전되어 있다면, 화살을 잡고 있는 손의 위치를 찾아서 시위를 업데이트합니다.
            IXRSelectInteractor hand = nockedArrow.firstInteractorSelecting;
            if (hand != null)
            {
                // 손의 위치로 활 시위를 업데이트
                UpdateBowString(hand.transform.position);

                if (enableDebugLogs)
                    Debug.Log($"손 위치: {hand.transform.position}, 시위 업데이트됨");
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogWarning("손을 찾을 수 없습니다!");
            }
        }
        else
        {
            // 화살이 없으면 시위를 기본 위치로 되돌립니다.
            ResetBowString();
        }
    }

    // 화살이 소켓에 들어가서 장전될 때 호출되는 함수
    private void OnArrowNocked(SelectEnterEventArgs args)
    {
        // 디버그 로그 추가
        if (enableDebugLogs)
            Debug.Log("화살이 소켓에 들어가서 장전되었습니다! (OnArrowNocked 호출)");

        nockedArrow = args.interactableObject;
        isArrowNocked = true;

        // 화살이 소켓에서 나갈 때(발사되거나 그냥 제거될 때) Shoot 함수를 호출하도록 이벤트를 연결
        nockedArrow.selectExited.AddListener(Shoot);

        //if (enableDebugLogs)
        //    Debug.Log($"화살 장전 완료: {nockedArrow.name}");
    }

    // 화살이 소켓에서 나갈 때(발사되거나 그냥 제거될 때) 호출
    private void OnArrowRemoved(SelectExitEventArgs args)
    {
        // 이벤트 리스너 제거
        if (args.interactableObject == nockedArrow)
        {
            nockedArrow.selectExited.RemoveListener(Shoot);
            ResetBowString();
            nockedArrow = null;
            isArrowNocked = false;

            if (enableDebugLogs)
                Debug.Log("화살이 제거되었습니다.");
        }
    }

    // 화살 발사 함수
    private void Shoot(SelectExitEventArgs args)
    {
        if (enableDebugLogs)
            Debug.Log("화살 발사! (Shoot 함수 호출)");

        // args.interactorObject는 화살을 잡고 있는 손(컨트롤러)
        // 당김 거리를 계산합니다 (손과 소켓의 거리)
        float pullDistance = Vector3.Distance(args.interactorObject.transform.position, nockSocket.transform.position);
        float clampedPullDistance = Mathf.Clamp(pullDistance, 0f, maxPullDistance);
        float finalForce = clampedPullDistance * shootingForceMultiplier;

        if (enableDebugLogs)
            Debug.Log($"당김 거리: {pullDistance}, 클램프된 거리: {clampedPullDistance}, 최종 힘: {finalForce}");

        // 화살을 소켓에서 분리하고 물리를 활성화
        Rigidbody arrowRigidbody = nockedArrow.transform.GetComponent<Rigidbody>();
        if (arrowRigidbody != null)
        {
            arrowRigidbody.isKinematic = false;
            arrowRigidbody.useGravity = true;

            // 화살의 전방 방향으로 힘을 가합니다.
            Vector3 shootDirection = nockedArrow.transform.forward;
            arrowRigidbody.AddForce(shootDirection * finalForce, ForceMode.Impulse);

            if (enableDebugLogs)
                Debug.Log($"화살에 힘 적용: {shootDirection * finalForce}");
        }
        else
        {
            Debug.LogError("화살에 Rigidbody가 없습니다!");
        }
    }

    // 시위(Line Renderer)를 기본 위치로 되돌림
    private void ResetBowString()
    {
        if (bowStringRenderer != null)
        {
            bowStringRenderer.positionCount = 2;
            bowStringRenderer.SetPosition(0, stringStartPoint.position);
            bowStringRenderer.SetPosition(1, stringEndPoint.position);
        }
    }

    // 시위를 손의 위치로 업데이트
    private void UpdateBowString(Vector3 pullPosition)
    {
        if (bowStringRenderer != null)
        {
            // 당김 거리를 제한
            Vector3 direction = (pullPosition - nockSocket.transform.position).normalized;
            float distance = Vector3.Distance(nockSocket.transform.position, pullPosition);
            float clampedDistance = Mathf.Clamp(distance, 0f, maxPullDistance);
            Vector3 clampedPullPosition = nockSocket.transform.position + direction * clampedDistance;

            bowStringRenderer.positionCount = 3;
            bowStringRenderer.SetPosition(0, stringStartPoint.position);
            bowStringRenderer.SetPosition(1, clampedPullPosition);
            bowStringRenderer.SetPosition(2, stringEndPoint.position);
        }
    }

    // 수동으로 화살 생성 (테스트용)
    [ContextMenu("Create Arrow")]
    public void CreateArrow()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            GameObject newArrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
            Debug.Log("화살이 수동으로 생성되었습니다.");
        }
        else
        {
            Debug.LogError("화살 프리팹 또는 생성 위치가 설정되지 않았습니다!");
        }
    }

    // 현재 상태 정보 반환
    public bool IsArrowNocked() => isArrowNocked;
    public float GetPullDistance()
    {
        if (isArrowNocked && nockedArrow != null)
        {
            IXRSelectInteractor hand = nockedArrow.firstInteractorSelecting;
            if (hand != null)
            {
                return Vector3.Distance(hand.transform.position, nockSocket.transform.position);
            }
        }
        return 0f;
    }
}
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;

///// <summary>
///// VR 활 컨트롤러 - XR Interaction Toolkit과 연동 v
///// 활 시위 당기기, 화살 발사, 물리 시뮬레이션을 담당
///// </summary>
//public class BowController : MonoBehaviour
//{
//    [Header("Bow References")]
//    [Tooltip("활 시위의 고정점 (활 몸체에 연결된 부분)")]
//    public Transform stringAttachPoint;

//    [Tooltip("화살이 시작되는 위치")]
//    public Transform arrowStartPoint;

//    [Tooltip("화살 프리팹")]
//    public GameObject arrowPrefab;

//    [Header("Bow Physics")]
//    [Tooltip("활 시위의 최대 당김 거리")]
//    public float maxPullDistance = 0.5f;

//    [Tooltip("활 시위의 탄성 계수 (높을수록 더 강한 발사)")]
//    public float bowTension = 1000f;

//    [Tooltip("화살 발사 속도 배수")]
//    public float arrowSpeedMultiplier = 1.5f;

//    [Header("Visual Feedback")]
//    [Tooltip("활 시위를 시각적으로 표현하는 라인 렌더러")]
//    public LineRenderer bowString;

//    [Tooltip("활 시위의 색상")]
//    public Color stringColor = Color.white;

//    [Header("Audio")]
//    [Tooltip("활 시위 당기는 소리")]
//    public AudioClip pullSound;

//    [Tooltip("화살 발사 소리")]
//    public AudioClip releaseSound;

//    [Tooltip("화살 장전 소리")]
//    public AudioClip nockSound;

//    // 내부 변수들
//    private GameObject _currentArrow;
//    private bool _isStringPulled = false;
//    private Transform _pullingHand;
//    private float _currentPullDistance = 0f;
//    private Vector3 _originalStringPosition;
//    private AudioSource _audioSource;
//    private XRGrabInteractable _bowGrabInteractable;

//    // 이벤트
//    public System.Action<float> OnPullStrengthChanged;
//    public System.Action OnArrowReleased;

//    void Start()
//    {
//        InitializeBow();
//    }

//    void Update()
//    {
//        UpdateBowString();
//        UpdatePullPhysics();
//    }

//    /// <summary>
//    /// 활 초기화
//    /// </summary>
//    void InitializeBow()
//    {
//        // AudioSource 설정
//        _audioSource = GetComponent<AudioSource>();
//        if (_audioSource == null)
//        {
//            _audioSource = gameObject.AddComponent<AudioSource>();
//        }

//        // XR Grab Interactable 설정
//        _bowGrabInteractable = GetComponent<XRGrabInteractable>();
//        if (_bowGrabInteractable == null)
//        {
//            _bowGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
//        }

//        // 활 시위 라인 렌더러 설정
//        SetupBowString();

//        // 초기 활 시위 위치 저장
//        _originalStringPosition = stringAttachPoint.position;
//    }

//    /// <summary>
//    /// 활 시위 라인 렌더러 설정
//    /// </summary>
//    void SetupBowString()
//    {
//        if (bowString == null)
//        {
//            bowString = gameObject.AddComponent<LineRenderer>();
//        }

//        bowString.material = new Material(Shader.Find("Sprites/Default"));
//        //bowString.color = stringColor;
//        bowString.startWidth = 0.01f;
//        bowString.endWidth = 0.01f;
//        bowString.positionCount = 2;
//        bowString.useWorldSpace = true;
//    }

//    /// <summary>
//    /// 활 시위 시각적 업데이트
//    /// </summary>
//    void UpdateBowString()
//    {
//        if (bowString != null)
//        {
//            Vector3 startPos = _originalStringPosition;
//            Vector3 endPos = _isStringPulled ? _pullingHand.position : _originalStringPosition;

//            bowString.SetPosition(0, startPos);
//            bowString.SetPosition(1, endPos);
//        }
//    }

//    /// <summary>
//    /// 활 시위 당기기 물리 업데이트
//    /// </summary>
//    void UpdatePullPhysics()
//    {
//        if (_isStringPulled && _pullingHand != null)
//        {
//            // 현재 당김 거리 계산
//            _currentPullDistance = Vector3.Distance(_originalStringPosition, _pullingHand.position);
//            _currentPullDistance = Mathf.Clamp(_currentPullDistance, 0f, maxPullDistance);

//            // 당김 강도 이벤트 호출 (0-1 범위)
//            float pullStrength = _currentPullDistance / maxPullDistance;
//            OnPullStrengthChanged?.Invoke(pullStrength);

//            // 활 시위 위치 업데이트
//            stringAttachPoint.position = Vector3.Lerp(_originalStringPosition, _pullingHand.position, 0.5f);
//        }
//    }

//    /// <summary>
//    /// 활 시위 당기기 시작
//    /// </summary>
//    /// <param name="hand">당기는 손의 Transform</param>
//    public void StartPull(Transform hand)
//    {
//        if (!_isStringPulled)
//        {
//            _isStringPulled = true;
//            _pullingHand = hand;
//            _currentPullDistance = 0f;

//            // 화살 생성
//            CreateArrow();

//            // 당기는 소리 재생
//            if (pullSound != null)
//            {
//                _audioSource.PlayOneShot(pullSound);
//            }
//        }
//    }

//    /// <summary>
//    /// 화살 생성
//    /// </summary>
//    void CreateArrow()
//    {
//        if (arrowPrefab != null && _currentArrow == null)
//        {
//            _currentArrow = Instantiate(arrowPrefab, arrowStartPoint.position, arrowStartPoint.rotation, arrowStartPoint);

//            // 화살의 Rigidbody를 비활성화 (발사 전까지)
//            Rigidbody arrowRb = _currentArrow.GetComponent<Rigidbody>();
//            if (arrowRb != null)
//            {
//                arrowRb.isKinematic = true;
//            }

//            // 장전 소리 재생
//            if (nockSound != null)
//            {
//                _audioSource.PlayOneShot(nockSound);
//            }
//        }
//    }

//    /// <summary>
//    /// 활 시위 놓기 (화살 발사)
//    /// </summary>
//    public void ReleasePull()
//    {
//        if (_isStringPulled && _currentArrow != null)
//        {
//            // 화살 발사
//            FireArrow();

//            // 활 시위 원위치
//            ResetBowString();

//            // 발사 소리 재생
//            if (releaseSound != null)
//            {
//                _audioSource.PlayOneShot(releaseSound);
//            }

//            // 이벤트 호출
//            OnArrowReleased?.Invoke();
//        }

//        _isStringPulled = false;
//        _pullingHand = null;
//        _currentArrow = null;
//    }

//    /// <summary>
//    /// 화살 발사
//    /// </summary>
//    void FireArrow()
//    {
//        // 화살을 부모에서 분리
//        _currentArrow.transform.parent = null;

//        // Rigidbody 활성화
//        Rigidbody arrowRb = _currentArrow.GetComponent<Rigidbody>();
//        if (arrowRb != null)
//        {
//            arrowRb.isKinematic = false;

//            // 발사 방향 계산 (활 시위 방향)
//            Vector3 fireDirection = (arrowStartPoint.position - stringAttachPoint.position).normalized;

//            // 발사 힘 계산 (당김 거리에 비례)
//            float pullStrength = _currentPullDistance / maxPullDistance;
//            float fireForce = bowTension * pullStrength * arrowSpeedMultiplier;

//            // 화살에 힘 적용
//            arrowRb.AddForce(fireDirection * fireForce, ForceMode.Impulse);

//            // 화살 회전 추가 (더 현실적인 궤적)
//            arrowRb.AddTorque(arrowRb.transform.right * 10f, ForceMode.Impulse);
//        }
//    }

//    /// <summary>
//    /// 활 시위 원위치
//    /// </summary>
//    void ResetBowString()
//    {
//        stringAttachPoint.position = _originalStringPosition;
//        _currentPullDistance = 0f;
//        OnPullStrengthChanged?.Invoke(0f);
//    }

//    /// <summary>
//    /// 현재 당김 강도 반환 (0-1)
//    /// </summary>
//    public float GetPullStrength()
//    {
//        return _currentPullDistance / maxPullDistance;
//    }

//    /// <summary>
//    /// 활이 잡혀있는지 확인
//    /// </summary>
//    public bool IsGrabbed()
//    {
//        return _bowGrabInteractable != null && _bowGrabInteractable.isSelected;
//    }
//}