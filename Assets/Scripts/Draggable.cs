using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler
{
    // Variables ---------------------------------------------------------------

    [Header("References")]
    [SerializeField] private Canvas tilesCanvas;

    [Header("Transform")]
    [HideInInspector] private RectTransform rectTransform;

    [Header("Flags")]
    [HideInInspector] private bool isHeld;

    // Main Functions ----------------------------------------------------------

    private void Awake()
    {
        InitializeAwake();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / tilesCanvas.scaleFactor;
    }

    // Helper Functions --------------------------------------------------------

    private void InitializeAwake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
}
