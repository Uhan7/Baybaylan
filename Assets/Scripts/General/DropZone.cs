using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    // Variables ---------------------------------------------------------------
    [SerializeField] private bool allowInsertion;

    // Main Functions ----------------------------------------------------------
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag.gameObject;
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

        draggedObject.transform.SetParent(transform);
        if (allowInsertion) draggedObject.transform.SetSiblingIndex(newIndex);
    }
}
