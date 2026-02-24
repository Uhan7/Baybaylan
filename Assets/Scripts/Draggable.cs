using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IEndDragHandler
{
    // Variables ---------------------------------------------------------------

    [Header("References")]
    [SerializeField] private Canvas tilesCanvas;

    [Header("Transform")]
    [HideInInspector] private RectTransform rectTransform;

    [Header("Flags")]
    [HideInInspector] public bool isBeingDragged;

    // Main Functions ----------------------------------------------------------

    private void Awake()
    {
        InitializeAwake();
    }

    public void OnDrag(PointerEventData eventData)
    {
        isBeingDragged = true;
        rectTransform.anchoredPosition += eventData.delta / tilesCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isBeingDragged = false;
    }

    // Helper Functions --------------------------------------------------------

    private void InitializeAwake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
}
