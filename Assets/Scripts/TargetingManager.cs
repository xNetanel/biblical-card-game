using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TargetingManager : MonoBehaviour
{
    public static TargetingManager Instance;

    [Header("UI")]
    public GameObject targetingPanel;
    public TextMeshProUGUI targetingText;

    [Header("State")]
    public bool isTargeting = false;
    private Action<CardUI> onTargetSelected;
    private bool requireEnemy;
    private bool requireFriendly;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RequestTarget(bool enemy, bool friendly, Action<CardUI> callback)
    {
        isTargeting = true;
        requireEnemy = enemy;
        requireFriendly = friendly;
        onTargetSelected = callback;

        if (targetingPanel != null)
            targetingPanel.SetActive(true);
        if (targetingText != null)
            targetingText.text = enemy ? "Select an Enemy Target" : "Select a Friendly Target";

        Debug.Log("Targeting mode: Select a target.");
    }

    public void SelectTarget(CardUI target, bool targetIsPlayerCard)
    {
        if (!isTargeting) return;

        if (requireEnemy && targetIsPlayerCard) return;
        if (requireFriendly && !targetIsPlayerCard) return;

        isTargeting = false;
        if (targetingPanel != null)
            targetingPanel.SetActive(false);

        onTargetSelected?.Invoke(target);
        onTargetSelected = null;
        Debug.Log($"Target selected: {target.cardData.cardName}");
    }

    public void CancelTargeting()
    {
        isTargeting = false;
        if (targetingPanel != null)
            targetingPanel.SetActive(false);
        onTargetSelected = null;
        Debug.Log("Targeting cancelled.");
    }
}