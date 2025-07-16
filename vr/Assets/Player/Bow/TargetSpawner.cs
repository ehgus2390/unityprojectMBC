using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 타겟 스포너 - 게임 중 타겟을 자동으로 생성하고 관리
/// </summary>
public class TargetSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("생성할 타겟 프리팹")]
    public GameObject targetPrefab;
    
    [Tooltip("최대 동시 타겟 수")]
    public int maxTargets = 5;
    
    [Tooltip("타겟 생성 간격 (초)")]
    public float spawnInterval = 3f;
    
    [Tooltip("타겟이 파괴된 후 새로운 타겟 생성까지의 지연 시간")]
    public float respawnDelay = 2f;
    
    [Header("Spawn Area")]
    [Tooltip("타겟 생성 영역의 중심")]
    public Transform spawnCenter;
    
    [Tooltip("타겟 생성 영역의 크기")]
    public Vector3 spawnAreaSize = new Vector3(10f, 5f, 20f);
    
    [Tooltip("타겟 생성 최소 거리")]
    public float minSpawnDistance = 5f;
    
    [Tooltip("타겟 생성 최대 거리")]
    public float maxSpawnDistance = 20f;
    
    [Header("Target Behavior")]
    [Tooltip("생성된 타겟이 움직이는지 여부")]
    public bool spawnMovingTargets = true;
    
    [Tooltip("생성된 타겟이 회전하는지 여부")]
    public bool spawnRotatingTargets = false;
    
    [Tooltip("타겟의 체력 범위")]
    public Vector2Int healthRange = new Vector2Int(50, 150);
    
    [Header("Difficulty")]
    [Tooltip("시간이 지날수록 생성 간격이 줄어드는지 여부")]
    public bool decreaseSpawnInterval = true;
    
    [Tooltip("최소 생성 간격")]
    public float minSpawnInterval = 1f;
    
    [Tooltip("난이도 증가 속도")]
    public float difficultyIncreaseRate = 0.1f;
    
    // 내부 변수들
    private List<GameObject> _activeTargets = new List<GameObject>();
    private float _currentSpawnInterval;
    private float _lastSpawnTime;
    private bool _isSpawning = false;
    private Coroutine _spawnCoroutine;
    
    // 이벤트
    public System.Action<GameObject> OnTargetSpawned;
    public System.Action<GameObject> OnTargetDestroyed;
    
    void Start()
    {
        InitializeSpawner();
    }
    
    /// <summary>
    /// 스포너 초기화
    /// </summary>
    void InitializeSpawner()
    {
        _currentSpawnInterval = spawnInterval;
        _lastSpawnTime = Time.time;
        
        if (spawnCenter == null)
        {
            spawnCenter = transform;
        }
        
        // 기존 타겟 정리
        CleanupExistingTargets();
        
        // 스포닝 시작
        StartSpawning();
    }
    
    /// <summary>
    /// 기존 타겟 정리
    /// </summary>
    void CleanupExistingTargets()
    {
        GameObject[] existingTargets = GameObject.FindGameObjectsWithTag("Target");
        foreach (GameObject target in existingTargets)
        {
            if (target != null)
            {
                DestroyImmediate(target);
            }
        }
        _activeTargets.Clear();
    }
    
    /// <summary>
    /// 스포닝 시작
    /// </summary>
    public void StartSpawning()
    {
        if (!_isSpawning)
        {
            _isSpawning = true;
            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }
    
    /// <summary>
    /// 스포닝 중지
    /// </summary>
    public void StopSpawning()
    {
        _isSpawning = false;
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
    }
    
    /// <summary>
    /// 스포닝 루틴
    /// </summary>
    IEnumerator SpawnRoutine()
    {
        while (_isSpawning)
        {
            // 타겟 수가 최대치보다 적으면 생성
            if (_activeTargets.Count < maxTargets)
            {
                SpawnTarget();
            }
            
            // 난이도 증가
            if (decreaseSpawnInterval)
            {
                _currentSpawnInterval = Mathf.Max(minSpawnInterval, 
                    _currentSpawnInterval - difficultyIncreaseRate * Time.deltaTime);
            }
            
            yield return new WaitForSeconds(_currentSpawnInterval);
        }
    }
    
    /// <summary>
    /// 타겟 생성
    /// </summary>
    void SpawnTarget()
    {
        if (targetPrefab == null) return;
        
        // 스폰 위치 계산
        Vector3 spawnPosition = CalculateSpawnPosition();
        
        // 타겟 생성
        GameObject newTarget = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
        
        // 타겟 설정
        SetupTarget(newTarget);
        
        // 활성 타겟 목록에 추가
        _activeTargets.Add(newTarget);
        
        // 이벤트 호출
        OnTargetSpawned?.Invoke(newTarget);
        
        _lastSpawnTime = Time.time;
    }
    
    /// <summary>
    /// 스폰 위치 계산
    /// </summary>
    Vector3 CalculateSpawnPosition()
    {
        Vector3 center = spawnCenter.position;
        
        // 랜덤 방향과 거리
        Vector3 randomDirection = Random.onUnitSphere;
        randomDirection.y = Mathf.Abs(randomDirection.y); // 위쪽으로만 생성
        
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        Vector3 spawnPosition = center + randomDirection * randomDistance;
        
        // 스폰 영역 내로 제한
        spawnPosition.x = Mathf.Clamp(spawnPosition.x, center.x - spawnAreaSize.x * 0.5f, center.x + spawnAreaSize.x * 0.5f);
        spawnPosition.y = Mathf.Clamp(spawnPosition.y, center.y, center.y + spawnAreaSize.y);
        spawnPosition.z = Mathf.Clamp(spawnPosition.z, center.z - spawnAreaSize.z * 0.5f, center.z + spawnAreaSize.z * 0.5f);
        
        return spawnPosition;
    }
    
    /// <summary>
    /// 타겟 설정
    /// </summary>
    void SetupTarget(GameObject target)
    {
        TargetController targetController = target.GetComponent<TargetController>();
        if (targetController != null)
        {
            // 랜덤 체력 설정
            targetController.maxHealth = Random.Range(healthRange.x, healthRange.y + 1);
            
            // 움직임 설정
            targetController.isMoving = spawnMovingTargets && Random.value > 0.5f;
            targetController.isRotating = spawnRotatingTargets && Random.value > 0.7f;
            
            // 파괴 이벤트 연결
            targetController.OnTargetDestroyed += () => OnTargetDestroyedHandler(target);
        }
    }
    
    /// <summary>
    /// 타겟 파괴 핸들러
    /// </summary>
    void OnTargetDestroyedHandler(GameObject target)
    {
        // 활성 타겟 목록에서 제거
        _activeTargets.Remove(target);
        
        // 이벤트 호출
        OnTargetDestroyed?.Invoke(target);
        
        // 지연 후 타겟 제거
        StartCoroutine(RemoveTargetAfterDelay(target, respawnDelay));
    }
    
    /// <summary>
    /// 지연 후 타겟 제거
    /// </summary>
    IEnumerator RemoveTargetAfterDelay(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (target != null)
        {
            Destroy(target);
        }
    }
    
    /// <summary>
    /// 모든 타겟 제거
    /// </summary>
    public void ClearAllTargets()
    {
        foreach (GameObject target in _activeTargets)
        {
            if (target != null)
            {
                Destroy(target);
            }
        }
        _activeTargets.Clear();
    }
    
    /// <summary>
    /// 현재 활성 타겟 수 반환
    /// </summary>
    public int GetActiveTargetCount()
    {
        return _activeTargets.Count;
    }
    
    /// <summary>
    /// 스포너 리셋
    /// </summary>
    public void ResetSpawner()
    {
        StopSpawning();
        ClearAllTargets();
        _currentSpawnInterval = spawnInterval;
        StartSpawning();
    }
    
    void OnDrawGizmosSelected()
    {
        // 스폰 영역 시각화
        if (spawnCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(spawnCenter.position, spawnAreaSize);
            
            // 최소/최대 거리 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnCenter.position, minSpawnDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnCenter.position, maxSpawnDistance);
        }
    }
} 