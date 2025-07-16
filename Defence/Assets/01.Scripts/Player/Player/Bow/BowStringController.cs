//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;

///// <summary>
///// VR 활 시위 컨트롤러
///// XR 컨트롤러와 활 시위를 연동하여 당기기/놓기 동작을 처리
///// </summary>
//public class BowStringController : MonoBehaviour
//{
//    [Header("Bow Reference")]
//    [Tooltip("연동할 활 컨트롤러")]
//    public BowController bowController;

//    [Header("Interaction Settings")]
//    [Tooltip("활 시위와 상호작용할 수 있는 거리")]
//    public float interactionDistance = 0.1f;

//    [Tooltip("활 시위를 잡을 수 있는 영역")]
//    public Collider stringCollider;

//    [Header("Haptic Feedback")]
//    [Tooltip("활 시위 당길 때 햅틱 피드백 강도")]
//    public float pullHapticStrength = 0.5f;

//    [Tooltip("활 시위 놓을 때 햅틱 피드백 강도")]
//    public float releaseHapticStrength = 0.8f;

//    // 내부 변수들
//    private XRGrabInteractable _stringGrabInteractable;
//    private XRDirectInteractor _currentInteractor;
//    private bool _isBeingPulled = false;
//    private Vector3 _originalPosition;

//    void Start()
//    {
//        InitializeStringController();
//    }

//    /// <summary>
//    /// 활 시위 컨트롤러 초기화
//    /// </summary>
//    void InitializeStringController()
//    {
//        // 원본 위치 저장
//        _originalPosition = transform.position;

//        // XR Grab Interactable 설정
//        _stringGrabInteractable = GetComponent<XRGrabInteractable>();
//        if (_stringGrabInteractable == null)
//        {
//            _stringGrabInteractable = gameObject.AddComponent<XRGrabInteractable>();
//        }

//        // 이벤트 연결
//        _stringGrabInteractable.selectEntered.AddListener(OnStringGrabbed);
//        _stringGrabInteractable.selectExited.AddListener(OnStringReleased);
//        _stringGrabInteractable.hoverEntered.AddListener(OnStringHoverEnter);
//        _stringGrabInteractable.hoverExited.AddListener(OnStringHoverExit);

//        // 활 시위는 활과 함께 움직여야 하므로 부모 설정
//        if (bowController != null)
//        {
//            transform.SetParent(bowController.transform);
//        }
//    }

//    /// <summary>
//    /// 활 시위가 잡혔을 때 호출
//    /// </summary>
//    /// <param name="args">선택 이벤트 인자</param>
//    void OnStringGrabbed(SelectEnterEventArgs args)
//    {
//        _currentInteractor = args.interactorObject as XRDirectInteractor;
//        if (_currentInteractor != null && bowController != null)
//        {
//            _isBeingPulled = true;

//            // 활 시위 당기기 시작
//            bowController.StartPull(_currentInteractor.transform);

//            // 햅틱 피드백
//            SendHapticFeedback(pullHapticStrength);
//        }
//    }

//    /// <summary>
//    /// 활 시위가 놓아졌을 때 호출
//    /// </summary>
//    /// <param name="args">선택 해제 이벤트 인자</param>
//    void OnStringReleased(SelectExitEventArgs args)
//    {
//        if (_isBeingPulled && bowController != null)
//        {
//            _isBeingPulled = false;

//            // 활 시위 놓기 (화살 발사)
//            bowController.ReleasePull();

//            // 햅틱 피드백
//            SendHapticFeedback(releaseHapticStrength);
//        }

//        _currentInteractor = null;
//    }

//    /// <summary>
//    /// 활 시위에 호버 시작
//    /// </summary>
//    /// <param name="args">호버 이벤트 인자</param>
//    void OnStringHoverEnter(HoverEnterEventArgs args)
//    {
//        // 호버 시 시각적 피드백 (선택사항)
//        Renderer renderer = GetComponent<Renderer>();
//        if (renderer != null)
//        {
//            renderer.material.color = Color.yellow;
//        }
//    }

//    /// <summary>
//    /// 활 시위에서 호버 해제
//    /// </summary>
//    /// <param name="args">호버 해제 이벤트 인자</param>
//    void OnStringHoverExit(HoverExitEventArgs args)
//    {
//        // 호버 해제 시 원래 색상으로 복원
//        Renderer renderer = GetComponent<Renderer>();
//        if (renderer != null)
//        {
//            renderer.material.color = Color.white;
//        }
//    }

//    /// <summary>
//    /// 햅틱 피드백 전송
//    /// </summary>
//    /// <param name="strength">피드백 강도 (0-1)</param>
//    void SendHapticFeedback(float strength)
//    {
//        if (_currentInteractor != null)
//        {
//            XRBaseController controller = _currentInteractor.GetComponent<XRBaseController>();
//            if (controller != null)
//            {
//                controller.SendHapticImpulse(strength, 0.1f);
//            }
//        }
//    }

//    void Update()
//    {
//        // 활 시위가 당겨지고 있을 때 위치 업데이트
//        if (_isBeingPulled && _currentInteractor != null)
//        {
//            // 컨트롤러 위치로 활 시위 이동
//            transform.position = _currentInteractor.transform.position;

//            // 활 시위가 너무 멀리 당겨지지 않도록 제한
//            float distanceFromOriginal = Vector3.Distance(_originalPosition, transform.position);
//            if (distanceFromOriginal > bowController.maxPullDistance)
//            {
//                Vector3 direction = (transform.position - _originalPosition).normalized;
//                transform.position = _originalPosition + direction * bowController.maxPullDistance;
//            }
//        }
//        else if (!_isBeingPulled)
//        {
//            // 활 시위가 놓아졌을 때 원래 위치로 복원
//            transform.position = _originalPosition;
//        }
//    }

//    /// <summary>
//    /// 활 시위가 당겨지고 있는지 확인
//    /// </summary>
//    public bool IsBeingPulled()
//    {
//        return _isBeingPulled;
//    }

//    /// <summary>
//    /// 현재 당김 거리 반환
//    /// </summary>
//    public float GetCurrentPullDistance()
//    {
//        if (_isBeingPulled)
//        {
//            return Vector3.Distance(_originalPosition, transform.position);
//        }
//        return 0f;
//    }

//    void OnDestroy()
//    {
//        // 이벤트 연결 해제
//        if (_stringGrabInteractable != null)
//        {
//            _stringGrabInteractable.selectEntered.RemoveListener(OnStringGrabbed);
//            _stringGrabInteractable.selectExited.RemoveListener(OnStringReleased);
//            _stringGrabInteractable.hoverEntered.RemoveListener(OnStringHoverEnter);
//            _stringGrabInteractable.hoverExited.RemoveListener(OnStringHoverExit);
//        }
//    }
//}