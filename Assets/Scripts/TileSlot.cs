using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    // Variables ---------------------------------------------------------------

    // Put variables here



    // Main Functions ----------------------------------------------------------

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        eventData.pointerDrag.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position;
    }



    // Helper Functions --------------------------------------------------------

    // Put Helper Functions here
}