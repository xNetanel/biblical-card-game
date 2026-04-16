using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Turn State")]
    public bool isPlayerTurn = true;
    public int turnNumber = 1;

    [Header("Mana")]
    public int playerMana = 0;
    public int playerMaxMana = 0;
    public int opponentMana = 0;
    public int opponentMaxMana = 0;
    private const int MaxManaCap = 10;

    [Header("Hand")]
    public List<Card> playerHand = new List<Card>();
    public List<Card> playerDeck = new List<Card>();
    public List<Card> opponentDeck = new List<Card>();

    [Header("UI References")]
    public TextMeshProUGUI playerManaText;
    public TextMeshProUGUI opponentManaText;
    public TextMeshProUGUI turnText;
    public GameObject winPanel;
    public GameObject losePanel;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        Debug.Log("Game started.");
        BuildDecks();
        ShuffleDeck();
        ShuffleOpponentDeck();
        DrawCards(4);
        StartPlayerTurn();
    }

    void BuildDecks()
    {
        playerDeck.Clear();
        opponentDeck.Clear();

        if (DeckBuilder.deckHasBeenBuilt && DeckBuilder.builtDeck != null && DeckBuilder.builtDeck.Count == 30)
        {
            Debug.Log("Using player built deck.");
            foreach (Card card in DeckBuilder.builtDeck)
                playerDeck.Add(card.Clone());
        }
        else
        {
            Debug.Log("No built deck found, using all cards.");
            foreach (Card card in CardDatabase.Instance.allCards)
                playerDeck.Add(card.Clone());
        }

        foreach (Card card in CardDatabase.Instance.allCards)
            opponentDeck.Add(card.Clone());

        Debug.Log($"Player deck built with {playerDeck.Count} cards.");
        Debug.Log($"Opponent deck built with {opponentDeck.Count} cards.");
    }

    void ShuffleDeck()
    {
        for (int i = playerDeck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Card temp = playerDeck[i];
            playerDeck[i] = playerDeck[j];
            playerDeck[j] = temp;
        }
        Debug.Log("Deck shuffled.");
    }

    void ShuffleOpponentDeck()
    {
        for (int i = opponentDeck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Card temp = opponentDeck[i];
            opponentDeck[i] = opponentDeck[j];
            opponentDeck[j] = temp;
        }
    }

    public void DrawCards(int amount)
    {
        int drawn = 0;
        for (int i = 0; i < amount; i++)
        {
            if (playerDeck.Count == 0)
            {
                Debug.Log("Deck is empty.");
                break;
            }
            if (playerHand.Count >= 10)
            {
                Debug.Log("Hand is full. Card burned.");
                playerDeck.RemoveAt(0);
                break;
            }
            Card card = playerDeck[0];
            playerDeck.RemoveAt(0);
            playerHand.Add(card);
            drawn++;
            Debug.Log($"Drew: {card.cardName}");
        }

        if (drawn > 0)
        {
            AbilityManager.Instance.TriggerOnDraw(true, drawn);
            HandManager.Instance.RefreshHand();
        }
    }

    public bool PlayCard(Card card)
    {
        if (!isPlayerTurn)
        {
            Debug.Log("Not your turn.");
            return false;
        }

        if (card.manaCost > playerMana)
        {
            Debug.Log($"Not enough mana to play {card.cardName}. Cost: {card.manaCost}, Mana: {playerMana}");
            return false;
        }

        if (card.isUnit)
        {
            bool played = BoardManager.Instance.PlayCardToBoard(card, isPlayer: true);
            if (!played) return false;
        }

        playerMana -= card.manaCost;
        playerHand.Remove(card);
        UpdateManaUI();
        Debug.Log($"Played {card.cardName}. Mana remaining: {playerMana}");
        return true;
    }

    public void EndTurn()
    {
        if (!isPlayerTurn) return;

        Debug.Log("Player ends turn.");
        AbilityManager.Instance.TriggerEndOfTurnAbilities(true);
        AbilityManager.Instance.CheckPassiveAbilities(true);
        isPlayerTurn = false;
        BoardManager.Instance.RefreshAllUnitsForNewTurn(isPlayer: false);
        StartCoroutine(OpponentTurnWithDelay());
    }

    void StartPlayerTurn()
    {
        isPlayerTurn = true;

        playerMaxMana = Mathf.Min(playerMaxMana + 1, MaxManaCap);
        playerMana = playerMaxMana;

        BoardManager.Instance.RefreshAllUnitsForNewTurn(isPlayer: true);

        if (turnNumber > 1)
            DrawCards(1);

        UpdateManaUI();
        UpdateTurnUI();
        HandManager.Instance.RefreshHand();

        Debug.Log($"Player turn {turnNumber}. Mana: {playerMana}/{playerMaxMana}");
    }

    IEnumerator OpponentTurnWithDelay()
    {
        yield return new WaitForSeconds(1f);

        opponentMaxMana = Mathf.Min(opponentMaxMana + 1, MaxManaCap);
        opponentMana = opponentMaxMana;
        UpdateManaUI();
        Debug.Log($"Opponent turn. Mana: {opponentMana}/{opponentMaxMana}");

        yield return new WaitForSeconds(0.5f);

        if (MainMenu.isHardMode)
        {
            yield return StartCoroutine(RunAITurn());
        }
        else
        {
            yield return StartCoroutine(RunEasyTurn());
        }

        yield return new WaitForSeconds(0.5f);
        EndOpponentTurn();
    }

    IEnumerator RunEasyTurn()
    {
        Debug.Log("Opponent is taking an easy turn.");
        if (opponentDeck.Count > 0)
        {
            Card cardToPlay = opponentDeck.Find(c => c.isUnit && c.manaCost <= opponentMana);
            if (cardToPlay != null)
            {
                BoardManager.Instance.PlayCardToBoard(cardToPlay, isPlayer: false);
                opponentMana -= cardToPlay.manaCost;
                opponentDeck.Remove(cardToPlay);
                Debug.Log($"Opponent played {cardToPlay.cardName}.");
                yield return new WaitForSeconds(0.8f);

                Card secondCard = opponentDeck.Find(c => c.isUnit && c.manaCost <= opponentMana);
                if (secondCard != null)
                {
                    BoardManager.Instance.PlayCardToBoard(secondCard, isPlayer: false);
                    opponentMana -= secondCard.manaCost;
                    opponentDeck.Remove(secondCard);
                    Debug.Log($"Opponent played {secondCard.cardName}.");
                    yield return new WaitForSeconds(0.8f);
                }
            }
        }

        List<CardUI> unitsToAct = new List<CardUI>(BoardManager.Instance.opponentUnits);
        foreach (CardUI unit in unitsToAct)
        {
            if (unit == null || unit.hasAttackedThisTurn) continue;
            yield return new WaitForSeconds(0.6f);
            if (BoardManager.Instance.playerUnits.Count > 0)
            {
                CardUI target = BoardManager.Instance.playerUnits[0];
                BoardManager.Instance.ResolveCombat(unit, target);
                Debug.Log($"Opponent {unit.cardData.cardName} attacks {target.cardData.cardName}.");
            }
            else
            {
                AltarManager.Instance.DamagePlayerAltar(unit.currentAttack);
                unit.hasAttackedThisTurn = true;
                Debug.Log($"Opponent {unit.cardData.cardName} attacks your Altar for {unit.currentAttack}.");
            }
            yield return new WaitForSeconds(0.4f);
        }
    }

    IEnumerator RunAITurn()
    {
        List<string> plan = null;

        yield return StartCoroutine(AIOpponent.Instance.DecideAndExecuteTurn(
            opponentDeck,
            new List<CardUI>(BoardManager.Instance.opponentUnits),
            new List<CardUI>(BoardManager.Instance.playerUnits),
            opponentMana,
            AltarManager.Instance.opponentAltarHealth,
            AltarManager.Instance.playerAltarHealth,
            (receivedPlan) => { plan = receivedPlan; }
        ));

        if (plan == null)
        {
            Debug.Log("AI: No plan received. Ending turn.");
            yield break;
        }

        foreach (string action in plan)
        {
            if (action.Equals("END_TURN", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log("AI: END_TURN reached.");
                break;
            }

            bool actionExecuted = false;
            yield return StartCoroutine(ExecuteAIAction(action, (result) => actionExecuted = result));

            if (actionExecuted)
                yield return new WaitForSeconds(0.8f);
        }
    }

    IEnumerator ExecuteAIAction(string action, System.Action<bool> onComplete)
    {
        List<CardUI> opponentUnits = BoardManager.Instance.opponentUnits;
        List<CardUI> playerUnits = BoardManager.Instance.playerUnits;

        if (action.StartsWith("PLAY:", System.StringComparison.OrdinalIgnoreCase))
        {
            string cardName = action.Substring(5).Trim();

            if (opponentUnits.Count >= 6)
            {
                Debug.Log($"AI: Skipping PLAY '{cardName}' — board full.");
                onComplete(false);
                yield break;
            }

            Card card = opponentDeck.Find(c =>
                string.Equals(c.cardName.Trim(), cardName.Trim(),
                    System.StringComparison.OrdinalIgnoreCase) &&
                c.isUnit &&
                c.manaCost <= opponentMana);

            if (card == null)
            {
                Debug.Log($"AI: Skipping PLAY '{cardName}' — not found or too expensive.");
                onComplete(false);
                yield break;
            }

            BoardManager.Instance.PlayCardToBoard(card, isPlayer: false);
            opponentMana -= card.manaCost;
            opponentDeck.Remove(card);
            Debug.Log($"AI played {card.cardName}.");
            onComplete(true);
        }
        else if (action.StartsWith("ATTACK:", System.StringComparison.OrdinalIgnoreCase))
        {
            string[] parts = action.Substring(7).Split(
                new string[] { "->" }, System.StringSplitOptions.None);

            if (parts.Length != 2)
            {
                Debug.Log($"AI: Skipping malformed ATTACK '{action}'.");
                onComplete(false);
                yield break;
            }

            string attackerName = parts[0].Trim();
            string targetName = parts[1].Trim();

            CardUI attacker = opponentUnits.Find(u =>
                !u.hasAttackedThisTurn &&
                u.cardData.cardName.IndexOf(attackerName,
                    System.StringComparison.OrdinalIgnoreCase) >= 0);

            CardUI target = playerUnits.Find(u =>
                u.cardData.cardName.IndexOf(targetName,
                    System.StringComparison.OrdinalIgnoreCase) >= 0);

            if (attacker == null)
            {
                Debug.Log($"AI: Skipping ATTACK — '{attackerName}' not found or already attacked.");
                onComplete(false);
                yield break;
            }

            if (target == null)
            {
                Debug.Log($"AI: Skipping ATTACK — target '{targetName}' not found.");
                onComplete(false);
                yield break;
            }

            BoardManager.Instance.ResolveCombat(attacker, target);
            Debug.Log($"AI: {attacker.cardData.cardName} attacks {target.cardData.cardName}.");
            onComplete(true);
        }
        else if (action.StartsWith("ATTACK_ALTAR:", System.StringComparison.OrdinalIgnoreCase))
        {
            if (playerUnits.Count > 0)
            {
                Debug.Log("AI: Skipping ATTACK_ALTAR — player has units.");
                onComplete(false);
                yield break;
            }

            string attackerName = action.Substring(13).Trim();
            CardUI attacker = opponentUnits.Find(u =>
                !u.hasAttackedThisTurn &&
                u.cardData.cardName.IndexOf(attackerName,
                    System.StringComparison.OrdinalIgnoreCase) >= 0);

            if (attacker == null)
            {
                Debug.Log($"AI: Skipping ATTACK_ALTAR — '{attackerName}' not found or already attacked.");
                onComplete(false);
                yield break;
            }

            // Play animation toward opponent altar position
            Vector3 altarDirection = attacker.transform.position + new Vector3(0, 80f, 0);
            yield return StartCoroutine(attacker.AttackAnimation(altarDirection));

            AltarManager.Instance.DamagePlayerAltar(attacker.currentAttack);
            attacker.hasAttackedThisTurn = true;
            AudioManager.Instance.PlayCombatHit();
            Debug.Log($"AI: {attacker.cardData.cardName} attacks altar for {attacker.currentAttack}.");
            onComplete(true);
        }
        else
        {
            Debug.Log($"AI: Unknown action '{action}', skipping.");
            onComplete(false);
        }
    }

    void EndOpponentTurn()
    {
        AbilityManager.Instance.TriggerEndOfTurnAbilities(false);
        AbilityManager.Instance.CheckPassiveAbilities(false);
        turnNumber++;
        Debug.Log("Opponent ends turn.");
        StartPlayerTurn();
    }

    public void EndGame(bool winner)
    {
        isPlayerTurn = false;
        StopAllCoroutines();

        if (winner)
        {
            Debug.Log("You win!");
            AudioManager.Instance.PlayWin();
            if (winPanel != null) winPanel.SetActive(true);
        }
        else
        {
            Debug.Log("You lose!");
            AudioManager.Instance.PlayLose();
            if (losePanel != null) losePanel.SetActive(true);
        }
    }

    void UpdateManaUI()
    {
        if (playerManaText != null)
            playerManaText.text = $"Mana: {playerMana}/{playerMaxMana}";
        if (opponentManaText != null)
            opponentManaText.text = $"Mana: {opponentMana}/{opponentMaxMana}";
    }

    void UpdateTurnUI()
    {
        if (turnText != null)
            turnText.text = $"Turn {turnNumber}";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public bool CastScriptureCard(Card card)
    {
        if (card.manaCost > playerMana)
        {
            Debug.Log($"Not enough mana to cast {card.cardName}. Cost: {card.manaCost}, Mana: {playerMana}");
            return false;
        }

        playerMana -= card.manaCost;
        playerHand.Remove(card);
        AbilityManager.Instance.CastScripture(card, true);
        UpdateManaUI();
        HandManager.Instance.RefreshHand();
        Debug.Log($"Cast {card.cardName}. Mana remaining: {playerMana}");
        return true;
    }
}