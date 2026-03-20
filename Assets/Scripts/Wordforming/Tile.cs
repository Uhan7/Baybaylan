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

    [Header("Tile Visuals")]
    [SerializeField] private Sprite[] availableTileSprites;
    [SerializeField] private Color availableTileColor;
    [SerializeField] private Sprite[] activeTileSprites;
    [SerializeField] private Color activeTileColor;
    [SerializeField] private GameObject[] strokes;

    [Header("Score Info")]
    [SerializeField] private int baseScore = 10;
    [HideIf("isVowel"), ReadOnly, SerializeField] private int diacriticScore = 3;
    [HideInInspector] public int Score => baseScore + diacriticScore; // Used in SalitaSlots.cs

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
    }

    // Helper Functions --------------------------------------------------------
    public void ToggleNextModification() // Called by Button
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
                diacriticScore = 3;
                latinText = rootConsonant + "a";
                break;

            case Diacritic.Top:
                diacriticScore = 8;
                topKudlit.SetActive(true);
                latinText = rootConsonant + "i";
                break;

            case Diacritic.Bottom:
                diacriticScore = 10;
                bottomKudlit.SetActive(true);
                latinText = rootConsonant + "u";
                break;

            case Diacritic.Krus:
                diacriticScore = 0;
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

    public void ChangeSprite(bool active) // Can be called by the DropZone obj
    {
        if (active)
        {
            imageComponent.sprite = activeTileSprites[Random.Range(0, activeTileSprites.Length - 1)];
            foreach (GameObject stroke in strokes) stroke.GetComponent<Image>().color = activeTileColor;
        }
        else
        {
            imageComponent.sprite = availableTileSprites[Random.Range(0, availableTileSprites.Length - 1)];
            foreach (GameObject stroke in strokes) stroke.GetComponent<Image>().color = availableTileColor;
        }
    }
}
