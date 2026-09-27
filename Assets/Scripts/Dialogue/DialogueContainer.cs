using UnityEngine;
using TMPro;

public class DialogueContainer : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private Animator anim;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject nextIndicator;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateAnimations();
    }

    // Helper Functions --------------------------------------------------------
    private void UpdateAnimations()
    {
        if (anim != null) anim.SetBool("dialogueOpen", DialogueManager.Instance.dialoguing);
    }

    public void ClearText()
    {
        dialogueText.text = "";
        nextIndicator.SetActive(false);
    }

    public int PrepareText(string text)
    {
        // Lay out the complete sentence before hiding it so word wrapping stays fixed
        // while maxVisibleCharacters reveals the already-positioned glyphs.
        dialogueText.maxVisibleCharacters = int.MaxValue;
        dialogueText.text = text;
        dialogueText.ForceMeshUpdate();

        int characterCount = dialogueText.textInfo.characterCount;
        dialogueText.maxVisibleCharacters = 0;
        nextIndicator.SetActive(false);

        return characterCount;
    }

    public char GetVisibleCharacter(int index)
    {
        return dialogueText.textInfo.characterInfo[index].character;
    }

    public void SetVisibleCharacters(int count)
    {
        dialogueText.maxVisibleCharacters = count;
    }

    public void ShowFullText()
    {
        dialogueText.maxVisibleCharacters = int.MaxValue;
    }

    public void ShowNextIndicator(bool value)
    {
        nextIndicator.SetActive(value);
    }
}
