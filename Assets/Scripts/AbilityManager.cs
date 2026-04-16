using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // =====================
    // BATTLECRY ABILITIES
    // =====================

    public void TriggerBattlecry(CardUI cardUI, bool isPlayer)
    {
        string name = cardUI.cardData.cardName;
        

        switch (name)
        {
            case "Adam of Eden":
                Battlecry_AdamOfEden(isPlayer);
                break;
            case "EVE":
                Battlecry_Eve(cardUI, isPlayer);
                break;
            case "SETH":
                Battlecry_Seth(isPlayer);
                break;
            case "PHAROAH":
                Battlecry_Pharaoh(isPlayer);
                break;
            case "REBECCA":
                Battlecry_Rebecca(cardUI, isPlayer);
                break;
            case "Abimelech of Gerar":
                Battlecry_Abimelech(isPlayer);
                break;
            case "THE SERPENT":
                Battlecry_Serpent(isPlayer);
                break;
            case "CAIN":
                Battlecry_Cain(cardUI, isPlayer);
                break;
            case "SARAH":
                Battlecry_Sarah(isPlayer);
                break;
            case "BENJAMIN":
                Battlecry_Benjamin(isPlayer);
                break;
            case "LOT":
                Battlecry_Lot(cardUI, isPlayer);
                break;
            case "RACHEL":
                Battlecry_Rachel(cardUI, isPlayer);
                break;
            case "JUDAH":
                Battlecry_Judah(cardUI, isPlayer);
                break;
            case "POTIPHAR":
                Battlecry_Potiphar(isPlayer);
                break;
        }
    }

    void Battlecry_AdamOfEden(bool isPlayer)
    {
        if (!isPlayer) return;
        GameManager.Instance.DrawCards(1);
        Debug.Log("Adam of Eden: Drew 1 card.");
    }

    void Battlecry_Eve(CardUI eve, bool isPlayer)
    {
        List<CardUI> friendlyUnits = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        List<CardUI> others = friendlyUnits.FindAll(u => u != eve);
        if (others.Count == 0)
        {
            Debug.Log("Eve: No other friendly units to buff.");
            return;
        }

        CardUI target = others[UnityEngine.Random.Range(0, others.Count)];
        target.currentAttack += 1;
        target.currentHealth += 1;
        target.UpdateStatsUI();
        AudioManager.Instance.PlayBuff();
        Debug.Log($"Eve: Gave {target.cardData.cardName} +1/+1. Now {target.currentAttack}/{target.currentHealth}.");
    }

    void Battlecry_Seth(bool isPlayer)
    {
        SummonToken("SERVANT", 1, 1, isPlayer);
        Debug.Log("Seth: Summoned a 1/1 Servant.");
    }

    void Battlecry_Pharaoh(bool isPlayer)
    {
        List<CardUI> enemyUnits = isPlayer
            ? BoardManager.Instance.opponentUnits
            : BoardManager.Instance.playerUnits;

        List<CardUI> targets = new List<CardUI>(enemyUnits);
        foreach (CardUI unit in targets)
            unit.TakeDamage(1, fromSpell: true);

        Debug.Log("Pharaoh: Dealt 1 damage to all enemy units.");
    }

    void Battlecry_Rebecca(CardUI rebecca, bool isPlayer)
    {
        if (!isPlayer) return;
        GameManager.Instance.DrawCards(1);

        List<CardUI> friendlyUnits = BoardManager.Instance.playerUnits;
        List<CardUI> others = friendlyUnits.FindAll(u => u != rebecca);
        if (others.Count == 0)
        {
            Debug.Log("Rebecca: Drew a card but no friendly units to buff.");
            return;
        }

        CardUI target = others[UnityEngine.Random.Range(0, others.Count)];
        target.currentHealth += 1;
        target.UpdateStatsUI();
        AudioManager.Instance.PlayBuff();
        Debug.Log($"Rebecca: Drew a card and gave {target.cardData.cardName} +1 Health. Now {target.currentAttack}/{target.currentHealth}.");
    }

    void Battlecry_Abimelech(bool isPlayer)
    {
        List<CardUI> enemyUnits = isPlayer
            ? BoardManager.Instance.opponentUnits
            : BoardManager.Instance.playerUnits;

        if (enemyUnits.Count == 0)
        {
            Debug.Log("Abimelech: No enemy units to freeze.");
            return;
        }

        if (isPlayer)
        {
            Debug.Log("Abimelech: Select an enemy unit to freeze.");
            TargetingManager.Instance.RequestTarget(
                enemy: true,
                friendly: false,
                callback: (target) =>
                {
                    target.isFrozen = true;
                    Debug.Log($"Abimelech: {target.cardData.cardName} is frozen and cannot attack next turn.");
                    AudioManager.Instance.PlayDebuff();
                }
            );
        }
        else
        {
            CardUI target = enemyUnits[UnityEngine.Random.Range(0, enemyUnits.Count)];
            target.isFrozen = true;
            Debug.Log($"Opponent Abimelech: {target.cardData.cardName} is frozen and cannot attack next turn.");
            AudioManager.Instance.PlayDebuff();
        }
    }

    void Battlecry_Serpent(bool isPlayer)
    {
        List<CardUI> enemyUnits = isPlayer
            ? BoardManager.Instance.opponentUnits
            : BoardManager.Instance.playerUnits;

        if (enemyUnits.Count == 0)
        {
            Debug.Log("The Serpent: No enemy units to debuff.");
            return;
        }

        if (isPlayer)
        {
            Debug.Log("The Serpent: Select an enemy unit to give -1 Attack.");
            TargetingManager.Instance.RequestTarget(
                enemy: true,
                friendly: false,
                callback: (target) =>
                {
                    target.currentAttack = Mathf.Max(0, target.currentAttack - 1);
                    target.UpdateStatsUI();
                    Debug.Log($"The Serpent: Gave {target.cardData.cardName} -1 Attack. Now {target.currentAttack}/{target.currentHealth}.");
                    AudioManager.Instance.PlayDebuff();
                }
            );
        }
        else
        {
            CardUI target = enemyUnits[UnityEngine.Random.Range(0, enemyUnits.Count)];
            target.currentAttack = Mathf.Max(0, target.currentAttack - 1);
            target.UpdateStatsUI();
            Debug.Log($"Opponent Serpent: Gave {target.cardData.cardName} -1 Attack. Now {target.currentAttack}/{target.currentHealth}.");
        }
    }

    void Battlecry_Cain(CardUI cain, bool isPlayer)
    {
        List<CardUI> friendlyUnits = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        List<CardUI> others = friendlyUnits.FindAll(u => u != cain);
        if (others.Count == 0)
        {
            Debug.Log("Cain: No other friendly units to damage.");
            return;
        }

        if (isPlayer)
        {
            Debug.Log("Cain: Select a friendly unit to deal 2 damage to, then Cain gains +3 Attack.");
            TargetingManager.Instance.RequestTarget(
                enemy: false,
                friendly: true,
                callback: (target) =>
                {
                    if (target == cain)
                    {
                        Debug.Log("Cain: Cannot target yourself. Ability fizzled.");
                        return;
                    }
                    target.TakeDamage(2);
                    cain.currentAttack += 3;
                    cain.UpdateStatsUI();
                    Debug.Log($"Cain: Dealt 2 damage to {target.cardData.cardName}, gained +3 Attack. Now {cain.currentAttack}/{cain.currentHealth}.");
                }
            );
        }
        else
        {
            CardUI target = others[UnityEngine.Random.Range(0, others.Count)];
            target.TakeDamage(2);
            cain.currentAttack += 3;
            cain.UpdateStatsUI();
            Debug.Log($"Opponent Cain: Dealt 2 damage to {target.cardData.cardName}, gained +3 Attack. Now {cain.currentAttack}/{cain.currentHealth}.");
        }
    }

    void Battlecry_Sarah(bool isPlayer)
    {
        if (!isPlayer) return;

        List<Card> hand = GameManager.Instance.playerHand;
        List<Card> scriptures = hand.FindAll(c => !c.isUnit);

        if (scriptures.Count == 0)
        {
            Debug.Log("Sarah: No Scripture card in hand to reduce.");
            return;
        }

        Card target = scriptures[UnityEngine.Random.Range(0, scriptures.Count)];
        target.manaCost = Mathf.Max(0, target.manaCost - 2);
        Debug.Log($"Sarah: Reduced cost of {target.cardName} by 2. New cost: {target.manaCost}.");
        HandManager.Instance.RefreshHand();
    }

    void Battlecry_Benjamin(bool isPlayer)
    {
        if (!isPlayer) return;

        List<Card> hand = GameManager.Instance.playerHand;
        List<CardUI> board = BoardManager.Instance.playerUnits;

        bool josephInHand = hand.Exists(c => c.cardName == "Joseph");
        bool josephOnBoard = board.Exists(u => u.cardData.cardName == "Joseph");

        if (josephInHand || josephOnBoard)
        {
            AltarManager.Instance.HealPlayerAltar(2);
            Debug.Log($"Benjamin: Joseph found ({(josephInHand ? "in hand" : "on board")}). Healed Altar by 2.");
        }
        else
        {
            Debug.Log("Benjamin: Joseph not found in hand or on board. Ability fizzled.");
        }
    }

    void Battlecry_Lot(CardUI lot, bool isPlayer)
    {
        List<CardUI> friendlyUnits = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        Debug.Log($"Lot battlecry fired. isPlayer: {isPlayer}. Friendly unit count: {friendlyUnits.Count}");
        foreach (CardUI u in friendlyUnits)
            Debug.Log($" - Unit on board: {u.cardData.cardName}");

        if (friendlyUnits.Count == 1)
        {
            lot.currentAttack += 2;
            lot.UpdateStatsUI();
            AudioManager.Instance.PlayBuff();
            Debug.Log($"Lot: Gained +2 Attack. Now {lot.currentAttack}/{lot.currentHealth}.");
        }
        else
        {
            Debug.Log("Lot: Not alone, ability fizzled.");
        }
    }

    void Battlecry_Rachel(CardUI rachel, bool isPlayer)
    {
        List<CardUI> friendlyUnits = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        List<CardUI> others = friendlyUnits.FindAll(u => u != rachel);
        if (others.Count == 0)
        {
            Debug.Log("Rachel: No other friendly units to buff.");
            return;
        }

        foreach (CardUI unit in others)
        {
            unit.currentHealth += 1;
            unit.UpdateStatsUI();
            AudioManager.Instance.PlayBuff();
            Debug.Log($"Rachel: Gave {unit.cardData.cardName} +1 Health. Now {unit.currentAttack}/{unit.currentHealth}.");
        }
    }

    void Battlecry_Judah(CardUI judah, bool isPlayer)
    {
        List<CardUI> friendlyUnits = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        List<CardUI> others = friendlyUnits.FindAll(u => u != judah);
        if (others.Count == 0)
        {
            Debug.Log("Judah: No other friendly units to buff.");
            return;
        }

        foreach (CardUI unit in others)
        {
            unit.currentAttack += 1;
            unit.UpdateStatsUI();
            AudioManager.Instance.PlayBuff();
            Debug.Log($"Judah: Gave {unit.cardData.cardName} +1 Attack. Now {unit.currentAttack}/{unit.currentHealth}.");
        }
    }

    void Battlecry_Potiphar(bool isPlayer)
    {
        List<CardUI> enemyUnits = isPlayer
            ? BoardManager.Instance.opponentUnits
            : BoardManager.Instance.playerUnits;

        if (enemyUnits.Count == 0)
        {
            Debug.Log("Potiphar: No enemy units to silence.");
            return;
        }

        if (isPlayer)
        {
            Debug.Log("Potiphar: Select an enemy unit to silence.");
            TargetingManager.Instance.RequestTarget(
                enemy: true,
                friendly: false,
                callback: (target) =>
                {
                    SilenceUnit(target);
                }
            );
        }
        else
        {
            CardUI target = enemyUnits[UnityEngine.Random.Range(0, enemyUnits.Count)];
            SilenceUnit(target);
            AudioManager.Instance.PlayDebuff();
        }
    }

    void SilenceUnit(CardUI target)
    {
        target.isSilenced = true;
        target.currentAttack = target.cardData.attack;
        target.currentHealth = Mathf.Min(target.currentHealth, target.cardData.health);
        target.isFrozen = false;
        target.isProtected = false;
        target.cardData.abilityDescription = "Silenced.";
        if (target.abilityText != null)
            target.abilityText.text = "Silenced.";
        target.UpdateStatsUI();
        Debug.Log($"Potiphar: Silenced {target.cardData.cardName}. Stats reset to {target.currentAttack}/{target.currentHealth}.");
    }

    // =====================
    // DEATHRATTLE ABILITIES
    // =====================

    public void TriggerDeathrattle(CardUI cardUI, bool isPlayer)
    {
        if (cardUI.isSilenced) return;

        string name = cardUI.cardData.cardName;

        switch (name)
        {
            case "ABEL":
                Deathrattle_Abel(isPlayer);
                break;
            case "Adam, The Fallen":
                Deathrattle_AdamTheFallen(isPlayer);
                break;
        }
    }

    void Deathrattle_Abel(bool isPlayer)
    {
        List<CardUI> friendlyUnits = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        if (friendlyUnits.Count == 0)
        {
            Debug.Log("Abel Deathrattle: No friendly units to buff.");
            return;
        }

        CardUI target = friendlyUnits[UnityEngine.Random.Range(0, friendlyUnits.Count)];
        target.currentAttack += 1;
        target.currentHealth += 1;
        target.UpdateStatsUI();
        AudioManager.Instance.PlayBuff();
        Debug.Log($"Abel Deathrattle: Gave {target.cardData.cardName} +1/+1. Now {target.currentAttack}/{target.currentHealth}.");
    }

    void Deathrattle_AdamTheFallen(bool isPlayer)
    {
        Debug.Log($"Adam, The Fallen Deathrattle: Dealing 2 damage to {(isPlayer ? "player" : "opponent")} Altar.");
        if (isPlayer)
            AltarManager.Instance.DamagePlayerAltar(2);
        else
            AltarManager.Instance.DamageOpponentAltar(2);
        Debug.Log($"Altar health after Adam deathrattle - Player: {AltarManager.Instance.playerAltarHealth}, Opponent: {AltarManager.Instance.opponentAltarHealth}");
    }

    // =====================
    // ON DAMAGE TAKEN
    // =====================

    public void TriggerOnDamageTaken(CardUI cardUI, int amount, bool isPlayer)
    {
        if (cardUI.isSilenced) return;

        switch (cardUI.cardData.cardName)
        {
            case "LEAH":
                OnDamage_Leah(isPlayer);
                break;
        }
    }

    void OnDamage_Leah(bool isPlayer)
    {
        if (isPlayer)
            AltarManager.Instance.HealPlayerAltar(2);
        else
            AltarManager.Instance.HealOpponentAltar(2);
        AudioManager.Instance.PlayBuff();
        Debug.Log("Leah: Took damage, restored 2 health to Altar.");
    }

    // =====================
    // END OF TURN ABILITIES
    // =====================

    public void TriggerEndOfTurnAbilities(bool isPlayer)
    {
        List<CardUI> units = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        List<CardUI> snapshot = new List<CardUI>(units);
        foreach (CardUI unit in snapshot)
        {
            if (unit.isSilenced) continue;

            switch (unit.cardData.cardName)
            {
                case "ABRAHAM":
                    EndOfTurn_Abraham(isPlayer);
                    break;
            }
        }
    }

    void EndOfTurn_Abraham(bool isPlayer)
    {
        SummonToken("SERVANT", 1, 1, isPlayer);
        Debug.Log("Abraham: End of turn, summoned a 1/1 Servant.");
    }

    // =====================
    // ON DRAW ABILITIES
    // =====================

    public void TriggerOnDraw(bool isPlayer, int cardCount)
    {
        List<CardUI> units = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        List<CardUI> isaacCards = units.FindAll(u => u.cardData.cardName == "ISAAC");

        foreach (CardUI isaac in isaacCards)
        {
            isaac.currentHealth += cardCount;
            isaac.UpdateStatsUI();
            AudioManager.Instance.PlayBuff();
            Debug.Log($"Isaac: Drew {cardCount} card(s), gained +{cardCount} Health. Now {isaac.currentAttack}/{isaac.currentHealth}.");
        }
    }


    // =====================
    // PASSIVE ABILITIES
    // =====================

    public void CheckPassiveAbilities(bool isPlayer)
    {
        List<CardUI> friendlyUnits = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        List<CardUI> snapshot = new List<CardUI>(friendlyUnits);
        foreach (CardUI unit in snapshot)
        {
            switch (unit.cardData.cardName)
            {
                case "JOSEPH":
                    Debug.Log("Joseph: Cannot be destroyed by spells.");
                    break;
            }
        }
    }

    // =====================
    // SCRIPTURE ABILITIES
    // =====================

    public void CastScripture(Card card, bool isPlayer)
    {
        Debug.Log($"Casting scripture: {card.cardName}.");
        switch (card.cardName)
        {
            case "MANNA":
                Scripture_Manna(isPlayer);
                break;
            case "Mark of Cain":
                Scripture_MarkOfCain(isPlayer);
                break;
            case "Forbidden Fruit":
                Scripture_ForbiddenFruit(isPlayer);
                break;
            case "The Great Flood":
                Scripture_TheGreatFlood();
                break;
            case "Tower of Babel":
                Scripture_TowerOfBabel(isPlayer);
                break;
            case "The Flaming Sword":
                Scripture_TheFlamingSword(isPlayer);
                break;
        }
    }

    void Scripture_Manna(bool isPlayer)
    {
        if (!isPlayer) return;
        GameManager.Instance.DrawCards(2);
        Debug.Log("Manna: Drew 2 cards.");
    }

    void Scripture_MarkOfCain(bool isPlayer)
    {
        List<CardUI> friendlyUnits = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        if (friendlyUnits.Count == 0)
        {
            Debug.Log("Mark of Cain: No friendly units to heal.");
            return;
        }

        Debug.Log("Mark of Cain: Select a friendly unit to give +2 Health.");
        TargetingManager.Instance.RequestTarget(
            enemy: false,
            friendly: true,
            callback: (target) =>
            {
                target.currentHealth += 2;
                target.UpdateStatsUI();
                AudioManager.Instance.PlayBuff();
                Debug.Log($"Mark of Cain: Gave {target.cardData.cardName} +2 Health. Now {target.currentAttack}/{target.currentHealth}.");
            }
        );
    }

    void Scripture_ForbiddenFruit(bool isPlayer)
    {
        List<CardUI> friendlyUnits = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        if (friendlyUnits.Count == 0)
        {
            Debug.Log("Forbidden Fruit: No friendly units on board.");
            return;
        }

        CardUI randomTarget = friendlyUnits[UnityEngine.Random.Range(0, friendlyUnits.Count)];
        randomTarget.TakeDamage(1);
        Debug.Log($"Forbidden Fruit: Dealt 1 damage to {randomTarget.cardData.cardName}.");

        List<CardUI> remaining = isPlayer
            ? BoardManager.Instance.playerUnits
            : BoardManager.Instance.opponentUnits;

        if (remaining.Count == 0)
        {
            Debug.Log("Forbidden Fruit: No friendly units left to buff.");
            return;
        }

        Debug.Log("Forbidden Fruit: Select a friendly unit to give +2 Attack.");
        TargetingManager.Instance.RequestTarget(
            enemy: false,
            friendly: true,
            callback: (buffed) =>
            {
                buffed.currentAttack += 2;
                buffed.UpdateStatsUI();
                AudioManager.Instance.PlayBuff(); 
                Debug.Log($"Forbidden Fruit: Gave {buffed.cardData.cardName} +2 Attack. Now {buffed.currentAttack}/{buffed.currentHealth}.");
            }
        );
    }

    void Scripture_TheGreatFlood()
    {
        List<CardUI> allUnits = new List<CardUI>();
        allUnits.AddRange(BoardManager.Instance.playerUnits);
        allUnits.AddRange(BoardManager.Instance.opponentUnits);

        foreach (CardUI unit in allUnits)
        {
                unit.TakeDamage(5, fromSpell: true);
        }

        Debug.Log($"The Great Flood occured.");
    }

    void Scripture_TowerOfBabel(bool isPlayer)
    {
        List<CardUI> enemyUnits = isPlayer
            ? BoardManager.Instance.opponentUnits
            : BoardManager.Instance.playerUnits;

        if (enemyUnits.Count == 0)
        {
            Debug.Log("Tower of Babel: No enemy units to target.");
            return;
        }

        Debug.Log("Tower of Babel: Select an enemy unit to swap its Attack and Health.");
        TargetingManager.Instance.RequestTarget(
            enemy: isPlayer,
            friendly: !isPlayer,
            callback: (target) =>
            {
                int oldAttack = target.currentAttack;
                int oldHealth = target.currentHealth;
                target.currentAttack = oldHealth;
                target.currentHealth = oldAttack;
                target.UpdateStatsUI();
                AudioManager.Instance.PlayDebuff();
                Debug.Log($"Tower of Babel: Swapped {target.cardData.cardName} stats. Now {target.currentAttack}/{target.currentHealth}.");
            }
        );
    }

    void Scripture_TheFlamingSword(bool isPlayer)
    {
        List<CardUI> enemyUnits = isPlayer
            ? BoardManager.Instance.opponentUnits
            : BoardManager.Instance.playerUnits;

        if (enemyUnits.Count == 0)
        {
            Debug.Log("The Flaming Sword: No enemy units to target.");
            return;
        }

        Debug.Log("The Flaming Sword: Select an enemy unit to deal 2 damage to.");
        TargetingManager.Instance.RequestTarget(
            enemy: true,
            friendly: false,
            callback: (target) =>
            {
                target.TakeDamage(2, fromSpell: true);
                Debug.Log($"The Flaming Sword: Dealt 2 damage to {target.cardData.cardName}. Now {target.currentAttack}/{target.currentHealth}.");
            }
        );
    }

    // =====================
    // SURVIVING COMBAT ABILITIES
    // =====================

    public void TriggerOnCombatSurvive(CardUI cardUI, bool isPlayer)
    {
        if (cardUI.isSilenced) return;

        switch (cardUI.cardData.cardName)
        {
            case "NOAH":
                if (isPlayer)
                {
                    GameManager.Instance.DrawCards(1);
                    Debug.Log("Noah: Survived combat, drew 1 card.");
                }
                break;
        }
    }

    // =====================
    // TOKEN SUMMONER
    // =====================

    public void SummonToken(string tokenName, int attack, int health, bool isPlayer)
    {
        Card token = CardDatabase.Instance.GetToken(tokenName);
        if (token == null)
        {
            token = ScriptableObject.CreateInstance<Card>();
            token.cardName = tokenName;
            token.rarity = Rarity.Common;
            token.attack = attack;
            token.health = health;
            token.manaCost = 0;
            token.isUnit = true;
        }

        bool success = BoardManager.Instance.PlayCardToBoard(token, isPlayer);
        if (success)
            Debug.Log($"Token summoned: {tokenName} ({attack}/{health}) for {(isPlayer ? "player" : "opponent")}.");
        else
            Debug.Log($"Token could not be summoned: board full.");
    }

    // =====================
    // ON UNIT SUMMONED
    // =====================

    public void TriggerOnUnitSummoned(CardUI summoned, bool isPlayer)
    {
        Debug.Log($"Unit summoned: {summoned.cardData.cardName}");
    }

}