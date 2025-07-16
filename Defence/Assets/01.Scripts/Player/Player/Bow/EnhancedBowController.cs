using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

/// <summary>
/// The Lab VR 스타일의 향상된 활 컨트롤러
/// 오른손에 화살 자동 생성, 자연스러운 시위 당기기, 정확한 발사 시스템
/// </summary>
public class EnhancedBowController : MonoBehaviour
{
    [Header("Bow References")]
    [Tooltip("활 시위를 표시할 라인 렌더러")]
    [SerializeField] private LineRenderer bowStringRenderer;

    [Tooltip("시위 시작점")]
    [SerializeField] private Transform stringStartPoint;

    [Tooltip("시위 끝점")]
    [SerializeField] private Transform stringEndPoint;

    [Tooltip("화살을 장전할 소켓")]
    [SerializeField] private XRSocketInteractor nockSocket;

    [Tooltip("시위 당김 감지 영역")]
    [SerializeField] private Transform stringPullArea;

    [Header("Arrow System")]
    [Tooltip("화살 프리팹")]
    [SerializeField] private GameObject arrowPrefab;

    [Tooltip("오른손 화살 생성 위치")]
    [SerializeField] private Transform rightHandArrowSpawn;

    [Tooltip("자동 화살 생성 간격 (초)")]
    [SerializeField] private float arrowSpawnInterval = 1.5f;

    [Tooltip("최대 보유 화살 개수")]
    [SerializeField] private int maxArrows = 10;

    [Header("Bow Physics")]
    [Tooltip("발사 힘 배수")]
    [SerializeField] private float shootingForceMultiplier = 25f;

    [Tooltip("최대 당김 거리")]
    [SerializeField] private float maxPullDistance = 0.6f;

    [Tooltip("시위 탄성 계수")]
    [SerializeField] private float stringTension = 1.2f;

    [Tooltip("화살 회전력")]
    [SerializeField] private float arrowSpinForce = 15f;

    [Header("Visual & Audio")]
    [Tooltip("시위 당김 사운드")]
    [SerializeField] private AudioClip pullSound;

    [Tooltip("화살 발사 사운드")]
    [SerializeField] private AudioClip releaseSound;

    [Tooltip("화살 장전 사운드")]
    [SerializeField] private AudioClip nockSound;

    [Tooltip("시위 색상")]
    [SerializeField] private Color stringColor = Color.white;

    [Tooltip("시위 두께")]
    [SerializeField] private float stringWidth = 0.005f;

    [Header("Debug")]
    [Tooltip("디버그 로그 활성화")]
    [SerializeField] private bool enableDebugLogs = true;

    // 내부 변수들
    private IXRSelectInteractable nockedArrow = null;
    private bool isArrowNocked = false;
    private bool isStringPulled = false;
    private Transform pullingHand = null;
    private float currentPullDistance = 0f;
    private Vector3 originalStringPosition;
    private AudioSource audioSource;
    private int currentArrowCount = 0;
    private Coroutine arrowSpawnCoroutine;
    private XRDirectInteractor rightHandInteractor;

    // 이벤트
    public System.Action<float> OnPullStrengthChanged;
    public System.Action OnArrowReleased;
    public System.Action<int> OnArrowCountChanged;

    void Start()
    {
        InitializeBow();
    }

    void Update()
    {
        UpdateBowString();
        UpdatePullPhysics();
    }

    /// <summary>
    /// 활 초기화
    /// </summary>
    void InitializeBow()
    {
        // AudioSource 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 오른손 컨트롤러 찾기
        FindRightHandController();

        // 시위 초기화
        SetupBowString();

        // 소켓 이벤트 연결
        if (nockSocket != null)
        {
            nockSocket.selectEntered.AddListener(OnArrowNocked);
            nockSocket.selectExited.AddListener(OnArrowRemoved);
        }

        // 시위 당김 감지 설정
        SetupStringPullDetection();

        // 자동 화살 생성 시작
        StartArrowSpawning();

        if (enableDebugLogs)
            Debug.Log("향상된 활 컨트롤러가 초기화되었습니다.");
    }

