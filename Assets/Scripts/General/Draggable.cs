using UnityEngine;
using UnityEngine.UI;
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
    [HideInInspector] private LayoutElement layoutElement;

    [Header("References")]
    [SerializeField] public Canvas canvas; // To be set by spawners (TileSet.cs)

    [Header("Transform")]
    [HideInInspector] private RectTransform rectTransform;

    [Header("Storage Values")]
    [HideInInspector] private Transform originalParent;
    [HideInInspector] private int originalSiblingIndex;

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
        layoutElement.ignoreLayout = true;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(canvas.transform);

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
        layoutElement.ignoreLayout = false;

        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
        }

        dragNotify?.OnDragEnd();
    }

    // Helper Functions --------------------------------------------------------
    private void InitializeAwake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        dragNotify = GetComponent<IDragNotify>();
        layoutElement = GetComponent<LayoutElement>();
    }
}
