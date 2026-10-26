using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PopupUI : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [SerializeField] private Animator anim;

    [Header("References")]
    [SerializeField] private GameObject dimObj;
    [SerializeField] private Button returnToTitleButton;

    [Header("Input")]
    [SerializeField] private bool toggleWithEscape;

    [Header("Layering")]
    [Tooltip("Render this popup above every other UI canvas. Intended for the pause/settings window.")]
    [SerializeField] private bool alwaysOnTop;

    [SerializeField] private int topSortingOrder = 32000;

    private bool isOpen;
    private Canvas popupCanvas;
    private GameObject settingsOverlay;
    private static int lastSettingsInteractionFrame = -1;

    // Dialogue uses raw input, which bypasses UI raycast blockers. Check the
    // actual pointer target too so opening settings is safe in either Update order.
    public static bool BlocksDialogueInput()
    {
        if (Time.frameCount <= lastSettingsInteractionFrame + 1) return true;
        foreach (PopupUI popup in FindObjectsByType<PopupUI>(FindObjectsSortMode.None))
            if (popup.alwaysOnTop && popup.isOpen) return true;

        if (EventSystem.current == null) return false;
        var hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        }, hits);
        if (hits.Count == 0) return false;
        PopupUI owner = hits[0].gameObject.GetComponentInParent<PopupUI>();
        if (owner != null && owner.alwaysOnTop) return true;
        Button button = hits[0].gameObject.GetComponentInParent<Button>();
        if (button == null) return false;
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            if (button.onClick.GetPersistentTarget(i) is PopupUI target && target.alwaysOnTop)
                return true;
        return false;
    }

    public static void HideSettingsForSceneChange()
    {
        foreach (PopupUI popup in FindObjectsByType<PopupUI>(FindObjectsSortMode.None))
        {
            if (!popup.alwaysOnTop) continue;
            popup.Popup(false);
        }
    }

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        isOpen = anim != null && anim.GetBool("Open");

        if (returnToTitleButton != null && gameObject.scene.name == "Title Screen")
            returnToTitleButton.gameObject.SetActive(false);

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

        if (alwaysOnTop)
        {
            lastSettingsInteractionFrame = Time.frameCount;
            EnsureSettingsOverlay();
            settingsOverlay.SetActive(val);
        }

        if (val && alwaysOnTop)
            BringToFront();

        anim.SetBool("Open", val);
        CallDimBackground(val);
    }

    private void EnsureSettingsOverlay()
    {
        if (settingsOverlay != null) return;
        settingsOverlay = new GameObject("Settings Dimmer", typeof(RectTransform),
            typeof(Canvas), typeof(GraphicRaycaster), typeof(Image), typeof(Button));
        settingsOverlay.layer = gameObject.layer;
        RectTransform rect = settingsOverlay.GetComponent<RectTransform>();
        rect.SetParent(transform.parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        Canvas canvas = settingsOverlay.GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = topSortingOrder - 1;
        Image image = settingsOverlay.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.5f);
        Button button = settingsOverlay.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(() => Popup(false));
    }

    private void OnDisable()
    {
        if (!alwaysOnTop) return;
        isOpen = false;
        lastSettingsInteractionFrame = Time.frameCount;
        if (settingsOverlay != null) settingsOverlay.SetActive(false);
    }

    private void OnDestroy()
    {
        if (settingsOverlay != null) Destroy(settingsOverlay);
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
