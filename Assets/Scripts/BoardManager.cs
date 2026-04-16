using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("Board Slots")]
    public List<Transform> playerSlots = new List<Transform>();
    public List<Transform> opponentSlots = new List<Transform>();

    [Header("Active Units")]
    public List<CardUI> playerUnits = new List<CardUI>();
    public List<CardUI> opponentUnits = new List<CardUI>();

    [Header("Card Prefab")]
    public GameObject cardPrefab;

    [Header("Selection State")]
    private CardUI selectedCard = null;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool PlayCardToBoard(Card card, bool isPlayer)
    {
        List<Transform> slots = isPlayer ? playerSlots : opponentSlots;
        List<CardUI> units = isPlayer ? playerUnits : opponentUnits;

        Transform freeSlot = GetFreeSlot(slots, units);
        if (freeSlot == null)
        {
            Debug.Log("No free slots on the board.");
            return false;
        }

        Card cardCopy = card.Clone();

        GameObject cardObj = Instantiate(cardPrefab, freeSlot.position, Quaternion.identity, freeSlot);
        CardUI cardUI = cardObj.GetComponent<CardUI>();
        cardUI.isPlayerCard = isPlayer;
        cardUI.PopulateCard(cardCopy);

        Button btn = cardObj.GetComponent<Button>();
        if (btn == null)
            btn = cardObj.AddComponent<Button>();
        btn.onClick.AddListener(() => cardUI.OnCardClicked());

        units.Add(cardUI);
        AudioManager.Instance.PlayCardPlayed();
        Debug.Log($"{card.cardName} played to board. {(isPlayer ? "Player" : "Opponent")} now has {units.Count} unit(s).");

        AbilityManager.Instance.TriggerOnUnitSummoned(cardUI, isPlayer);
        Debug.Log("Triggering Battlecry");
        AbilityManager.Instance.TriggerBattlecry(cardUI, isPlayer);
        Debug.Log("Battlecry triggered");
        return true;
    }

    Transform GetFreeSlot(List<Transform> slots, List<CardUI> units)
    {
        foreach (Transform slot in slots)
        {
            bool occupied = false;
            foreach (CardUI unit in units)
            {
                if (unit.transform.parent == slot)
                {
                    occupied = true;
                    break;
                }
            }
            if (!occupied) return slot;
        }
        return null;
    }


    public void SelectCard(CardUI card)
    {
        if (card.isPlayerCard)
        {
            if (selectedCard != null)
                selectedCard.SetSelected(false);
            selectedCard = card;
            card.SetSelected(true);
            Debug.Log($"Selected: {card.cardData.cardName}");
        }
        else
        {
            if (selectedCard == null)
            {
                Debug.Log("Select a friendly card first.");
                return;
            }
            ResolveCombat(selectedCard, card);
            selectedCard.SetSelected(false);
            selectedCard = null;
        }
    }

    public void ResolveCombat(CardUI attacker, CardUI defender)
    {
        Debug.Log($"{attacker.cardData.cardName} attacks {defender.cardData.cardName}");
        StartCoroutine(ResolveCombatWithAnimation(attacker, defender));
    }

    IEnumerator ResolveCombatWithAnimation(CardUI attacker, CardUI defender)
    {
        yield return StartCoroutine(attacker.AttackAnimation(defender.transform.position));

        int attackerDamage = attacker.currentAttack;
        int defenderDamage = defender.currentAttack;

        attacker.TakeDamage(defenderDamage);
        defender.TakeDamage(attackerDamage);

        AudioManager.Instance.PlayCombatHit();

        if (attacker != null && !attacker.Equals(null) && attacker.currentHealth > 0)
        {
            attacker.combatSurviveCount++;
            CheckJacobTransform(attacker);
            AbilityManager.Instance.TriggerOnCombatSurvive(attacker, attacker.isPlayerCard);
        }

        if (defender != null && !defender.Equals(null) && defender.currentHealth > 0)
        {
            defender.combatSurviveCount++;
            CheckJacobTransform(defender);
            AbilityManager.Instance.TriggerOnCombatSurvive(defender, defender.isPlayerCard);
        }

        attacker.hasAttackedThisTurn = true;
    }

    public void AttackAltar(CardUI attacker)
    {
        if (attacker.hasAttackedThisTurn) return;

        if (BoardManager.Instance.opponentUnits.Count > 0)
        {
            Debug.Log("Cannot attack Altar while opponent has units on board.");
            return;
        }

        StartCoroutine(AttackAltarWithAnimation(attacker));
    }

    IEnumerator AttackAltarWithAnimation(CardUI attacker)
    {
        Vector3 altarDirection = new Vector3(0, 1, 0);
        yield return StartCoroutine(attacker.AttackAnimation(attacker.transform.position + altarDirection * 50f));

        Debug.Log($"{attacker.cardData.cardName} attacks opponent Altar for {attacker.currentAttack}");
        AltarManager.Instance.DamageOpponentAltar(attacker.currentAttack);
        AudioManager.Instance.PlayCombatHit();
        attacker.hasAttackedThisTurn = true;

        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
            selectedCard = null;
        }
    }

    public void RemoveUnitFromBoard(CardUI unit)
    {
        if (playerUnits.Contains(unit))
            playerUnits.Remove(unit);
        else if (opponentUnits.Contains(unit))
            opponentUnits.Remove(unit);
    }

    public void RefreshAllUnitsForNewTurn(bool isPlayer)
    {
        List<CardUI> units = isPlayer ? playerUnits : opponentUnits;
        foreach (CardUI unit in units)
            unit.RefreshForNewTurn();
    }

    public int GetPlayerUnitCount() => playerUnits.Count;
    public int GetOpponentUnitCount() => opponentUnits.Count;

    public CardUI GetSelectedCard()
    {
        return selectedCard;
    }

    void CheckJacobTransform(CardUI unit)
    {
        if (unit.cardData.cardName == "JACOB" && unit.combatSurviveCount >= 2 && !unit.hasTransformed)
        {
            unit.hasTransformed = true;

            Card israelCard = CardDatabase.Instance.GetToken("ISRAEL");
            if (israelCard == null)
            {
                Debug.Log("Jacob: Israel card not found.");
                return;
            }

            unit.currentAttack += 2;
            unit.currentHealth += 2;
            unit.cardData = israelCard;
            unit.UpdateStatsUI();

            if (unit.cardNameText != null)
                unit.cardNameText.text = "ISRAEL";
            if (unit.cardArtImage != null && israelCard.cardArt != null)
                unit.cardArtImage.sprite = israelCard.cardArt;
            if (unit.abilityText != null)
                unit.abilityText.text = israelCard.abilityDescription;
            if (unit.cardRarityText != null)
                unit.cardRarityText.text = israelCard.rarity.ToString();

            Debug.Log($"Jacob transformed into Israel! Now {unit.currentAttack}/{unit.currentHealth}.");
        }
    }
}