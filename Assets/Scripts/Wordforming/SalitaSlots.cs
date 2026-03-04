using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;

public class SalitaSlots : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Tiles")]
    [SerializeField] private List<Tile> activeTiles = new List<Tile>();

    [Header("Word Properties")]
    [ReadOnly, SerializeField] private string baybayinSalita;
    [ReadOnly, SerializeField] private string latinSalita;

    [Header("Score Properties")]
    [ReadOnly, SerializeField] private int baseSalitaScore = 0;

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI salitaText;

    // Main Functions ----------------------------------------------------------

    // Helper Functions --------------------------------------------------------
    public void ChangeSalitaText() // Used in Button
    {
        latinSalita = "";
        foreach (Transform child in transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null) latinSalita += tile.latinText;
        }

        salitaText.text = latinSalita;
    }

    private void CheckSalita()
    {
        // Fill Soon
    }
}
