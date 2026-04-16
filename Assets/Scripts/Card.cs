using System.Collections.Generic;
using UnityEngine;

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "NewCard", menuName = "Biblical/Card")]
public class Card : ScriptableObject
{
    [Header("Card Identity")]
    public string cardName;
    public Rarity rarity;
    public Sprite cardArt;

    [Header("Stats")]
    public int manaCost;
    [HideInInspector]
    public int originalManaCost;
    public int attack;
    public int health;

    [Header("Ability")]
    [TextArea(3, 6)]
    public string abilityDescription;

    [Header("Flags")]
    public bool isUnit;  // false for Miracles and Artifacts

    public Card Clone()
    {
        Card clone = ScriptableObject.CreateInstance<Card>();
        clone.cardName = this.cardName;
        clone.rarity = this.rarity;
        clone.cardArt = this.cardArt;
        clone.manaCost = this.manaCost;
        clone.originalManaCost = this.manaCost;
        clone.attack = this.attack;
        clone.health = this.health;
        clone.abilityDescription = this.abilityDescription;
        clone.isUnit = this.isUnit;
        return clone;
    }
}