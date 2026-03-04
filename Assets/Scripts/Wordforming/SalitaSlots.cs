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
    [ReadOnly, SerializeField] private int salitaScore = 0;

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI salitaText;

    // Main Functions ----------------------------------------------------------

    // Button Functions
    public void EvaluateSalita()
    {
        UpdateActiveTiles();
        GetSalitaFromTiles();

        if (!IsSalitaValid())
        {
            // Do things if invalid Salita
            // Right now, this won't do anything because IsSalitaValid always returns true
        }
        else
        {
            UpdateActiveTiles();
            ScoreSalita();
            UpdateSalitaText();
        }
    }

    // Helper Functions --------------------------------------------------------
    private bool IsSalitaValid()
    {
        // Check if salita exists in the wordlist
        // If exists, proceed
        // If doesn't exist

        // For now, assume all salita is valid
        return true;
    }

    private void UpdateActiveTiles()
    {
        activeTiles.Clear();

        foreach (Transform child in transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null) activeTiles.Add(tile);
        }
    }

    private void GetSalitaFromTiles()
    {
        latinSalita = "";
        baybayinSalita = ""; // Eventually get the baybayin as well

        foreach (Tile activeTile in activeTiles)
        {
            latinSalita += activeTile.latinText;
        }
    }

    private void ScoreSalita()
    {
        salitaScore = 0;

        foreach (Tile activeTile in activeTiles)
        {
            // Some cool effects here
            salitaScore += activeTile.score;
        }

        salitaScore *= activeTiles.Count;
    }

    private void UpdateSalitaText()
    {
        salitaText.text = latinSalita;
        // Separate function because it may get complicated with i/e and o/u conversion
    }
}
