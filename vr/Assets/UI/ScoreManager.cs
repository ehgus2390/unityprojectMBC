using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 게임 점수를 관리하는 시스템
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    [Tooltip("현재 점수")]
    [SerializeField] private int currentScore = 0;
    
    [Tooltip("최고 점수")]
    [SerializeField] private int highScore = 0;
    
    [Header("Events")]
    [Tooltip("점수가 변경되었을 때 호출되는 이벤트")]
    public UnityEvent<int> OnScoreChanged;
    
    [Tooltip("최고 점수가 갱신되었을 때 호출되는 이벤트")]
    public UnityEvent<int> OnHighScoreChanged;
    
    // 프로퍼티
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    
    void Start()
    {
        // 저장된 최고 점수 불러오기
        LoadHighScore();
    }
    
    /// <summary>
    /// 점수 추가
    /// </summary>
    /// <param name="points">추가할 점수</param>
    public void AddScore(int points)
    {
        currentScore += points;
        
        // 최고 점수 체크
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
        }
        
        // 이벤트 호출
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// 점수 설정
    /// </summary>
    /// <param name="score">설정할 점수</param>
    public void SetScore(int score)
    {
        currentScore = score;
        
        // 최고 점수 체크
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
        }
        
        // 이벤트 호출
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// 점수 리셋
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// 최고 점수 저장
    /// </summary>
    void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// 최고 점수 불러오기
    /// </summary>
    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }
    
    /// <summary>
    /// 최고 점수 리셋
    /// </summary>
    public void ResetHighScore()
    {
        highScore = 0;
        PlayerPrefs.DeleteKey("HighScore");
        OnHighScoreChanged?.Invoke(highScore);
    }
} 