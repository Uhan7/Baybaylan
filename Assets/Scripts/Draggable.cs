using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Variables ---------------------------------------------------------------

    [Header("Components")]
    [HideInInspector] private CanvasGroup canvasGroup;

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

    public void OnBeginDrag(PointerEventData eventData)
    {
        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / tilesCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isBeingDragged = false;
        canvasGroup.blocksRaycasts = true;
    }



    // Helper Functions --------------------------------------------------------

    private void InitializeAwake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }
}
