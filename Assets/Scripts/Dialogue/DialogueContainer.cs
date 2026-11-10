using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueContainer : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private Animator anim;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject nextIndicator;
    [SerializeField] private Button skipButton;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (skipButton != null) skipButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateAnimations();
    }

    // Helper Functions --------------------------------------------------------
    private void UpdateAnimations()
    {
        bool dialogueOpen = DialogueManager.Instance != null && DialogueManager.Instance.dialoguing;
        if (anim != null) anim.SetBool("dialogueOpen", dialogueOpen);
        if (skipButton != null && skipButton.gameObject.activeSelf != dialogueOpen)
            skipButton.gameObject.SetActive(dialogueOpen);
    }

    public void SkipDialogue()
    {
        if (DialogueManager.Instance != null) DialogueManager.Instance.SkipDialogue();
    }

    public bool IsPointerOverSkipButton()
    {
        if (skipButton == null || !skipButton.isActiveAndEnabled) return false;
        Canvas canvas = skipButton.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(
            skipButton.transform as RectTransform, Input.mousePosition, camera);
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
