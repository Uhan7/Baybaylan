using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class DialogueContainer : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private Animator anim;

    [Header("References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject nextIndicator;
    [SerializeField] private Button skipButton;

    private static readonly Regex ShakeTag = new Regex("<shake>", RegexOptions.IgnoreCase);
    private readonly List<int> shakePositions = new List<int>();
    private int nextShakeIndex;

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
        shakePositions.Clear();
        nextShakeIndex = 0;
        dialogueText.text = "";
        nextIndicator.SetActive(false);
    }

    public int PrepareText(string text)
    {
        shakePositions.Clear();
        nextShakeIndex = 0;
        int removedLength = 0;
        string displayText = ShakeTag.Replace(text ?? "", match =>
        {
            shakePositions.Add(match.Index - removedLength);
            removedLength += match.Length;
            return "";
        });

        // Lay out the complete sentence before hiding it so word wrapping stays fixed
        // while maxVisibleCharacters reveals the already-positioned glyphs.
        dialogueText.maxVisibleCharacters = int.MaxValue;
        dialogueText.text = displayText;
        dialogueText.ForceMeshUpdate();

        int characterCount = dialogueText.textInfo.characterCount;
        // TMP indices account for rich-text tags, sprites, and Unicode characters.
        // Convert each cue's source position into a typewriter reveal position.
        int characterIndex = 0;
        for (int i = 0; i < shakePositions.Count; i++)
        {
            while (characterIndex < characterCount &&
                   dialogueText.textInfo.characterInfo[characterIndex].index < shakePositions[i])
                characterIndex++;

            shakePositions[i] = characterIndex;
        }
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
        TriggerShakeCues(count);
    }

    public void ShowFullText()
    {
        dialogueText.maxVisibleCharacters = int.MaxValue;
        TriggerShakeCues(int.MaxValue);
    }

    private void TriggerShakeCues(int visibleCharacters)
    {
        bool shouldShake = false;
        while (nextShakeIndex < shakePositions.Count &&
               shakePositions[nextShakeIndex] <= visibleCharacters)
        {
            nextShakeIndex++;
            shouldShake = true;
        }

        // Revealing the whole sentence at once combines remaining cues into one shake.
        if (shouldShake) ScreenShake.ShakeGlobal();
    }

    public void ShowNextIndicator(bool value)
    {
        nextIndicator.SetActive(value);
    }
}
