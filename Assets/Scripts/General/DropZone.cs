using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    // Variables ---------------------------------------------------------------

    // Main Functions ----------------------------------------------------------
    public void OnDrop(PointerEventData eventData)
    {
        eventData.pointerDrag.transform.SetParent(gameObject.transform);
    }

    // Helper Functions --------------------------------------------------------
}
