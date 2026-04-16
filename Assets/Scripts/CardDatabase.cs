using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance;

    [Header("All Cards")]
    public List<Card> allCards = new List<Card>();

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

        LoadCards();
    }

    void LoadCards()
    {
        Card[] loaded = Resources.LoadAll<Card>("Cards");
        allCards.Clear();
        foreach (Card card in loaded)
        {
            allCards.Add(card);
        }
        Debug.Log($"CardDatabase loaded {allCards.Count} cards.");
    }

    public Card GetCardByName(string name)
    {
        return allCards.Find(c => c.cardName == name);
    }

    public Card GetToken(string name)
    {
        Card[] tokens = Resources.LoadAll<Card>("Tokens");
        foreach (Card token in tokens)
        {
            if (token.cardName == name)
                return token;
        }
        Debug.Log($"Token {name} not found.");
        return null;
    }
}