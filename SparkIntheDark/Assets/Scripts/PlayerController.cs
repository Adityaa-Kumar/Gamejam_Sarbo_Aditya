using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [Tooltip("Number of lives")]
    public int lives = 3;

    [Header("Respawn Settings")]
    public Vector3 respawnPosition;

    [Tooltip("Invincibility duration after respawn (seconds)")]
    [Range(1f, 5f)]
    public float invincibilityDuration = 2f;

    private bool isInvincible = false;

    void Start()
    {
        respawnPosition = transform.position;
    }

    public void TakeDamage()
    {
        if (isInvincible) return;

        lives--;

        if (lives > 0)
        {
            Respawn();
        }
        else
        {
            GameOver();
        }
    }

    void Respawn()
    {
        transform.position = respawnPosition;
        StartCoroutine(InvincibilityCoroutine());

        Debug.Log($"Player respawned! Lives remaining: {lives}");
    }

    System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void GameOver()
    {
        Debug.Log("Game Over!");

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }
}
