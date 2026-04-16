using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("References")]
    public GameObject cardPrefab;
    public Transform handArea;

    [Header("Hand Display")]
    public List<CardUI> handCardUIs = new List<CardUI>();

    [Header("Spacing")]
    public float cardSpacing = 110f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }


    public void RefreshHand()
    {
        foreach (CardUI cardUI in handCardUIs)
        {
            if (cardUI != null)
                Destroy(cardUI.gameObject);
        }
        handCardUIs.Clear();

        List<Card> hand = GameManager.Instance.playerHand;
        int count = hand.Count;

        for (int i = 0; i < count; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, handArea);
            CardUI cardUI = cardObj.GetComponent<CardUI>();
            cardUI.PopulateCard(hand[i]);
            cardUI.isPlayerCard = true;
            cardUI.UpdateManaCostUI();

            RectTransform rt = cardObj.GetComponent<RectTransform>();
            float maxWidth = 900f;
            float spacing = Mathf.Min(cardSpacing, maxWidth / Mathf.Max(count - 1, 1));
            float totalWidth = (count - 1) * spacing;
            float startX = -totalWidth / 2f;
            rt.anchoredPosition = new Vector2(startX + i * spacing, 0);

            Button btn = cardObj.GetComponent<Button>();
            if (btn == null)
                btn = cardObj.AddComponent<Button>();

            Card cardData = hand[i];
            btn.onClick.AddListener(() => OnHandCardClicked(cardData));

            handCardUIs.Add(cardUI);
        }
    }

    void OnHandCardClicked(Card card)
    {
        if (!GameManager.Instance.isPlayerTurn)
        {
            Debug.Log("Not your turn.");
            return;
        }

        if (!card.isUnit)
        {
            bool cast = GameManager.Instance.CastScriptureCard(card);
            if (cast)
            {
                Debug.Log($"Cast {card.cardName} from hand.");
                HandManager.Instance.RefreshHand();
            }
            return;
        }

        bool played = GameManager.Instance.PlayCard(card);
        if (played)
        {
            Debug.Log($"Played {card.cardName} from hand.");
            RefreshHand();
        }
    }

    public void ApplySpellCostIncreaseToHand(int amount)
    {
        foreach (CardUI cardUI in handCardUIs)
        {
            if (cardUI != null && !cardUI.cardData.isUnit)
            {
                cardUI.cardData.manaCost += amount;
                cardUI.UpdateManaCostUI();
                Debug.Log($"Tower of Babel: {cardUI.cardData.cardName} cost increased to {cardUI.cardData.manaCost}.");
            }
        }
    }

    public void ResetSpellCosts()
    {
        foreach (CardUI cardUI in handCardUIs)
        {
            if (cardUI != null && !cardUI.cardData.isUnit)
            {
                cardUI.cardData.manaCost = cardUI.baseManaCost;
                cardUI.UpdateManaCostUI();
                Debug.Log($"Spell cost reset for {cardUI.cardData.cardName}. Back to {cardUI.baseManaCost}.");
            }
        }
    }
}