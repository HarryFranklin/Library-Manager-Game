using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    // The Singleton instance
    public static EconomyManager Instance { get; private set; }

    [Header("Economy Stats")]
    public int currentMoney = 0;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;

    private void Awake()
    {
        // Enforce the Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentMoney}";
        }
    }
}