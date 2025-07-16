using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// VR 디펜스 게임의 UI를 관리하는 스크립트
/// </summary>
public class DefenseGameUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("웨이브 정보 텍스트")]
    public TextMeshProUGUI waveText;
    
    [Tooltip("성 체력 바")]
    public Slider castleHealthBar;
    
    [Tooltip("성 체력 텍스트")]
    public TextMeshProUGUI castleHealthText;
    
    [Tooltip("점수 텍스트")]
    public TextMeshProUGUI scoreText;
    
    [Tooltip("처치한 적군 수 텍스트")]
    public TextMeshProUGUI enemiesKilledText;
    
    [Tooltip("게임 오버 패널")]
    public GameObject gameOverPanel;
    
    [Tooltip("게임 오버 텍스트")]
    public TextMeshProUGUI gameOverText;
    
    [Tooltip("재시작 버튼")]
    public Button restartButton;
    
    [Tooltip("메인 메뉴 버튼")]
    public Button mainMenuButton;
    
    [Header("Game References")]
    [Tooltip("성 체력 시스템")]
    public MonoBehaviour castleHealthComponent;
    
    [Tooltip("적군 스폰 시스템")]
    public MonoBehaviour enemySpawnerComponent;
    
    [Tooltip("점수 관리 시스템")]
    public MonoBehaviour scoreManagerComponent;
    
    // 내부 변수들
    private int currentScore = 0;
    private int enemiesKilled = 0;
    
    // 컴포넌트 참조
    private CastleHealth castleHealth;
    private EnemySpawner enemySpawner;
    private ScoreManager scoreManager;
    
    void Start()
    {
        InitializeComponents();
        InitializeUI();
        ConnectEvents();
    }
    
    /// <summary>
    /// 컴포넌트 초기화
    /// </summary>
    void InitializeComponents()
    {
        // CastleHealth 컴포넌트 찾기
        if (castleHealthComponent != null)
        {
            castleHealth = castleHealthComponent as CastleHealth;
        }
        else
        {
            castleHealth = FindObjectOfType<CastleHealth>();
        }
        
        // EnemySpawner 컴포넌트 찾기
        if (enemySpawnerComponent != null)
        {
            enemySpawner = enemySpawnerComponent as EnemySpawner;
        }
        else
        {
            enemySpawner = FindObjectOfType<EnemySpawner>();
        }
        
        // ScoreManager 컴포넌트 찾기
        if (scoreManagerComponent != null)
        {
            scoreManager = scoreManagerComponent as ScoreManager;
        }
        else
        {
            scoreManager = FindObjectOfType<ScoreManager>();
        }
        
        // 컴포넌트가 없으면 경고 메시지
        if (castleHealth == null)
        {
            Debug.LogWarning("CastleHealth 컴포넌트를 찾을 수 없습니다. 성 체력 UI가 작동하지 않을 수 있습니다.");
        }
        
        if (enemySpawner == null)
        {
            Debug.LogWarning("EnemySpawner 컴포넌트를 찾을 수 없습니다. 웨이브 UI가 작동하지 않을 수 있습니다.");
        }
        
        if (scoreManager == null)
        {
            Debug.LogWarning("ScoreManager 컴포넌트를 찾을 수 없습니다. 점수 UI가 작동하지 않을 수 있습니다.");
        }
    }
    
    /// <summary>
    /// UI 초기화
    /// </summary>
    void InitializeUI()
    {
        // 게임 오버 패널 숨기기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // 버튼 이벤트 연결
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
        
        // 초기 UI 업데이트
        UpdateWaveUI();
        UpdateScoreUI();
    }
    
    /// <summary>
    /// 이벤트 연결
    /// </summary>
    void ConnectEvents()
    {
        // 성 체력 이벤트 연결
        if (castleHealth != null)
        {
            castleHealth.OnHealthChanged += OnCastleHealthChanged;
            castleHealth.OnGameOver += OnGameOver;
        }
        
        // 적군 스폰 이벤트 연결
        if (enemySpawner != null)
        {
            enemySpawner.OnWaveStart += OnWaveStart;
            enemySpawner.OnWaveEnd += OnWaveEnd;
        }
        
        // 점수 관리 이벤트 연결
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged += OnScoreChanged;
        }
    }
    
    /// <summary>
    /// 성 체력 변경 시 호출
    /// </summary>
    void OnCastleHealthChanged(int currentHealth, int maxHealth)
    {
        UpdateCastleHealthUI(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// 게임 오버 시 호출
    /// </summary>
    void OnGameOver()
    {
        ShowGameOverUI();
    }
    
    /// <summary>
    /// 웨이브 시작 시 호출
    /// </summary>
    void OnWaveStart(int waveNumber)
    {
        UpdateWaveUI();
    }
    
    /// <summary>
    /// 웨이브 종료 시 호출
    /// </summary>
    void OnWaveEnd(int waveNumber)
    {
        UpdateWaveUI();
    }
    
    /// <summary>
    /// 점수 변경 시 호출
    /// </summary>
    void OnScoreChanged(int newScore)
    {
        currentScore = newScore;
        UpdateScoreUI();
    }
    
    /// <summary>
    /// 웨이브 UI 업데이트
    /// </summary>
    void UpdateWaveUI()
    {
        if (waveText != null && enemySpawner != null)
        {
            waveText.text = $"웨이브 {enemySpawner.CurrentWave}";
        }
        else if (waveText != null)
        {
            waveText.text = "웨이브 정보 없음";
        }
    }
    
    /// <summary>
    /// 성 체력 UI 업데이트
    /// </summary>
    void UpdateCastleHealthUI(int currentHealth, int maxHealth)
    {
        if (castleHealthBar != null)
        {
            castleHealthBar.value = (float)currentHealth / maxHealth;
        }
        
        if (castleHealthText != null)
        {
            castleHealthText.text = $"{currentHealth} / {maxHealth}";
        }
    }
    
    /// <summary>
    /// 점수 UI 업데이트
    /// </summary>
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"점수: {currentScore}";
        }
        
        if (enemiesKilledText != null)
        {
            if (enemySpawner != null)
            {
                enemiesKilledText.text = $"처치: {enemySpawner.EnemiesKilled}";
            }
            else
            {
                enemiesKilledText.text = $"처치: {enemiesKilled}";
            }
        }
    }
    
    /// <summary>
    /// 게임 오버 UI 표시
    /// </summary>
    void ShowGameOverUI()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        if (gameOverText != null)
        {
            gameOverText.text = $"게임 오버!\n최종 점수: {currentScore}\n처치한 적군: {enemiesKilled}";
        }
    }
    
    /// <summary>
    /// 게임 재시작
    /// </summary>
    void RestartGame()
    {
        // 씬 재로드
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
    
    /// <summary>
    /// 메인 메뉴로 이동
    /// </summary>
    void GoToMainMenu()
    {
        // 메인 메뉴 씬으로 이동 (씬 이름은 프로젝트에 맞게 수정)
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    /// <summary>
    /// 점수 추가
    /// </summary>
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }
    
    /// <summary>
    /// 처치한 적군 수 증가
    /// </summary>
    public void AddEnemyKill()
    {
        enemiesKilled++;
        UpdateScoreUI();
    }
    
    void OnDestroy()
    {
        // 이벤트 연결 해제
        if (castleHealth != null)
        {
            castleHealth.OnHealthChanged -= OnCastleHealthChanged;
            castleHealth.OnGameOver -= OnGameOver;
        }
        
        if (enemySpawner != null)
        {
            enemySpawner.OnWaveStart -= OnWaveStart;
            enemySpawner.OnWaveEnd -= OnWaveEnd;
        }
        
        if (scoreManager != null)
        {
            scoreManager.OnScoreChanged -= OnScoreChanged;
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(GoToMainMenu);
        }
    }
} 