    /// <summary>
    /// 오른손 컨트롤러 찾기
    /// </summary>
    void FindRightHandController()
    {
        XRDirectInteractor[] interactors = FindObjectsOfType<XRDirectInteractor>();
        foreach (var interactor in interactors)
        {
            if (interactor.name.ToLower().Contains("right") ||
                interactor.name.ToLower().Contains("r_") ||
                interactor.name.ToLower().Contains("r "))
            {
                rightHandInteractor = interactor;
                break;
            }
        }

        if (rightHandInteractor == null && interactors.Length > 0)
        {
            rightHandInteractor = interactors[0];
            Debug.LogWarning("오른손 컨트롤러를 정확히 찾지 못했습니다. 첫 번째 컨트롤러를 사용합니다.");
        }

        // 오른손 화살 생성 위치 설정
        if (rightHandArrowSpawn == null && rightHandInteractor != null)
        {
            rightHandArrowSpawn = rightHandInteractor.transform;
        }
    }

    /// <summary>
    /// 시위 시각적 설정
    /// </summary>
    void SetupBowString()
    {
        if (bowStringRenderer == null)
        {
            bowStringRenderer = gameObject.AddComponent<LineRenderer>();
        }

        bowStringRenderer.material = new Material(Shader.Find("Sprites/Default"));
        bowStringRenderer.startColor = stringColor;
        bowStringRenderer.endColor = stringColor;
        bowStringRenderer.startWidth = stringWidth;
        bowStringRenderer.endWidth = stringWidth;
        bowStringRenderer.positionCount = 2;
        bowStringRenderer.useWorldSpace = true;

        // 원래 시위 위치 저장
        if (stringStartPoint != null && stringEndPoint != null)
        {
            originalStringPosition = (stringStartPoint.position + stringEndPoint.position) * 0.5f;
            ResetBowString();
        }
    }

    /// <summary>
    /// 시위 당김 감지 설정
    /// </summary>
    void SetupStringPullDetection()
    {
        if (stringPullArea == null)
        {
            // 시위 당김 감지 영역 생성
            GameObject pullArea = new GameObject("StringPullArea");
            pullArea.transform.SetParent(transform);
            pullArea.transform.position = originalStringPosition;

            SphereCollider pullCollider = pullArea.AddComponent<SphereCollider>();
            pullCollider.isTrigger = true;
            pullCollider.radius = 0.1f;

            StringPullDetector detector = pullArea.AddComponent<StringPullDetector>();
            detector.bowController = this;

            stringPullArea = pullArea.transform;
        }
    }

    /// <summary>
    /// 자동 화살 생성 시작
    /// </summary>
    void StartArrowSpawning()
    {
        if (arrowSpawnCoroutine != null)
        {
            StopCoroutine(arrowSpawnCoroutine);
        }
        arrowSpawnCoroutine = StartCoroutine(AutoArrowSpawn());
    }

    /// <summary>
    /// 자동 화살 생성 코루틴
    /// </summary>
    IEnumerator AutoArrowSpawn()
    {
        while (true)
        {
            if (currentArrowCount < maxArrows)
            {
                SpawnArrowInHand();
            }
            yield return new WaitForSeconds(arrowSpawnInterval);
        }
    }

    /// <summary>
    /// 오른손에 화살 생성
    /// </summary>
    void SpawnArrowInHand()
    {
        if (arrowPrefab == null || rightHandArrowSpawn == null) return;

        // 화살 생성 위치 (오른손 앞쪽)
        Vector3 spawnPos = rightHandArrowSpawn.position + rightHandArrowSpawn.forward * 0.15f;
        GameObject newArrow = Instantiate(arrowPrefab, spawnPos, rightHandArrowSpawn.rotation);

        // 화살 컴포넌트 설정
        SetupArrowComponents(newArrow);

        currentArrowCount++;
        OnArrowCountChanged?.Invoke(currentArrowCount);

        if (enableDebugLogs)
            Debug.Log($"오른손에 화살이 생성되었습니다. 현재 개수: {currentArrowCount}");
    }

