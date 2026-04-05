using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

[RequireComponent(typeof(Draggable))]
public class Tile : MonoBehaviour
{

    // Setups... put this in an enums manager
    private enum Diacritic
    {
        None,
        Top,
        Bottom,
        Krus
    }

    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private Draggable draggableScript;
    [HideInInspector] private Image imageComponent;

    [Header("Diacritics")]
    [SerializeField] private GameObject topKudlit;
    [SerializeField] private GameObject bottomKudlit;
    [SerializeField] private GameObject krus;

    [Header("Current State")]
    [SerializeField] private Diacritic currentCharmod = Diacritic.None;

    [Header("Audio")]
    [HideInInspector] public AudioSource sfxSource; // To be set by spawners (TileSet.cs)
    [SerializeField] private AudioClip diacriticSFX;

    [Header("Tile Info")]
    [SerializeField] private bool isVowel;
    [HideIf("isVowel"), SerializeField] private string rootConsonant;
    [ShowIf("isVowel"), SerializeField] private string vowel;
    [ReadOnly, SerializeField] public string latinText; // Used in TileSlot.cs

    [Header("Default Visuals")]
    [SerializeField] private Sprite[] availableTileSprites;
    [SerializeField] private Color availableTileColor;
    [SerializeField] private Sprite[] activeTileSprites;
    [SerializeField] private Color activeTileColor;
    [SerializeField] private GameObject[] strokes;

    [Header("Modified Tile Visuals")]
    [SerializeField] private Color availableGoldenStrokeColor;
    [SerializeField] private Color activeGoldenStrokeColor;
    [SerializeField] private GameObject vowelBoostedSymbol;

    [Header("Score Info")]
    [SerializeField] private int baseScore = 10;
    [HideIf("isVowel"), ReadOnly, SerializeField] private int diacriticScore = 0;
    [ReadOnly, SerializeField] private float scoreMultiplier = 1;
    [HideInInspector] public int Score => Mathf.RoundToInt((baseScore + diacriticScore) * scoreMultiplier); // Used in SalitaSlots.cs

    [Header("Other Tile Info")]
    [SerializeField] private int chance = 5;

    [Header("Flags")]
    [HideInInspector] private bool wasBeingDragged;
    [HideInInspector] private bool isGold; //Set this to be more scaleable soon... have a TileModifier script maybe

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        draggableScript = GetComponent<Draggable>();
        imageComponent = GetComponent<Image>();
    }

    private void Start()
    {
        // Tiles always start as inactive/available
        ChangeSprite(false);

        currentCharmod = Diacritic.None;
        ToggleCharmodObject();
        if (isVowel)
        {
            latinText = vowel;
            diacriticScore = 0;
        }
        else latinText = rootConsonant + "a";

        ApplyGoldChance();
        if (isVowel) ApplyVowelBoost();
    }

    private void Update()
    {
        ChangeSpriteOnDrag();
    }

    // Helper Functions --------------------------------------------------------
    public void ToggleNextModification() // Called by Button | PLEASE CHANGE NAME TO BE "DIACRITIC"
    {
        if (isVowel) return; // Skip if vowel
        if (draggableScript.isBeingDragged) return;

        if (currentCharmod == Diacritic.Krus) currentCharmod = Diacritic.None;
        else currentCharmod++;

        sfxSource.PlayOneShot(diacriticSFX);
        ToggleCharmodObject();
    }

    private void ToggleCharmodObject()
    {
        if (isVowel) Debug.LogWarning("ToggleCharmodObject called on a vowel.");

        ClearAllCharmods();

        switch (currentCharmod)
        {
            case Diacritic.None:
                diacriticScore = 0;
                latinText = rootConsonant + "a";
                break;

            case Diacritic.Top:
                diacriticScore = 5;
                topKudlit.SetActive(true);
                latinText = rootConsonant + "i";
                break;

            case Diacritic.Bottom:
                diacriticScore = 10;
                bottomKudlit.SetActive(true);
                latinText = rootConsonant + "u";
                break;

            case Diacritic.Krus:
                diacriticScore = 8;
                krus.SetActive(true);
                latinText = rootConsonant;
                break;

            default:
                Debug.LogError("ActivateModification p_currentModification incorrect enum.");
                break;
        }
    }

    private void ClearAllCharmods()
    {
        topKudlit.SetActive(false);
        bottomKudlit.SetActive(false);
        krus.SetActive(false);
    }

    public int GetChance()
    {
        return chance;
    }

    public void ChangeSprite(bool active) // Can be called by the DropZone obj
    {
        if (active)
        {
            imageComponent.sprite = activeTileSprites[Random.Range(0, activeTileSprites.Length)];
            foreach (GameObject stroke in strokes) stroke.GetComponent<Image>().color = activeTileColor;
            if (isGold) foreach (GameObject stroke in strokes) stroke.GetComponent<Image>().color = activeGoldenStrokeColor;
        }
        else
        {
            imageComponent.sprite = availableTileSprites[Random.Range(0, availableTileSprites.Length)];
            foreach (GameObject stroke in strokes) stroke.GetComponent<Image>().color = availableTileColor;
            if (isGold) foreach (GameObject stroke in strokes) stroke.GetComponent<Image>().color = availableGoldenStrokeColor;
        }
    }

    private void ChangeSpriteOnDrag()
    {
        if (draggableScript.isBeingDragged && !wasBeingDragged) ChangeSprite(false);

        wasBeingDragged = draggableScript.isBeingDragged;
    }

    // Tile Modifications ------------------------------------------------------
    private void ApplyGoldChance()
    {
        float chance = AlahasManager.Instance.goldenTileChance;

        if (Random.value <= chance)
        {
            isGold = true;
            scoreMultiplier += AlahasManager.Instance.goldenTileMultiplier;

            foreach (GameObject stroke in strokes) stroke.GetComponent<Image>().color = availableGoldenStrokeColor;
        }
    }

    private void ApplyVowelBoost()
    {
        if (!AlahasManager.Instance.boostVowels) return;

        scoreMultiplier += AlahasManager.Instance.vowelMultiplier;
        vowelBoostedSymbol.SetActive(true);
    }
}
