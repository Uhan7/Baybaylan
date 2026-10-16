using UnityEngine;
using UnityEngine.UI;

public class PopupUI : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private Animator anim;

    [Header("References")]
    [SerializeField] private GameObject dimObj;

    [Header("Input")]
    [SerializeField] private bool toggleWithEscape;

    [Header("Layering")]
    [Tooltip("Render this popup above every other UI canvas. Intended for the pause/settings window.")]
    [SerializeField] private bool alwaysOnTop;

    [SerializeField] private int topSortingOrder = 32000;

    private bool isOpen;
    private Canvas popupCanvas;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        isOpen = anim != null && anim.GetBool("Open");

        if (alwaysOnTop)
            BringToFront();
    }

    private void Update()
    {
        if (toggleWithEscape && Input.GetKeyDown(KeyCode.Escape))
            Popup(!isOpen);
    }

    // Helper Functions --------------------------------------------------------
    public void Popup(bool val)
    {
        if (anim == null) return;

        isOpen = val;

        if (val && alwaysOnTop)
            BringToFront();

        anim.SetBool("Open", val);
        CallDimBackground(val);
    }

    private void BringToFront()
    {
        transform.SetAsLastSibling();

        if (popupCanvas == null)
            popupCanvas = GetComponent<Canvas>();

        if (popupCanvas == null)
            popupCanvas = gameObject.AddComponent<Canvas>();

        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = topSortingOrder;

        // Graphics under a nested override-sorting canvas need their own raycaster.
        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    private void CallDimBackground(bool val)
    {
        if (dimObj == null) return;

        Animator dimAnim = dimObj.GetComponent<Animator>();
        if (dimAnim == null) return;

        if (val) dimAnim.Play("image_fade_in");
        else dimAnim.Play("image_fade_out");
    }
}
