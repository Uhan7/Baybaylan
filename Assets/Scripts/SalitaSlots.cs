using UnityEngine;
using NaughtyAttributes;
using TMPro;

public class SalitaSlots : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Tiles")]
    [SerializeField] private TileSlot[] tileSlots;

    [Header("Word Properties")]
    [ReadOnly, SerializeField] private string baybayinSalita;
    [ReadOnly, SerializeField] private string latinSalita;

    [Header("Score Properties")]
    [ReadOnly, SerializeField] private int baseSalitaScore = 0;

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI salitaText;

    // Main Functions ----------------------------------------------------------

    private void Update()
    {
        // Temporary Space to "Submit"
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeSalitaText();
        }
    }

    // Helper Functions --------------------------------------------------------
    private void ChangeSalitaText ()
    {
        latinSalita = "";
        foreach (TileSlot tileSlot in tileSlots)
        {
            tileSlot.ChangeCharacterInSlot();
            latinSalita += tileSlot.currentTileLatinText;
        }

        salitaText.text = latinSalita;
    }

    // Put Helper Functions here
}
