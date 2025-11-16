using UnityEngine;

public class PowerPellet : MonoBehaviour
{
    [Header("Power-Up Settings")]
    [Tooltip("Duration of power-up effect in seconds")]
    [Range(5f, 30f)]
    public float powerUpDuration = 10f;

    [Tooltip("Score value for collecting power pellet")]
    public int scoreValue = 50;

    [Header("Visual Effects")]
    public AudioClip pickupSound;
    public GameObject pickupEffect;

    [Tooltip("Scale pulsing for visual feedback")]
    public bool pulseEffect = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (pulseEffect)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * scale;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivatePowerUp(other.gameObject);
        }
    }

    void ActivatePowerUp(GameObject player)
    {
        // Activate power mode for player
        PowerUpManager powerUpManager = FindObjectOfType<PowerUpManager>();
        if (powerUpManager != null)
        {
            powerUpManager.ActivatePowerMode(powerUpDuration);
        }

        // Update score
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
        }

        // Audio and VFX
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
