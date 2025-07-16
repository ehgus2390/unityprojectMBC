using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 적군을 스폰하고 웨이브를 관리하는 시스템
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("스폰할 적군 프리팹")]
    public GameObject enemyPrefab;

    [Tooltip("스폰 포인트들")]
    public Transform[] spawnPoints;

    [Tooltip("웨이브 간 대기 시간 (초)")]
    public float waveDelay = 5f;

    [Tooltip("적군 스폰 간격 (초)")]
    public float spawnInterval = 2f;

    [Header("Wave Settings")]
    [Tooltip("현재 웨이브 번호")]
    public int currentWave = 1;

    [Tooltip("웨이브당 적군 수")]
    public int enemiesPerWave = 5;

    [Tooltip("웨이브가 진행될수록 적군 수 증가량")]
    public int enemyIncreasePerWave = 2;

    [Tooltip("최대 웨이브 수 (0 = 무한)")]
    public int maxWaves = 0;

    [Header("Enemy Types")]
    [Tooltip("다양한 적군 프리팹들")]
    public GameObject[] enemyTypes;

    [Tooltip("각 적군 타입의 스폰 확률 (0~1)")]
    [Range(0f, 1f)]
    public float[] enemySpawnChances;

    [Header("Events")]
    [Tooltip("웨이브가 시작되었을 때 호출되는 이벤트")]
    public UnityEngine.Events.UnityEvent<int> OnWaveStart;

    [Tooltip("웨이브가 끝났을 때 호출되는 이벤트")]
    public UnityEngine.Events.UnityEvent<int> OnWaveEnd;

    [Tooltip("모든 웨이브가 끝났을 때 호출되는 이벤트")]
    public UnityEngine.Events.UnityEvent OnAllWavesComplete;

    // 내부 변수들
    private bool _isSpawning = false;
    private int _enemiesSpawned = 0;
    private int _enemiesKilled = 0;
    private List<GameObject> _activeEnemies = new List<GameObject>();

    // 프로퍼티
    public bool IsSpawning => _isSpawning;
    public int CurrentWave => currentWave;
    public int EnemiesSpawned => _enemiesSpawned;
    public int EnemiesKilled => _enemiesKilled;
    public int ActiveEnemies => _activeEnemies.Count;

    void Start()
    {
        // 적군 프리팹이 설정되지 않았다면 기본값 설정
        if (enemyPrefab == null && enemyTypes.Length > 0)
        {
            enemyPrefab = enemyTypes[0];
        }

        // 스폰 포인트가 설정되지 않았다면 자동으로 찾기
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            FindSpawnPoints();
        }

        // 첫 번째 웨이브 시작
        StartCoroutine(StartWaveCoroutine());
    }

    /// <summary>
    /// 스폰 포인트들을 자동으로 찾기
    /// </summary>
    void FindSpawnPoints()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPointObjects.Length > 0)
        {
            spawnPoints = new Transform[spawnPointObjects.Length];
            for (int i = 0; i < spawnPointObjects.Length; i++)
            {
                spawnPoints[i] = spawnPointObjects[i].transform;
            }
        }
        else
        {
            // 기본 스폰 포인트 생성
            CreateDefaultSpawnPoints();
        }
    }

    /// <summary>
    /// 기본 스폰 포인트 생성
    /// </summary>
    void CreateDefaultSpawnPoints()
    {
        spawnPoints = new Transform[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i}");
            spawnPoint.tag = "SpawnPoint";
            spawnPoint.transform.position = transform.position + Vector3.forward * (10 + i * 5) + Vector3.right * (i - 1) * 3;
            spawnPoints[i] = spawnPoint.transform;
        }
    }

    /// <summary>
    /// 웨이브 시작
    /// </summary>
    public void StartWave()
    {
        if (!_isSpawning)
        {
            StartCoroutine(StartWaveCoroutine());
        }
    }

    /// <summary>
    /// 웨이브 시작 코루틴
    /// </summary>
    IEnumerator StartWaveCoroutine()
    {
        _isSpawning = true;
        _enemiesSpawned = 0;
        _enemiesKilled = 0;

        // 웨이브 시작 이벤트 호출
        OnWaveStart?.Invoke(currentWave);

        Debug.Log($"웨이브 {currentWave} 시작!");

        // 현재 웨이브의 적군 수 계산
        int enemiesInThisWave = enemiesPerWave + (currentWave - 1) * enemyIncreasePerWave;

        // 적군 스폰
        for (int i = 0; i < enemiesInThisWave; i++)
        {
            SpawnEnemy();
            _enemiesSpawned++;

            // 마지막 적군이 아니면 대기
            if (i < enemiesInThisWave - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        // 모든 적군이 스폰된 후, 모든 적군이 죽거나 성에 도달할 때까지 대기
        yield return new WaitUntil(() => _activeEnemies.Count == 0);

        // 웨이브 종료
        _isSpawning = false;
        OnWaveEnd?.Invoke(currentWave);

        Debug.Log($"웨이브 {currentWave} 완료! 처치한 적군: {_enemiesKilled}");

        // 다음 웨이브 준비
        currentWave++;

        // 최대 웨이브 수 체크
        if (maxWaves > 0 && currentWave > maxWaves)
        {
            OnAllWavesComplete?.Invoke();
            Debug.Log("모든 웨이브 완료!");
        }
        else
        {
            // 다음 웨이브 시작
            yield return new WaitForSeconds(waveDelay);
            StartCoroutine(StartWaveCoroutine());
        }
    }

    /// <summary>
    /// 적군 스폰
    /// </summary>
    void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        // 스폰 포인트 선택
        Transform spawnPoint = GetRandomSpawnPoint();
        if (spawnPoint == null) return;

        // 적군 타입 선택
        GameObject enemyToSpawn = SelectEnemyType();

        // 적군 생성
        GameObject enemy = Instantiate(enemyToSpawn, spawnPoint.position, spawnPoint.rotation);

        // 적군 컨트롤러 설정
        EnemyController enemyController = enemy.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            // 이벤트 연결
            enemyController.OnEnemyDeath += OnEnemyDeath;
            enemyController.OnEnemyReachedCastle += OnEnemyReachedCastle;
        }

        // 활성 적군 목록에 추가
        _activeEnemies.Add(enemy);
    }

    /// <summary>
    /// 랜덤 스폰 포인트 선택
    /// </summary>
    Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    /// <summary>
    /// 적군 타입 선택
    /// </summary>
    GameObject SelectEnemyType()
    {
        // 다양한 적군 타입이 있다면 확률에 따라 선택
        if (enemyTypes.Length > 1 && enemySpawnChances.Length == enemyTypes.Length)
        {
            float random = Random.Range(0f, 1f);
            float cumulative = 0f;

            for (int i = 0; i < enemyTypes.Length; i++)
            {
                cumulative += enemySpawnChances[i];
                if (random <= cumulative)
                {
                    return enemyTypes[i];
                }
            }
        }

        // 기본값 반환
        return enemyPrefab;
    }

    /// <summary>
    /// 적군이 죽었을 때 호출
    /// </summary>
    void OnEnemyDeath(EnemyController enemy)
    {
        _enemiesKilled++;
        _activeEnemies.Remove(enemy.gameObject);
    }

    /// <summary>
    /// 적군이 성에 도달했을 때 호출
    /// </summary>
    void OnEnemyReachedCastle(EnemyController enemy)
    {
        _activeEnemies.Remove(enemy.gameObject);
    }

    /// <summary>
    /// 모든 적군 제거 (디버그용)
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in _activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        _activeEnemies.Clear();
    }

    /// <summary>
    /// 웨이브 설정 변경
    /// </summary>
    public void SetWaveSettings(int newEnemiesPerWave, float newSpawnInterval, float newWaveDelay)
    {
        enemiesPerWave = newEnemiesPerWave;
        spawnInterval = newSpawnInterval;
        waveDelay = newWaveDelay;
    }

    void OnDestroy()
    {
        // 이벤트 연결 해제
        foreach (GameObject enemy in _activeEnemies)
        {
            if (enemy != null)
            {
                EnemyController enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.OnEnemyDeath -= OnEnemyDeath;
                    enemyController.OnEnemyReachedCastle -= OnEnemyReachedCastle;
                }
            }
        }
    }
}