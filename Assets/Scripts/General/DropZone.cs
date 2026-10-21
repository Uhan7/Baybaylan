using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DropZone : MonoBehaviour, IDropHandler
{
    // Variables ---------------------------------------------------------------
    [Header("Modifications")]
    [SerializeField] private bool allowInsertion;
    [SerializeField] private bool changeTileBox;
    [SerializeField] private bool isActiveDropZone;

    [Header("Events")]
    [SerializeField] private bool invokeOnlyOnce;
    [SerializeField] private UnityEvent eventOnDrop;

    [HideInInspector] private bool hasInvokedDropEvent;

    // Main Functions ----------------------------------------------------------
    public void OnDrop(PointerEventData eventData)
    {
        if (Draggable.InteractionLocked) return;
        if (eventData.pointerDrag == null) return;

        GameObject draggedObject = eventData.pointerDrag.gameObject;
        Draggable draggable = draggedObject.GetComponent<Draggable>();
        if (draggable == null) return;
        Tile tile = draggedObject.GetComponent<Tile>();

        RectTransform rectTransform = transform as RectTransform;
        Vector2 localMousePosition;
        int newIndex = 0;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localMousePosition);

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform child = transform.GetChild(i) as RectTransform;
            if (child.gameObject == draggedObject) continue;

            if (localMousePosition.x > child.localPosition.x) newIndex = i + 1;
        }

        if (allowInsertion)
            newIndex = draggable.GetPreviewSiblingIndex(transform, newIndex);

        draggedObject.transform.SetParent(transform);
        draggable.RegisterSmoothLayoutStart(transform);
        if (allowInsertion) draggedObject.transform.SetSiblingIndex(newIndex);

        if (changeTileBox && tile != null) tile.ChangeSprite(isActiveDropZone);

        tile?.PlayPlacementSFX(isActiveDropZone);
        draggable.PlayDropParticles();

        if (!invokeOnlyOnce || !hasInvokedDropEvent)
        {
            eventOnDrop?.Invoke();
            hasInvokedDropEvent = true;
        }
    }

    public bool PreviewInsertion(Draggable draggable, PointerEventData eventData)
    {
        if (!allowInsertion || draggable == null) return false;

        RectTransform placeholder = draggable.GetDragPlaceholder();
        RectTransform rectTransform = transform as RectTransform;
        if (placeholder == null || rectTransform == null) return false;

        placeholder.SetParent(transform, false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localMousePosition);

        int insertionIndex = 0;
        foreach (Transform child in transform)
        {
            if (child == placeholder) continue;

            RectTransform childRect = child as RectTransform;
            if (childRect != null && localMousePosition.x > childRect.localPosition.x)
                insertionIndex++;
        }

        placeholder.SetSiblingIndex(insertionIndex);
        LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        return true;
    }
}
