using UnityEngine;

public class PopupUI : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private Animator anim;

    [Header("References")]
    [SerializeField] private GameObject dimObj;

    // Main Functions ----------------------------------------------------------

    // Helper Functions --------------------------------------------------------
    public void Popup(bool val)
    {
        anim.SetBool("Open", val);
        CallDimBackground(val);
    }

    private void CallDimBackground(bool val)
    {
        Animator dimAnim = dimObj.GetComponent<Animator>();

        if (val) dimAnim.Play("image_fade_in");
        else dimAnim.Play("image_fade_out");
    }
}
