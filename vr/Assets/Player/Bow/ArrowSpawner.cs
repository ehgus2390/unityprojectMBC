using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

/// <summary>
/// 오른손에 자동으로 화살을 생성하는 시스템
/// VR 컨트롤러에서 화살을 자동으로 스폰하고 관리합니다.
/// </summary>
public class ArrowSpawner : MonoBehaviour
{
    [Header("Arrow Settings")]
    [Tooltip("생성할 화살 프리팹")]
    public GameObject arrowPrefab;
    
    [Tooltip("화살 생성 위치 (오른손)")]
    public Transform rightHandSpawnPoint;
    
    [Tooltip("화살 생성 간격 (초)")]
    public float spawnInterval = 2f;
    
    [Tooltip("최대 화살 개수")]
    public int maxArrows = 5;
    
    [Header("XR Settings")]
    [Tooltip("오른손 컨트롤러")]
    public XRDirectInteractor rightHandInteractor;
    
    [Tooltip("화살이 자동으로 생성되는지 여부")]
    public bool autoSpawn = true;
    
    [Header("Debug")]
    [Tooltip("디버그 로그 활성화")]
    public bool enableDebugLogs = true;
    
    // 내부 변수들
    private int currentArrowCount = 0;
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    
    void Start()
    {
        InitializeArrowSpawner();
    }
    
    /// <summary>
    /// 화살 스포너 초기화
    /// </summary>
    void InitializeArrowSpawner()
    {
        // 오른손 컨트롤러가 설정되지 않았다면 자동으로 찾기
        if (rightHandInteractor == null)
        {
            // 모든 XR Direct Interactor를 찾아서 오른손 컨트롤러 찾기
            XRDirectInteractor[] interactors = FindObjectsOfType<XRDirectInteractor>();
            foreach (var interactor in interactors)
            {
                // 오른손 컨트롤러는 보통 "Right" 또는 "R"이 포함된 이름을 가짐
                if (interactor.name.ToLower().Contains("right") || 
                    interactor.name.ToLower().Contains("r_") ||
                    interactor.name.ToLower().Contains("r "))
                {
                    rightHandInteractor = interactor;
                    break;
                }
            }
            
            // 여전히 찾지 못했다면 첫 번째 것을 사용
            if (rightHandInteractor == null && interactors.Length > 0)
            {
                rightHandInteractor = interactors[0];
                Debug.LogWarning("오른손 컨트롤러를 정확히 찾지 못했습니다. 첫 번째 컨트롤러를 사용합니다.");
            }
            
            if (rightHandInteractor == null)
            {
                Debug.LogError("오른손 컨트롤러를 찾을 수 없습니다!");
                return;
            }
        }
        
        // 스폰 포인트가 설정되지 않았다면 오른손 컨트롤러의 위치로 설정
        if (rightHandSpawnPoint == null)
        {
            rightHandSpawnPoint = rightHandInteractor.transform;
        }
        
        // 화살 프리팹이 설정되지 않았다면 경고
        if (arrowPrefab == null)
        {
            Debug.LogWarning("화살 프리팹이 설정되지 않았습니다!");
            return;
        }
        
        // 자동 스폰이 활성화되어 있다면 스폰 시작
        if (autoSpawn)
        {
            StartAutoSpawn();
        }
        
        if (enableDebugLogs)
            Debug.Log("화살 스포너가 초기화되었습니다.");
    }
    
    /// <summary>
    /// 자동 화살 스폰 시작
    /// </summary>
    public void StartAutoSpawn()
    {
        if (!isSpawning && autoSpawn)
        {
            isSpawning = true;
            spawnCoroutine = StartCoroutine(AutoSpawnCoroutine());
            
            if (enableDebugLogs)
                Debug.Log("자동 화살 스폰이 시작되었습니다.");
        }
    }
    
