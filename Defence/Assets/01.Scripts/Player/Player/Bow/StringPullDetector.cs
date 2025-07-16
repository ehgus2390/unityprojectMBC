using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 시위 당김을 감지하는 향상된 시스템
/// The Lab VR 스타일의 자연스러운 시위 당김 감지
/// </summary>
public class StringPullDetector : MonoBehaviour
{
    [Header("References")]
    [Tooltip("향상된 활 컨트롤러")]
    public EnhancedBowController bowController;

    [Tooltip("기존 활 컨트롤러 (호환성용)")]
    public BowController legacyBowController;

    [Header("Detection Settings")]
    [Tooltip("시위 당김 감지 거리")]
    public float detectionRadius = 0.15f;

    [Tooltip("시위 당김 해제 거리")]
    public float releaseDistance = 0.25f;

    [Tooltip("디버그 시각화 활성화")]
    public bool enableDebugVisualization = true;

    // 내부 변수들
    private bool isPulling = false;
    private Transform currentHand = null;
    private float currentDistance = 0f;
    private SphereCollider detectionCollider;

    void Start()
    {
        SetupDetectionArea();
    }

    void Update()
    {
        if (isPulling && currentHand != null)
        {
            UpdatePullDistance();
        }
    }

    /// <summary>
    /// 감지 영역 설정
    /// </summary>
    void SetupDetectionArea()
    {
        detectionCollider = GetComponent<SphereCollider>();
        if (detectionCollider == null)
        {
            detectionCollider = gameObject.AddComponent<SphereCollider>();
        }

        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRadius;

        // 디버그 시각화
        if (enableDebugVisualization)
        {
            CreateDebugVisualization();
        }
    }

    /// <summary>
    /// 디버그 시각화 생성
    /// </summary>
    void CreateDebugVisualization()
    {
        GameObject debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        debugSphere.name = "StringPullDebug";
        debugSphere.transform.SetParent(transform);
        debugSphere.transform.localPosition = Vector3.zero;
        debugSphere.transform.localScale = Vector3.one * detectionRadius * 2f;

        // 투명한 재질 적용
        Renderer renderer = debugSphere.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material debugMaterial = new Material(Shader.Find("Standard"));
            debugMaterial.color = new Color(1f, 0f, 0f, 0.3f);
            debugMaterial.SetFloat("_Mode", 3); // Transparent mode
            debugMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            debugMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            debugMaterial.SetInt("_ZWrite", 0);
            debugMaterial.DisableKeyword("_ALPHATEST_ON");
            debugMaterial.EnableKeyword("_ALPHABLEND_ON");
            debugMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            debugMaterial.renderQueue = 3000;
            renderer.material = debugMaterial;
        }

        // 콜라이더 제거 (트리거만 사용)
        DestroyImmediate(debugSphere.GetComponent<Collider>());
    }

    /// <summary>
    /// 당김 거리 업데이트
    /// </summary>
    void UpdatePullDistance()
    {
        if (currentHand == null) return;

        currentDistance = Vector3.Distance(transform.position, currentHand.position);

        // 시위 당김 해제 거리를 벗어나면 해제
        if (currentDistance > releaseDistance)
        {
            ReleasePull();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // XR 컨트롤러나 손 감지
        if (IsHandController(other) && !isPulling)
        {
            StartPull(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 손이 감지 영역을 벗어나면 시위 당김 해제
        if (IsHandController(other) && isPulling && other.transform == currentHand)
        {
            ReleasePull();
        }
    }

    /// <summary>
    /// 손 컨트롤러인지 확인
    /// </summary>
    bool IsHandController(Collider other)
    {
        // XR 컨트롤러 관련 컴포넌트 확인
        if (other.GetComponent<XRDirectInteractor>() != null ||
            other.GetComponent<XRRayInteractor>() != null ||
            other.GetComponent<XRGrabInteractable>() != null)
        {
            return true;
        }

        // 태그로 확인
        if (other.CompareTag("Player") ||
            other.CompareTag("Hand") ||
            other.CompareTag("Controller"))
        {
            return true;
        }

        // 이름으로 확인
        string objName = other.name.ToLower();
        if (objName.Contains("hand") ||
            objName.Contains("controller") ||
            objName.Contains("interactor"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 시위 당김 시작
    /// </summary>
    void StartPull(Transform hand)
    {
        if (isPulling) return;

        isPulling = true;
        currentHand = hand;
        currentDistance = 0f;

        // 향상된 활 컨트롤러 사용
        if (bowController != null)
        {
            bowController.StartStringPull(hand);
        }
        // 기존 활 컨트롤러 사용 (호환성)
        else if (legacyBowController != null)
        {
            // 기존 메서드가 있다면 호출
            var method = legacyBowController.GetType().GetMethod("StartPull");
            if (method != null)
            {
                method.Invoke(legacyBowController, new object[] { hand });
            }
        }

        Debug.Log("시위 당김 감지됨");
    }

    /// <summary>
    /// 시위 당김 해제
    /// </summary>
    void ReleasePull()
    {
        if (!isPulling) return;

        isPulling = false;
        currentHand = null;
        currentDistance = 0f;

        // 향상된 활 컨트롤러 사용
        if (bowController != null)
        {
            bowController.ReleaseStringPull();
        }
        // 기존 활 컨트롤러 사용 (호환성)
        else if (legacyBowController != null)
        {
            // 기존 메서드가 있다면 호출
            var method = legacyBowController.GetType().GetMethod("ReleasePull");
            if (method != null)
            {
                method.Invoke(legacyBowController, null);
            }
        }

        Debug.Log("시위 당김 해제됨");
    }

    /// <summary>
    /// 현재 당김 상태 반환
    /// </summary>
    public bool IsPulling()
    {
        return isPulling;
    }

    /// <summary>
    /// 현재 당김 거리 반환
    /// </summary>
    public float GetCurrentPullDistance()
    {
        return currentDistance;
    }

    void OnDrawGizmosSelected()
    {
        // 감지 영역 시각화
        Gizmos.color = isPulling ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // 해제 거리 시각화
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, releaseDistance);
    }
}