using UnityEngine;
using UnityEngine.EventSystems;
using NaughtyAttributes;

public class TileSlot : MonoBehaviour, IDropHandler
{
    // Variables ---------------------------------------------------------------
    [Header("Tile Properties")]
    [HideInInspector] private Tile currentTileScript;
    [ReadOnly, SerializeField] public string currentTileLatinText; // Used in SalitaSlot.cs

    // Main Functions ----------------------------------------------------------
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        if (currentTileScript != null) return; // So you don't place on an occupied tileslot... can swap or whatever soon for more UX

        currentTileScript = eventData.pointerDrag.GetComponent<Tile>();
        if (currentTileScript == null) return;

        ChangeCurrentTile();
        eventData.pointerDrag.GetComponent<RectTransform>().position = GetComponent<RectTransform>().position;
    }

    // Helper Functions --------------------------------------------------------
    private void ChangeCurrentTile()
    {
        if (currentTileScript.currentTileSlot != null) currentTileScript.currentTileSlot.ClearTileSlot();
        currentTileScript.currentTileSlot = this;
    }

    public void ChangeCharacterInSlot() // Called in SalitaSlots.cs
    {
        if (currentTileScript == null)
        {
            currentTileLatinText = "";
            return;
        }

        currentTileLatinText = currentTileScript.latinText;
    }

    public void ClearTileSlot() // Used in Tile.cs... in this script
    {
        currentTileScript = null;
        currentTileLatinText = "";
    }
}