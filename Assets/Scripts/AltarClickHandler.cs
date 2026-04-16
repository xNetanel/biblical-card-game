using UnityEngine;
using UnityEngine.UI;

public class AltarClickHandler : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(OnAltarClicked);
    }

    void OnAltarClicked()
    {
        if (BoardManager.Instance.opponentUnits.Count > 0)
        {
            Debug.Log("Cannot attack Altar while opponent has units on board.");
            return;
        }

        CardUI selected = BoardManager.Instance.GetSelectedCard();
        if (selected == null)
        {
            Debug.Log("No card selected to attack with.");
            return;
        }

        if (selected.hasAttackedThisTurn)
        {
            Debug.Log($"{selected.cardData.cardName} has already attacked this turn.");
            return;
        }

        BoardManager.Instance.AttackAltar(selected);
    }
}