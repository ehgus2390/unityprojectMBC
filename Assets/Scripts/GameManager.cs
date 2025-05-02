using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}

    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] GameObject gameOverUI;

    public bool IsGameOver {  get; private set; }= false;
    private int score = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //씬 전환을 해도 파괴되지 않도록 하려면
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (IsGameOver && Input.GetMouseButtonDown(0))
        {
            RestartGame();
        }
    }

    public void AddScore(int newScore)
    {
        score += newScore;
        UpdateScoreUI();
    }
    private void UpdateScoreUI()
    {
        if (scoreText != null) 
        {
            scoreText.text = $"Score : {score}";
        }
    }
    public void OnPlayerDead()
    {
        IsGameOver = true;
        if(gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
