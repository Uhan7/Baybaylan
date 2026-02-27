using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using TMPro;

public class SalitaSlots : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Tiles")]
    [SerializeField] private GameObject tileSlotPrefab;
    [SerializeField] private List<TileSlot> tileSlots;
    [SerializeField] private int tileSlotsAmount = 8;

    [Header("Word Properties")]
    [ReadOnly, SerializeField] private string baybayinSalita;
    [ReadOnly, SerializeField] private string latinSalita;

    [Header("Score Properties")]
    [ReadOnly, SerializeField] private int baseSalitaScore = 0;

    [Header("UI Stuff")]
    [SerializeField] private TextMeshProUGUI salitaText;

    // Main Functions ----------------------------------------------------------
    private void Start()
    {
        GenerateTileSlots();
    }

    private void Update()
    {
        // Temporary Space to "Submit"
        // if (Input.GetKeyDown(KeyCode.Space)) ChangeSalitaText();
    }

    // Helper Functions --------------------------------------------------------
    public void ChangeSalitaText() // Used in Button
    {
        latinSalita = "";
        foreach (TileSlot tileSlot in tileSlots)
        {
            tileSlot.ChangeCharacterInSlot();
            latinSalita += tileSlot.currentTileLatinText;
        }

        salitaText.text = latinSalita;
    }

    private void CheckSalita()
    {
        // Fill Soon
    }

    private void GenerateTileSlots()
    {
        for (int i = 0; i < tileSlotsAmount; i++)
        {
            GameObject slot = Instantiate(tileSlotPrefab, transform);
            tileSlots.Add(slot.GetComponent<TileSlot>());
        }
    }
}
