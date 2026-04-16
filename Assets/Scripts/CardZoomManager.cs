using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardZoomManager : MonoBehaviour
{
    public static CardZoomManager Instance;

    [Header("Zoom Overlay")]
    public GameObject zoomOverlay;

    [Header("Zoom Card Components")]
    public Image cardZoomFrame;
    public Image cardZoomArt;
    public TextMeshProUGUI cardZoomNameText;
    public TextMeshProUGUI abilityZoomText;
    public TextMeshProUGUI attackZoomText;
    public TextMeshProUGUI healthZoomText;
    public TextMeshProUGUI manaZoomCost;
    public TextMeshProUGUI cardZoomTypeText;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowZoom(CardUI card)
    {
        if (zoomOverlay == null) return;

        zoomOverlay.SetActive(true);
        Debug.Log($"ShowZoom called for {card.cardData.cardName}. Overlay active: {zoomOverlay.activeSelf}. Name text null: {cardZoomNameText == null}");

        if (cardZoomNameText != null) cardZoomNameText.text = card.cardData.cardName;
        if (manaZoomCost != null)
        {
            manaZoomCost.text = card.cardData.manaCost.ToString();
            if (card.cardData.manaCost < card.baseManaCost)
                manaZoomCost.color = new Color(0.1f, 0.8f, 0.1f);
            else if (card.cardData.manaCost > card.baseManaCost)
                manaZoomCost.color = new Color(0.9f, 0.1f, 0.1f);
            else
                manaZoomCost.color = Color.white;
        }
        if (attackZoomText != null)
        {
            attackZoomText.text = card.cardData.isUnit ? card.currentAttack.ToString() : "—";
            if (card.cardData.isUnit)
            {
                if (card.currentAttack > card.cardData.attack)
                    attackZoomText.color = new Color(0.1f, 0.8f, 0.1f);
                else if (card.currentAttack < card.cardData.attack)
                    attackZoomText.color = new Color(0.9f, 0.1f, 0.1f);
                else
                    attackZoomText.color = Color.white;
            }
        }
        if (healthZoomText != null)
        {
            healthZoomText.text = card.cardData.isUnit ? card.currentHealth.ToString() : "—";
            if (card.cardData.isUnit)
            {
                if (card.currentHealth > card.cardData.health)
                    healthZoomText.color = new Color(0.1f, 0.8f, 0.1f);
                else if (card.currentHealth < card.cardData.health)
                    healthZoomText.color = new Color(0.9f, 0.1f, 0.1f);
                else
                    healthZoomText.color = Color.white;
            }
        }
        if (abilityZoomText != null) abilityZoomText.text = card.cardData.abilityDescription;
        if (cardZoomTypeText != null) cardZoomTypeText.text = card.cardData.rarity.ToString();
        if (cardZoomArt != null && card.cardData.cardArt != null)
            cardZoomArt.sprite = card.cardData.cardArt;
        if (cardZoomFrame != null)
            ApplyFrameColor(card.cardData.rarity);
    }

    public void HideZoom()
    {
        if (zoomOverlay != null)
            zoomOverlay.SetActive(false);
    }

    void ApplyFrameColor(Rarity type)
    {
        if (cardZoomFrame == null) return;

        switch (type)
        {
            case Rarity.Common:
                cardZoomFrame.color = new Color(0.7f, 0.7f, 0.7f); break;
            case Rarity.Rare:
                cardZoomFrame.color = new Color(0.1f, 0.4f, 0.9f); break;
            case Rarity.Epic:
                cardZoomFrame.color = new Color(0.6f, 0.1f, 0.8f); break;
            case Rarity.Legendary:
                cardZoomFrame.color = new Color(0.9f, 0.6f, 0.1f); break;
        }
    }
}