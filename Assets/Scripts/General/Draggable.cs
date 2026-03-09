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
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // Variables ---------------------------------------------------------------
    [Header("Events")]
    [HideInInspector] public static bool anyBeingDragged = false;

    [Header("Components")]
    [HideInInspector] private RectTransform rectTransform;
    [HideInInspector] private CanvasGroup canvasGroup;
    [HideInInspector] private LayoutElement layoutElement;
    [HideInInspector] private Animator anim;

    [Header("References")]
    [SerializeField] public Canvas canvas; // To be set by spawners (TileSet.cs)

    [Header("Audio")]
    [HideInInspector] public AudioSource sfxSource; // To be set by spawners (TileSet.cs)
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip dragSFX;

    [Header("Storage Values")]
    [HideInInspector] private Transform originalParent;
    [HideInInspector] private int originalSiblingIndex;

    [Header("Flags")]
    [HideInInspector] private IDragNotify dragNotify;
    [HideInInspector] public bool isBeingDragged; // For IDragNotify

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layoutElement = GetComponent<LayoutElement>();
        anim = GetComponent<Animator>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        anyBeingDragged = true;
        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
        layoutElement.ignoreLayout = true;

        anim.SetBool("Hover", false);
        anim.SetBool("Hold", false);

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(canvas.transform);

        sfxSource.PlayOneShot(dragSFX);

        dragNotify?.OnDragBegin();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        anyBeingDragged = false;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (anyBeingDragged) return;

        sfxSource.PlayOneShot(hoverSFX);
        anim.SetBool("Hover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.SetBool("Hover", false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (anyBeingDragged) return;

        anim.SetBool("Hold", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        anim.SetBool("Hold", false);
    }

    // Helper Functions --------------------------------------------------------
}
