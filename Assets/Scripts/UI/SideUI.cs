using UnityEngine;

public class SideUI : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private Animator anim;

    [Header("References")]
    [SerializeField] private GameObject dimObj;

    [Header("Flags")]
    [HideInInspector] private bool isHovering;
    [HideInInspector] private bool shouldOpen;
    [HideInInspector] private bool forceOpen;

    // Main Functions ----------------------------------------------------------
    private void Update()
    {
        UpdateAnimator();
    }

    // Event Functions ---------------------------------------------------------
    public void ToggleFocus(bool val)
    {
        isHovering = val;

        CallDimBackground();
    }

    // Helper Functions --------------------------------------------------------
    private void UpdateAnimator()
    {
        shouldOpen = forceOpen || isHovering ||
            (DialogueManager.Instance != null && DialogueManager.Instance.dialoguing);
        anim.SetBool("isOpen", shouldOpen);
    }

    public void SetForcedOpen(bool value)
    {
        forceOpen = value;
        UpdateAnimator();
    }

    public bool GetShouldOpen()
    {
        return shouldOpen;
    }

    private void CallDimBackground()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.dialoguing) return;

        Animator dimAnim = dimObj.GetComponent<Animator>();

        if (isHovering) dimAnim.Play("image_fade_in");
        else dimAnim.Play("image_fade_out");
    }
}
