using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;

public class SalitaSlots : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Configurations")]
    [SerializeField] private LevelConfig config;

    [Header("Tiles")]
    [SerializeField] private TileSet tileSet;
    [SerializeField] private List<Tile> activeTiles = new List<Tile>();

    [Header("Word Properties")]
    [ReadOnly, SerializeField] private string baybayinSalita;
    [ReadOnly, SerializeField] private string latinSalita;

    [Header("Score Properties")]
    [ReadOnly, SerializeField] private int salitaScore = 0;
    [SerializeField] private float scoreScaleValue = 0.5f;

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI salitaText;

    // Main Functions ----------------------------------------------------------

    // Button Functions
    public void EvaluateSalita()
    {
        UpdateActiveTiles();
        GetSalitaFromTiles();
        UpdateSalitaText();

        if (!IsSalitaValid())
        {
            salitaText.color = Color.red; // Eventually make this play animation
        }
        else
        {
            salitaText.color = Color.green; // Eventually make this play animation
            ScoreSalita();
            ReplaceActiveTiles();
        }
    }

    // Helper Functions --------------------------------------------------------
    private bool IsSalitaValid()
    {
        if (GameManager.Instance.validWords.Contains(latinSalita.ToLower())) return true;
        else return false;
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

        foreach (Tile activeTile in activeTiles) latinSalita += activeTile.latinText;
    }

    private void ScoreSalita()
    {
        salitaScore = 0;

        foreach (Tile activeTile in activeTiles)
        {
            // Some cool effects here
            salitaScore += activeTile.score;
        }

        salitaScore = (int)(salitaScore * activeTiles.Count * scoreScaleValue);

        GameManager.Instance.ChangeMahika(salitaScore);
    }

    private void UpdateSalitaText()
    {
        salitaText.text = latinSalita;
        // Separate function because it may get complicated with i/e and o/u conversion
    }

    private void ReplaceActiveTiles()
    {
        foreach (Tile activeTile in activeTiles) Destroy(activeTile.gameObject);

        tileSet.SpawnRandomTiles(activeTiles.Count);
    }
}
