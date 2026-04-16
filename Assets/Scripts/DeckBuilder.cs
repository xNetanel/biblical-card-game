using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class DeckBuilder : MonoBehaviour
{
    public static List<Card> builtDeck = new List<Card>();
    public static bool deckHasBeenBuilt = false;

    [Header("Card Collection")]
    public Transform cardGridParent;
    public GameObject cardPrefab;

    [Header("Deck Display")]
    public Transform deckListParent;
    public GameObject deckEntryPrefab;

    [Header("UI")]
    public TextMeshProUGUI deckCountText;
    public Button playButton;
    public Button clearButton;

    [Header("Rules Panel")]
    public GameObject rulesPanel;
    public Button rulesButton;

    private List<Card> allCards = new List<Card>();
    private List<Card> currentDeck = new List<Card>();

    void Start()
    {
        Debug.Log("DeckBuilder Start called.");
        allCards = new List<Card>(Resources.LoadAll<Card>("Cards"));
        Debug.Log($"Cards loaded: {allCards.Count}");
        allCards.Sort((a, b) => a.rarity.CompareTo(b.rarity));
        currentDeck = new List<Card>(builtDeck);
        DisplayAllCards();
        UpdateDeckUI();
    }

    void DisplayAllCards()
    {
        foreach (Transform child in cardGridParent)
            Destroy(child.gameObject);

        foreach (Card card in allCards)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardGridParent);

            CardUI cardUI = cardObj.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.PopulateCard(card);
                cardUI.enabled = false;
            }

            CardHover hover = cardObj.GetComponent<CardHover>();
            if (hover != null)
                hover.enabled = false;

            Button btn = cardObj.GetComponent<Button>();
            if (btn == null)
                btn = cardObj.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();

            Card captured = card;
            btn.onClick.AddListener(() =>
            {
                TryAddCard(captured);
                DisplayAllCards();
            });
        }
    }

    void TryAddCard(Card card)
    {
        Debug.Log($"TryAddCard called for {card.cardName}");
        int currentCount = CountInDeck(card);
        int maxAllowed = GetMaxCopies(card.rarity);

        if (currentCount >= maxAllowed)
        {
            Debug.Log($"Cannot add more copies of {card.cardName}.");
            return;
        }

        if (currentDeck.Count >= 30)
        {
            Debug.Log("Deck is full.");
            return;
        }

        currentDeck.Add(card);
        Debug.Log($"Added {card.cardName}. ({currentDeck.Count}/30)");
        UpdateDeckUI();
    }

    int CountInDeck(Card card)
    {
        int count = 0;
        foreach (Card c in currentDeck)
            if (c.cardName == card.cardName) count++;
        return count;
    }

    int GetMaxCopies(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Legendary: return 1;
            case Rarity.Epic: return 2;
            case Rarity.Rare: return 2;
            case Rarity.Common: return 3;
            default: return 1;
        }
    }

    void UpdateDeckUI()
    {
        foreach (Transform child in deckListParent)
            Destroy(child.gameObject);

        Dictionary<string, int> counts = new Dictionary<string, int>();
        Dictionary<string, Card> cardRef = new Dictionary<string, Card>();

        foreach (Card card in currentDeck)
        {
            if (counts.ContainsKey(card.cardName))
                counts[card.cardName]++;
            else
            {
                counts[card.cardName] = 1;
                cardRef[card.cardName] = card;
            }
        }

        foreach (var entry in counts)
        {
            GameObject entryObj = Instantiate(deckEntryPrefab, deckListParent);
            TextMeshProUGUI text = entryObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = $"{entry.Value}x {entry.Key}";

            Button removeBtn = entryObj.GetComponentInChildren<Button>();
            if (removeBtn != null)
            {
                Card captured = cardRef[entry.Key];
                removeBtn.onClick.AddListener(() => RemoveCard(captured));
            }
        }

        deckCountText.text = $"{currentDeck.Count}/30";
        playButton.interactable = currentDeck.Count == 30;
    }

    void RemoveCard(Card card)
    {
        for (int i = currentDeck.Count - 1; i >= 0; i--)
        {
            if (currentDeck[i].cardName == card.cardName)
            {
                currentDeck.RemoveAt(i);
                Debug.Log($"Removed {card.cardName} from deck.");
                UpdateDeckUI();
                return;
            }
        }
    }

    public void OnClearClicked()
    {
        currentDeck.Clear();
        UpdateDeckUI();
        Debug.Log("Deck cleared.");
    }

    public void OnPlayClicked()
    {
        if (currentDeck.Count != 30)
        {
            Debug.Log("Deck must have exactly 30 cards.");
            return;
        }

        builtDeck = new List<Card>(currentDeck);
        deckHasBeenBuilt = true;
        Debug.Log($"Deck saved with {builtDeck.Count} cards. Starting game.");
        SceneManager.LoadScene("SampleScene");
    }

    public void OnBackClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ToggleRulesPanel()
    {
        if (rulesPanel != null)
        {
            rulesPanel.SetActive(!rulesPanel.activeSelf);
        }
    }

    public void OnCloseRulesClicked()
    {
        if (rulesPanel != null)
            rulesPanel.SetActive(false);
    }
}