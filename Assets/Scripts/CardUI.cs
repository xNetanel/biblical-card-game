using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;


public class CardUI : MonoBehaviour
{
    [Header("Card Data")]
    public Card cardData;

    [Header("UI Elements")]
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI abilityText;
    public TextMeshProUGUI cardRarityText;
    public Image cardArtImage;
    public Image cardFrame;

    [Header("Frame Colors by Type")]
    public Color warriorColor = new Color(0.8f, 0.2f, 0.2f);
    public Color prophetColor = new Color(0.2f, 0.4f, 0.8f);
    public Color miracleColor = new Color(0.6f, 0.2f, 0.8f);
    public Color artifactColor = new Color(0.8f, 0.6f, 0.1f);
    public Color celestialColor = new Color(0.2f, 0.8f, 0.4f);

    [Header("State")]
    public bool isPlayerCard = true;
    public bool hasAttackedThisTurn = false;
    public bool isFrozen = false;
    public bool isProtected = false;
    public int currentAttack;
    public int currentHealth;
    public int baseManaCost;
    public int combatSurviveCount = 0;
    public bool hasTransformed = false;
    public bool isSilenced = false;
    public bool isSelected = false;


    void Start()
    {
        if (cardData != null)
            RefreshVisuals();
    }

    public void PopulateCard(Card data)
    {
        cardData = data;
        currentAttack = data.attack;
        currentHealth = data.health;
        baseManaCost = data.originalManaCost > 0 ? data.originalManaCost : data.manaCost;

        if (cardNameText != null) cardNameText.text = data.cardName;
        if (manaCostText != null) manaCostText.text = data.manaCost.ToString();
        if (attackText != null) attackText.text = data.isUnit ? data.attack.ToString() : "—";
        if (healthText != null) healthText.text = data.isUnit ? data.health.ToString() : "—";
        if (abilityText != null) abilityText.text = data.abilityDescription;
        if (cardRarityText != null) cardRarityText.text = data.rarity.ToString();
        if (cardArtImage != null && data.cardArt != null) cardArtImage.sprite = data.cardArt;

        ApplyFrameColor(data.rarity);
        hasAttackedThisTurn = true;
        UpdateManaCostUI();
    }

    void ApplyFrameColor(Rarity rarity)
    {
        if (cardFrame == null) return;

        switch (rarity)
        {
            case Rarity.Common:
                cardFrame.color = new Color(0.7f, 0.7f, 0.7f); break;
            case Rarity.Rare:
                cardFrame.color = new Color(0.1f, 0.4f, 0.9f); break;
            case Rarity.Epic:
                cardFrame.color = new Color(0.6f, 0.1f, 0.8f); break;
            case Rarity.Legendary:
                cardFrame.color = new Color(0.9f, 0.6f, 0.1f); break;
        }
    }

    public void TakeDamage(int amount, bool fromSpell = false)
    {
        if (isProtected)
        {
            Debug.Log($"{cardData.cardName} is protected and took no damage.");
            return;
        }

        if (fromSpell && cardData.cardName == "Joseph")
        {
            Debug.Log($"Joseph: Cannot be destroyed by spells. Took no damage.");
            return;
        }

        currentHealth -= amount;
        if (healthText != null) healthText.text = currentHealth.ToString();
        Debug.Log($"{cardData.cardName} took {amount} damage. Health now: {currentHealth}");

        AbilityManager.Instance.TriggerOnDamageTaken(this, amount, isPlayerCard);
        UpdateStatsUI();

        if (currentHealth <= 0)
            Die();
    }

    public void Die()
    {
        BoardManager.Instance.RemoveUnitFromBoard(this);
        AbilityManager.Instance.TriggerDeathrattle(this, isPlayerCard);
        Destroy(gameObject);
    }

    public void RefreshForNewTurn()
    {
        if (isFrozen)
        {
            isFrozen = false;
            hasAttackedThisTurn = true;
            Debug.Log($"{cardData.cardName} is frozen and cannot attack this turn.");
            return;
        }
        isProtected = false;
        hasAttackedThisTurn = false;
    }

