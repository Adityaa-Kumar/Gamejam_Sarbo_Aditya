using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;

    [Header("Score Settings")]
    public int ghostEatenScore = 200;

    private int currentScore = 0;
    private int ghostsEatenCombo = 0;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }

    public void OnGhostEaten()
    {
        ghostsEatenCombo++;

        // Pac-Man style: each ghost eaten in power mode is worth more
        int ghostScore = ghostEatenScore * ghostsEatenCombo;
        AddScore(ghostScore);

        Debug.Log($"Ghost eaten! Combo: {ghostsEatenCombo}x Score: +{ghostScore}");
    }

    public void ResetGhostCombo()
    {
        ghostsEatenCombo = 0;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + currentScore.ToString();
        }
    }

    public void UpdateLivesUI(int lives)
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + lives.ToString();
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over! Final Score: " + currentScore);
        // Add game over logic here
    }
}
