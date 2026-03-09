using UnityEngine;

public class SideUI : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private Animator anim;

    [Header("References")]
    [SerializeField] private GameObject dimObj;

    [Header("Flags")]
    [HideInInspector] private bool isOpen;

    // Main Functions ----------------------------------------------------------

    // Event Functions ---------------------------------------------------------
    public void ToggleFocus(bool val)
    {
        isOpen = val;

        CallDimBackground();
        UpdateAnimator();
    }

    // Helper Functions --------------------------------------------------------
    private void UpdateAnimator()
    {
        anim.SetBool("isOpen", isOpen);
    }

    private void CallDimBackground()
    {
        Animator dimAnim = dimObj.GetComponent<Animator>();

        if (isOpen == true) dimAnim.Play("image_fade_in");
        else dimAnim.Play("image_fade_out");
    }
}