    /// <summary>
    /// 자동 화살 스폰 중지
    /// </summary>
    public void StopAutoSpawn()
    {
        if (isSpawning)
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            
            if (enableDebugLogs)
                Debug.Log("자동 화살 스폰이 중지되었습니다.");
        }
    }
    
    /// <summary>
    /// 자동 화살 스폰 코루틴
    /// </summary>
    IEnumerator AutoSpawnCoroutine()
    {
        while (isSpawning)
        {
            // 최대 화살 개수에 도달하지 않았다면 화살 생성
            if (currentArrowCount < maxArrows)
            {
                SpawnArrow();
            }
            
            // 지정된 간격만큼 대기
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
    /// <summary>
    /// 화살 수동 생성
    /// </summary>
    [ContextMenu("Spawn Arrow")]
    public void SpawnArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogError("화살 프리팹이 설정되지 않았습니다!");
            return;
        }
        
        if (rightHandSpawnPoint == null)
        {
            Debug.LogError("화살 생성 위치가 설정되지 않았습니다!");
            return;
        }
        
        if (currentArrowCount >= maxArrows)
        {
            if (enableDebugLogs)
                Debug.Log("최대 화살 개수에 도달했습니다.");
            return;
        }
        
        // 화살 생성 위치 계산 (오른손 앞쪽에 약간 떨어진 위치)
        Vector3 spawnPosition = rightHandSpawnPoint.position + rightHandSpawnPoint.forward * 0.1f;
        Quaternion spawnRotation = rightHandSpawnPoint.rotation;
        
        // 화살 생성
        GameObject newArrow = Instantiate(arrowPrefab, spawnPosition, spawnRotation);
        
        // 화살에 필요한 컴포넌트들 확인 및 추가
        SetupArrowComponents(newArrow);
        
        // 화살이 생성된 직후에는 물리 효과를 비활성화하여 자연스럽게 잡을 수 있도록 함
        Rigidbody arrowRb = newArrow.GetComponent<Rigidbody>();
        if (arrowRb != null)
        {
            arrowRb.isKinematic = true;
            arrowRb.useGravity = false;
            
            // 0.5초 후 물리 효과 활성화
            StartCoroutine(EnablePhysicsAfterDelay(arrowRb, 0.5f));
        }
        
        // 화살 개수 증가
        currentArrowCount++;
        
        if (enableDebugLogs)
            Debug.Log($"화살이 생성되었습니다. 현재 화살 개수: {currentArrowCount}");
    }
    
    /// <summary>
    /// 화살 컴포넌트 설정
    /// </summary>
    void SetupArrowComponents(GameObject arrow)
    {
        // XR Grab Interactable 컴포넌트 확인
        XRGrabInteractable grabInteractable = arrow.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null)
        {
            grabInteractable = arrow.AddComponent<XRGrabInteractable>();
        }
        
        // Rigidbody 컴포넌트 확인
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = arrow.AddComponent<Rigidbody>();
        }
        
        // ArrowController 컴포넌트 확인
        ArrowController arrowController = arrow.GetComponent<ArrowController>();
        if (arrowController == null)
        {
            arrowController = arrow.AddComponent<ArrowController>();
        }
        
        // ArrowInteractable 컴포넌트 확인
        ArrowInteractable arrowInteractable = arrow.GetComponent<ArrowInteractable>();
        if (arrowInteractable == null)
        {
            arrowInteractable = arrow.AddComponent<ArrowInteractable>();
        }
        
        // 화살이 파괴될 때 카운트 감소
        arrowInteractable.OnArrowDestroyed += OnArrowDestroyed;
    }
    
    /// <summary>
    /// 화살이 파괴되었을 때 호출
    /// </summary>
    void OnArrowDestroyed()
    {
        currentArrowCount = Mathf.Max(0, currentArrowCount - 1);
        
        if (enableDebugLogs)
            Debug.Log($"화살이 파괴되었습니다. 현재 화살 개수: {currentArrowCount}");
    }
    
    /// <summary>
    /// 모든 화살 제거
    /// </summary>
    [ContextMenu("Clear All Arrows")]
    public void ClearAllArrows()
    {
        ArrowInteractable[] arrows = FindObjectsOfType<ArrowInteractable>();
        foreach (var arrow in arrows)
        {
            if (arrow != null)
            {
                Destroy(arrow.gameObject);
            }
        }
        
        currentArrowCount = 0;
        
        if (enableDebugLogs)
            Debug.Log("모든 화살이 제거되었습니다.");
    }
    
    /// <summary>
    /// 현재 화살 개수 반환
    /// </summary>
    public int GetCurrentArrowCount()
    {
        return currentArrowCount;
    }
    
    /// <summary>
    /// 최대 화살 개수 반환
    /// </summary>
    public int GetMaxArrowCount()
    {
        return maxArrows;
    }
    
    /// <summary>
    /// 지연 후 물리 효과 활성화
    /// </summary>
    IEnumerator EnablePhysicsAfterDelay(Rigidbody rb, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
    
    void OnDestroy()
    {
        // 코루틴 정리
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
} 