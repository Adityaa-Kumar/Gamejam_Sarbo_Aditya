using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshProUGUI component to display coin count")]
    public TextMeshProUGUI coinText;

    [Header("Coin Settings")]
    [Tooltip("Starting number of coins")]
    public int startingCoins = 0;

    private int currentCoins;

    // Singleton instance
    public static CoinManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        currentCoins = startingCoins;
        UpdateCoinUI();
    }

    public void AddCoins(int amount)
    {
        currentCoins += amount;
        UpdateCoinUI();
    }

    public void RemoveCoins(int amount)
    {
        currentCoins -= amount;
        if (currentCoins < 0) currentCoins = 0;
        UpdateCoinUI();
    }

    public int GetCoinCount()
    {
        return currentCoins;
    }

    void UpdateCoinUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + currentCoins.ToString();
        }
        else
        {
            Debug.LogWarning("Coin Text UI not assigned in CoinManager!");
        }
    }
}
