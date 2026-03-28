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
        anim.SetBool("dialogueOpen", DialogueManager.Instance.dialoguing);
    }

    public void ClearText()
    {
        dialogueText.text = "";
        nextIndicator.SetActive(false);
    }

    public void SetTextInstant(string text)
    {
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;
        nextIndicator.SetActive(false);
    }

    public void SetVisibleCharacters(int count)
    {
        dialogueText.maxVisibleCharacters = count;
    }

    public void ShowFullText()
    {
        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
    }

    public void ShowNextIndicator(bool value)
    {
        nextIndicator.SetActive(value);
    }
}