using UnityEngine;
using TMPro;

public class AltarManager : MonoBehaviour
{
    public static AltarManager Instance;

    [Header("Altar Health")]
    public int playerAltarHealth = 30;
    public int opponentAltarHealth = 30;
    private const int MaxHealth = 30;

    [Header("UI References")]
    public TextMeshProUGUI playerAltarText;
    public TextMeshProUGUI opponentAltarText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void DamagePlayerAltar(int amount)
    {
        playerAltarHealth -= amount;
        playerAltarHealth = Mathf.Max(playerAltarHealth, 0);
        Debug.Log($"Player Altar took {amount} damage. Health: {playerAltarHealth}");
        UpdateUI();
        CheckWinCondition();
    }

    public void DamageOpponentAltar(int amount)
    {
        opponentAltarHealth -= amount;
        opponentAltarHealth = Mathf.Max(opponentAltarHealth, 0);
        Debug.Log($"Opponent Altar took {amount} damage. Health: {opponentAltarHealth}");
        UpdateUI();
        CheckWinCondition();
    }

    public void HealPlayerAltar(int amount)
    {
        playerAltarHealth += amount;
        playerAltarHealth = Mathf.Min(playerAltarHealth, MaxHealth);
        Debug.Log($"Player Altar healed {amount}. Health: {playerAltarHealth}");
        UpdateUI();
    }

    public void HealOpponentAltar(int amount)
    {
        opponentAltarHealth += amount;
        opponentAltarHealth = Mathf.Min(opponentAltarHealth, MaxHealth);
        Debug.Log($"Opponent Altar healed {amount}. Health: {opponentAltarHealth}");
        UpdateUI();
    }

    void CheckWinCondition()
    {
        if (playerAltarHealth <= 0)
        {
            Debug.Log("Opponent wins! Player Altar destroyed.");
            GameManager.Instance.EndGame(winner: false);
        }
        else if (opponentAltarHealth <= 0)
        {
            Debug.Log("Player wins! Opponent Altar destroyed.");
            GameManager.Instance.EndGame(winner: true);
        }
    }

    void UpdateUI()
    {
        if (playerAltarText != null)
            playerAltarText.text = $"Altar: {playerAltarHealth}";
        if (opponentAltarText != null)
            opponentAltarText.text = $"Altar: {opponentAltarHealth}";
    }
}