    /// <summary>
    /// 화살 컴포넌트 설정
    /// </summary>
    void SetupArrowComponents(GameObject arrow)
    {
        // XR Grab Interactable
        XRGrabInteractable grabInteractable = arrow.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = arrow.AddComponent<XRGrabInteractable>();
        }

        // Rigidbody
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = arrow.AddComponent<Rigidbody>();
        }
        rb.mass = 0.1f;
        rb.drag = 0.1f;

        // ArrowController
        ArrowController arrowController = arrow.GetComponent<ArrowController>();
        if (arrowController == null)
        {
            arrowController = arrow.AddComponent<ArrowController>();
        }

        // ArrowInteractable
        ArrowInteractable arrowInteractable = arrow.GetComponent<ArrowInteractable>();
        if (arrowInteractable == null)
        {
            arrowInteractable = arrow.AddComponent<ArrowInteractable>();
        }

        // 화살 파괴 시 카운트 감소
        //arrowInteractable.OnArrowDestroyed += () => {
        //    currentArrowCount = Mathf.Max(0, currentArrowCount - 1);
        //    OnArrowCountChanged?.Invoke(currentArrowCount);
        //};

        // 초기 물리 비활성화 (자연스러운 잡기)
        rb.isKinematic = true;
        rb.useGravity = false;
        StartCoroutine(EnableArrowPhysics(rb, 0.3f));
    }

    /// <summary>
    /// 화살 물리 활성화 지연
    /// </summary>
    IEnumerator EnableArrowPhysics(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    /// <summary>
    /// 시위 시각적 업데이트
    /// </summary>
    void UpdateBowString()
    {
        if (bowStringRenderer == null) return;

        Vector3 startPos = stringStartPoint.position;
        Vector3 endPos = stringEndPoint.position;

        if (isStringPulled && pullingHand != null)
        {
            // 시위가 당겨진 상태
            Vector3 pullPos = pullingHand.position;
            Vector3 direction = (pullPos - originalStringPosition).normalized;
            float distance = Vector3.Distance(originalStringPosition, pullPos);
            float clampedDistance = Mathf.Clamp(distance, 0f, maxPullDistance);
            Vector3 clampedPullPos = originalStringPosition + direction * clampedDistance;

            bowStringRenderer.positionCount = 3;
            bowStringRenderer.SetPosition(0, startPos);
            bowStringRenderer.SetPosition(1, clampedPullPos);
            bowStringRenderer.SetPosition(2, endPos);
        }
        else
        {
            // 시위가 당겨지지 않은 상태
            bowStringRenderer.positionCount = 2;
            bowStringRenderer.SetPosition(0, startPos);
            bowStringRenderer.SetPosition(1, endPos);
        }
    }

    /// <summary>
    /// 시위 당김 물리 업데이트
    /// </summary>
    void UpdatePullPhysics()
    {
        if (isStringPulled && pullingHand != null)
        {
            currentPullDistance = Vector3.Distance(originalStringPosition, pullingHand.position);
            currentPullDistance = Mathf.Clamp(currentPullDistance, 0f, maxPullDistance);

            float pullStrength = currentPullDistance / maxPullDistance;
            OnPullStrengthChanged?.Invoke(pullStrength);
        }
    }

    /// <summary>
    /// 화살이 소켓에 장전될 때
    /// </summary>
    private void OnArrowNocked(SelectEnterEventArgs args)
    {
        nockedArrow = args.interactableObject;
        isArrowNocked = true;

        if (nockSound != null)
        {
            audioSource.PlayOneShot(nockSound);
        }

        if (enableDebugLogs)
            Debug.Log("화살이 활에 장전되었습니다.");
    }

    /// <summary>
    /// 화살이 소켓에서 제거될 때
    /// </summary>
    private void OnArrowRemoved(SelectExitEventArgs args)
    {
        if (args.interactableObject == nockedArrow)
        {
            ResetBowString();
            nockedArrow = null;
            isArrowNocked = false;
            isStringPulled = false;
            pullingHand = null;
            currentPullDistance = 0f;
        }
    }

    /// <summary>
    /// 시위 당김 시작 (StringPullDetector에서 호출)
    /// </summary>
    public void StartStringPull(Transform hand)
    {
        if (!isStringPulled && isArrowNocked)
        {
            isStringPulled = true;
            pullingHand = hand;
            currentPullDistance = 0f;

            if (pullSound != null)
            {
                audioSource.PlayOneShot(pullSound);
            }

            if (enableDebugLogs)
                Debug.Log("시위를 당기기 시작했습니다.");
        }
    }

    /// <summary>
    /// 시위 당김 해제 (화살 발사)
    /// </summary>
    public void ReleaseStringPull()
    {
        if (isStringPulled && isArrowNocked && nockedArrow != null)
        {
            FireArrow();
            ResetBowString();

            if (releaseSound != null)
            {
                audioSource.PlayOneShot(releaseSound);
            }

            OnArrowReleased?.Invoke();

            if (enableDebugLogs)
                Debug.Log("화살을 발사했습니다.");
        }

        isStringPulled = false;
        pullingHand = null;
        currentPullDistance = 0f;
    }

    /// <summary>
    /// 화살 발사
    /// </summary>
    void FireArrow()
    {
        Rigidbody arrowRb = nockedArrow.transform.GetComponent<Rigidbody>();
        if (arrowRb != null)
        {
            arrowRb.isKinematic = false;
            arrowRb.useGravity = true;

            // 발사 방향 계산 (시위 방향)
            Vector3 fireDirection = (stringEndPoint.position - stringStartPoint.position).normalized;

            // 발사 힘 계산 (당김 거리에 비례)
            float pullStrength = currentPullDistance / maxPullDistance;
            float fireForce = shootingForceMultiplier * pullStrength * stringTension;

            // 화살에 힘 적용
            arrowRb.AddForce(fireDirection * fireForce, ForceMode.Impulse);

            // 화살 회전 추가 (더 자연스러운 비행)
            arrowRb.AddTorque(arrowRb.transform.right * arrowSpinForce, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// 시위 초기 위치로 복원
    /// </summary>
    void ResetBowString()
    {
        if (bowStringRenderer != null)
        {
            bowStringRenderer.positionCount = 2;
            bowStringRenderer.SetPosition(0, stringStartPoint.position);
            bowStringRenderer.SetPosition(1, stringEndPoint.position);
        }
    }

    /// <summary>
    /// 현재 당김 강도 반환 (0-1)
    /// </summary>
    public float GetPullStrength()
    {
        return currentPullDistance / maxPullDistance;
    }

    /// <summary>
    /// 화살이 장전되었는지 확인
    /// </summary>
    public bool IsArrowNocked()
    {
        return isArrowNocked;
    }

    /// <summary>
    /// 시위가 당겨지고 있는지 확인
    /// </summary>
    public bool IsStringPulled()
    {
        return isStringPulled;
    }

    /// <summary>
    /// 현재 화살 개수 반환
    /// </summary>
    public int GetCurrentArrowCount()
    {
        return currentArrowCount;
    }

    void OnDestroy()
    {
        if (arrowSpawnCoroutine != null)
        {
            StopCoroutine(arrowSpawnCoroutine);
        }

        if (nockSocket != null)
        {
            nockSocket.selectEntered.RemoveListener(OnArrowNocked);
            nockSocket.selectExited.RemoveListener(OnArrowRemoved);
        }
    }
}