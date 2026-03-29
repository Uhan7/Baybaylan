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
        shouldOpen = isHovering || DialogueManager.Instance.dialoguing;
        anim.SetBool("isOpen", shouldOpen);
    }

    public bool GetShouldOpen()
    {
        return shouldOpen;
    }

    private void CallDimBackground()
    {
        Animator dimAnim = dimObj.GetComponent<Animator>();

        if (isHovering) dimAnim.Play("image_fade_in");
        else dimAnim.Play("image_fade_out");
    }
}
