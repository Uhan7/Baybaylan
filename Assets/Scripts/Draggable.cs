using UnityEngine;
using UnityEngine.EventSystems;

// Setup -----------------------------------------------------------------------
public interface IDragNotify
{
    void OnDragBegin();
    void OnDragEnd();
}

// Main Class ------------------------------------------------------------------
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private CanvasGroup canvasGroup;

    [Header("References")]
    [SerializeField] private Canvas canvas;

    [Header("Transform")]
    [HideInInspector] private RectTransform rectTransform;

    [Header("Flags")]
    [HideInInspector] public bool isBeingDragged;
    [HideInInspector] private IDragNotify dragNotify;

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

        dragNotify?.OnDragBegin();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isBeingDragged = false;
        canvasGroup.blocksRaycasts = true;

        dragNotify?.OnDragEnd();
    }

    // Helper Functions --------------------------------------------------------
    private void InitializeAwake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        dragNotify = GetComponent<IDragNotify>();
    }
}