    public void OnCardClicked()
    {
        if (TargetingManager.Instance.isTargeting)
        {
            TargetingManager.Instance.SelectTarget(this, isPlayerCard);
            return;
        }

        if (!GameManager.Instance.isPlayerTurn) return;

        if (isPlayerCard)
        {
            if (hasAttackedThisTurn || isFrozen)
            {
                Debug.Log($"{cardData.cardName} cannot attack this turn.");
                return;
            }
            if (hasAttackedThisTurn)
            {
                Debug.Log($"{cardData.cardName} has summoning sickness and cannot attack yet.");
                return;
            }
            BoardManager.Instance.SelectCard(this);
            Debug.Log($"Selected {cardData.cardName} to attack.");
        }
        else
        {
            if (TargetingManager.Instance.isTargeting)
            {
                TargetingManager.Instance.SelectTarget(this, isPlayerCard);
                return;
            }
            CardUI selected = BoardManager.Instance.GetSelectedCard();
            if (selected == null)
            {
                Debug.Log("No friendly card selected.");
                return;
            }
            BoardManager.Instance.SelectCard(this);
        }
    }

    public void UpdateStatsUI()
    {
        if (attackText != null)
        {
            attackText.text = currentAttack.ToString();
            if (currentAttack > cardData.attack)
                attackText.color = new Color(0.1f, 0.8f, 0.1f);
            else if (currentAttack < cardData.attack)
                attackText.color = new Color(0.9f, 0.1f, 0.1f);
            else
                attackText.color = Color.white;
        }

        if (healthText != null)
        {
            healthText.text = currentHealth.ToString();
            if (currentHealth > cardData.health)
                healthText.color = new Color(0.1f, 0.8f, 0.1f);
            else if (currentHealth < cardData.health)
                healthText.color = new Color(0.9f, 0.1f, 0.1f);
            else
                healthText.color = Color.white;
        }
    }

    public void RefreshVisuals()
    {
        if (cardNameText != null) cardNameText.text = cardData.cardName;
        if (manaCostText != null) manaCostText.text = cardData.manaCost.ToString();
        if (attackText != null) attackText.text = cardData.isUnit ? currentAttack.ToString() : "—";
        if (healthText != null) healthText.text = cardData.isUnit ? currentHealth.ToString() : "—";
        if (abilityText != null) abilityText.text = cardData.abilityDescription;
        if (cardRarityText != null) cardRarityText.text = cardData.rarity.ToString();
        ApplyFrameColor(cardData.rarity);
    }

    public void UpdateManaCostUI()
    {
        if (manaCostText == null) return;
        manaCostText.text = cardData.manaCost.ToString();
        if (cardData.manaCost < baseManaCost)
            manaCostText.color = new Color(0.1f, 0.8f, 0.1f);
        else if (cardData.manaCost > baseManaCost)
            manaCostText.color = new Color(0.9f, 0.1f, 0.1f);
        else
            manaCostText.color = Color.white;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (selected)
            StartCoroutine(PulseSelection());
        else
        {
            StopAllCoroutines();
            ApplyFrameColor(cardData.rarity);
        }
    }

    IEnumerator PulseSelection()
    {
        Color originalColor = cardFrame.color;
        Color highlightColor = new Color(1f, 1f, 1f, 1f);
        float speed = 3f;

        while (isSelected)
        {
            float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;
            cardFrame.color = Color.Lerp(originalColor, highlightColor, t);
            yield return null;
        }
    }

    public IEnumerator AttackAnimation(Vector3 targetPosition)
    {
        Vector3 startPosition = transform.position;
        Vector3 direction = (targetPosition - startPosition).normalized;
        Vector3 lungePosition = startPosition + direction * 30f;

        float duration = 0.15f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, lungePosition, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(lungePosition, startPosition, elapsed / duration);
            yield return null;
        }

        transform.position = startPosition;
    }

}