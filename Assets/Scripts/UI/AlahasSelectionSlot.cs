using UnityEngine;
using UnityEngine.EventSystems;

class AlahasSelectionSlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    private AlahasSelectionController controller;
    private AlahasInfoPopup popup;
    private int slotIndex;

    internal void Configure(AlahasSelectionController owner, AlahasInfoPopup slotPopup, int index)
    {
        controller = owner;
        popup = slotPopup;
        slotIndex = index;
        popup.SetSelectionMode(true);
    }

    public void EndSelection()
    {
        if (popup != null) popup.SetSelectionMode(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (controller == null || eventData.pointerDrag == null) return;

        AlahasInventoryItem item = eventData.pointerDrag.GetComponent<AlahasInventoryItem>();
        if (item != null && item.IsUnlocked)
            controller.TryEquip(item.Alahas, slotIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (controller != null)
            controller.RemoveAt(slotIndex);
    }
}
