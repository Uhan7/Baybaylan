using UnityEngine;
using NaughtyAttributes;

public class Tile : MonoBehaviour
{

    // Setups

    private enum CharacterModification
    {
        None,
        Top,
        Bottom,
        Krus
    }



    // Variables ---------------------------------------------------------------

    [Header("Other Components")]
    [SerializeField] private Draggable draggableScript;

    [Header("References to Modifiers")]
    [SerializeField] private GameObject topKudlit;
    [SerializeField] private GameObject bottomKudlit;
    [SerializeField] private GameObject krus;

    [Header("Current State")]
    [SerializeField] private CharacterModification currentCharmod;

    [Header("Tile Info")]
    [SerializeField] private bool isVowel;
    [SerializeField] private int baseTileScore = 10;
    [HideIf("isVowel"), SerializeField] private string rootConsonant;
    [SerializeField] private string latinText;



    // Main Functions ----------------------------------------------------------

    private void Awake()
    {
        InitializeAwake();
    }

    private void Start()
    {
        InitializeStart();
    }



    // Helper Functions --------------------------------------------------------

    public void ToggleNextModification() // Called by Button
    {
        if (isVowel) return;
        if (draggableScript.isBeingDragged) return;

        if (currentCharmod == CharacterModification.Krus) currentCharmod = CharacterModification.None;
        else currentCharmod++;

        ToggleCharmodObject();
    }

    private void ToggleCharmodObject()
    {
        ClearAllCharmods();

        switch (currentCharmod)
        {
            case CharacterModification.None:
                break;

            case CharacterModification.Top:
                topKudlit.SetActive(true);
                break;

            case CharacterModification.Bottom:
                bottomKudlit.SetActive(true);
                break;

            case CharacterModification.Krus:
                krus.SetActive(true);
                break;

            default:
                Debug.LogError("Error: ActivateModification p_currentModification exceeded enum limit.");
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
        latinText = rootConsonant + "a";
    }
}
