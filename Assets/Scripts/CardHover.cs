using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hover Settings")]
    public float liftAmount = 20f;
    public float liftSpeed = 8f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isHovering = false;
    private RectTransform rectTransform;
    private CardUI cardUI;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        cardUI = GetComponent<CardUI>();
        originalPosition = rectTransform.anchoredPosition3D;
        targetPosition = originalPosition;
    }

    void Update()
    {
        rectTransform.anchoredPosition3D = Vector3.Lerp(
            rectTransform.anchoredPosition3D,
            targetPosition,
            Time.deltaTime * liftSpeed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetPosition = originalPosition + new Vector3(0, liftAmount, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetPosition = originalPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log($"Right clicked. cardUI is null: {cardUI == null}. ZoomManager is null: {CardZoomManager.Instance == null}");
            if (CardZoomManager.Instance != null && cardUI != null)
                CardZoomManager.Instance.ShowZoom(cardUI);
        }
    }

    public void ResetPosition()
    {
        originalPosition = rectTransform.anchoredPosition3D;
        targetPosition = originalPosition;
    }
}