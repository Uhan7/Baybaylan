using UnityEngine;
using NaughtyAttributes;

public class Tile : MonoBehaviour, IDragNotify
{

    // Setups ------------------------------------------------------------------
    private enum CharacterModification
    {
        None,
        Top,
        Bottom,
        Krus
    }

    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private Draggable draggableScript;

    [Header("References")]
    [HideInInspector] public TileSlot currentTileSlot; // Used in TileSlot.cs

    [Header("Charmods")]
    [SerializeField] private GameObject topKudlit;
    [SerializeField] private GameObject bottomKudlit;
    [SerializeField] private GameObject krus;

    [Header("Current State")]
    [SerializeField] private CharacterModification currentCharmod;

    [Header("Tile Info")]
    [SerializeField] private bool isVowel;
    [HideIf("isVowel"), SerializeField] private string rootConsonant;
    [ShowIf("isVowel"), SerializeField] private string vowel;
    [SerializeField] private int baseTileScore = 10;
    [ReadOnly, SerializeField] public string latinText; // Used in TileSlot.cs

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        InitializeAwake();
    }

    private void Start()
    {
        InitializeStart();
    }

    public void OnDragBegin()
    {
        if (currentTileSlot != null)
        {
            currentTileSlot.ClearTileSlot();
            currentTileSlot = null;
        }
    }

    public void OnDragEnd() { }

    // Helper Functions --------------------------------------------------------
    public void ToggleNextModification() // Called by Button
    {
        if (isVowel) return; // Skip if vowel
        if (draggableScript.isBeingDragged) return;

        if (currentCharmod == CharacterModification.Krus) currentCharmod = CharacterModification.None;
        else currentCharmod++;

        ToggleCharmodObject();
    }

    private void ToggleCharmodObject()
    {
        if (isVowel) Debug.LogWarning("ToggleCharmodObject called on a vowel.");

        ClearAllCharmods();

        switch (currentCharmod)
        {
            case CharacterModification.None:
                latinText = rootConsonant + "a";
                break;

            case CharacterModification.Top:
                topKudlit.SetActive(true);
                latinText = rootConsonant + "i";
                break;

            case CharacterModification.Bottom:
                bottomKudlit.SetActive(true);
                latinText = rootConsonant + "u";
                break;

            case CharacterModification.Krus:
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

    private void InitializeAwake()
    {
        draggableScript = GetComponent<Draggable>();
    }

    private void InitializeStart()
    {
        currentCharmod = CharacterModification.None;
        ToggleCharmodObject();
        if (isVowel) latinText = vowel;
        else latinText = rootConsonant + "a";
    }
}
