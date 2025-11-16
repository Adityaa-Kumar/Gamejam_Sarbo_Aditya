using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Coin Settings")]
    [Tooltip("Value of this coin")]
    [Range(1, 100)]
    public int coinValue = 1;

    [Header("Visual Effects")]
    [Tooltip("Play sound on pickup")]
    public AudioClip pickupSound;

    [Tooltip("Spawn particle effect on pickup")]
    public GameObject pickupEffect;

    [Tooltip("Rotate coin continuously")]
    public bool autoRotate = true;

    [Range(0f, 500f)]
    public float rotationSpeed = 100f;

    void Update()
    {
        if (autoRotate)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if player collected the coin
        if (other.CompareTag("Player"))
        {
            CollectCoin(other.gameObject);
        }
    }

    void CollectCoin(GameObject player)
    {
        // Update coin manager
        CoinManager coinManager = FindObjectOfType<CoinManager>();
        if (coinManager != null)
        {
            coinManager.AddCoins(coinValue);
        }
        else
        {
            Debug.LogWarning("CoinManager not found in scene!");
        }

        // Play pickup sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Spawn particle effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // Destroy coin
        Destroy(gameObject);
    }
}
