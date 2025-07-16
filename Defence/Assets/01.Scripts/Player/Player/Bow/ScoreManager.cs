using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임의 점수 시스템을 관리하는 스크립트
/// 현재 점수, 하이스코어, UI 업데이트, 저장/로드 기능을 제공합니다.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("현재 점수를 표시할 UI Text 컴포넌트")]
    public Text scoreText;

    [Tooltip("하이스코어를 표시할 UI Text 컴포넌트")]
    public Text highScoreText;

    [Header("Score Settings")]
    [Tooltip("현재 게임에서 획득한 점수")]
    public int currentScore = 0;

    [Tooltip("저장된 최고 점수")]
    public int highScore = 0;

    /// <summary>PlayerPrefs에 저장할 하이스코어의 키 이름</summary>
    private const string HIGH_SCORE_KEY = "HighScore";

    /// <summary>
    /// 스크립트 초기화 시 호출되는 함수
    /// 저장된 하이스코어를 로드하고 UI를 업데이트합니다.
    /// </summary>
    void Start()
    {
        LoadHighScore();
        UpdateScoreDisplay();
    }

    /// <summary>
    /// 점수를 추가하는 함수
    /// 현재 점수에 지정된 점수를 더하고 하이스코어를 체크합니다.
    /// </summary>
    /// <param name="points">추가할 점수</param>
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreDisplay();

        // 하이스코어 체크 및 업데이트
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            UpdateHighScoreDisplay();
        }
    }

    /// <summary>
    /// 현재 점수를 0으로 리셋하는 함수
    /// 새로운 게임 시작 시 사용됩니다.
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreDisplay();
    }

    /// <summary>
    /// 현재 점수 UI를 업데이트하는 함수
    /// scoreText가 설정되어 있을 때만 작동합니다.
    /// </summary>
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    /// <summary>
    /// 하이스코어 UI를 업데이트하는 함수
    /// highScoreText가 설정되어 있을 때만 작동합니다.
    /// </summary>
    void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore.ToString();
        }
    }

    /// <summary>
    /// 하이스코어를 PlayerPrefs에 저장하는 함수
    /// 게임 종료 시나 하이스코어 갱신 시 자동으로 호출됩니다.
    /// </summary>
    void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// PlayerPrefs에서 하이스코어를 로드하는 함수
    /// 게임 시작 시 자동으로 호출됩니다.
    /// </summary>
    void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        UpdateHighScoreDisplay();
    }

    /// <summary>
    /// 게임 종료 시 호출되는 함수
    /// 현재 하이스코어를 저장합니다.
    /// </summary>
    public void GameOver()
    {
        SaveHighScore();
    }

    /// <summary>
    /// 현재 점수를 반환하는 함수
    /// 다른 스크립트에서 점수 정보가 필요할 때 사용됩니다.
    /// </summary>
    /// <returns>현재 점수</returns>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// 하이스코어를 반환하는 함수
    /// 다른 스크립트에서 하이스코어 정보가 필요할 때 사용됩니다.
    /// </summary>
    /// <returns>하이스코어</returns>
    public int GetHighScore()
    {
        return highScore;
    }

    /// <summary>
    /// 모든 점수 데이터를 초기화하는 함수
    /// 하이스코어까지 모두 0으로 리셋합니다.
    /// </summary>
    public void ResetAllScores()
    {
        currentScore = 0;
        highScore = 0;
        PlayerPrefs.DeleteKey(HIGH_SCORE_KEY);
        UpdateScoreDisplay();
        UpdateHighScoreDisplay();
    }
}