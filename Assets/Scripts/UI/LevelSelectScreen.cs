using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteAlways]
public class LevelSelectScreen : MonoBehaviour
{
    [SerializeField] private LevelConfig config;
    [SerializeField] private SaveManager.SaveDataNames levelId;
    // Retained for older scene instances that have not been assigned a config.
    [SerializeField, HideInInspector] private string sceneNameToEnter;
    [SerializeField] private GameObject lockVisual;
    [SerializeField] private Button enterButton;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text protagonistName;
    [SerializeField] private TMP_Text targetName;
    [SerializeField] private Image protagonistPortrait;
    [SerializeField] private Image targetPortrait;
    [SerializeField] private Image background;
    [SerializeField] private RectTransform content;
    [SerializeField, Range(0f, 1f)] private float unlockedBackgroundValue = 0.75f;
    [SerializeField, Range(0f, 1f)] private float lockedBackgroundValue = 0.25f;
    [SerializeField, Range(0f, 1f)] private float lockedCharacterValue = 0.1f;

    private bool isLocked;
    public LevelConfig Config => config;
    public string SceneName => config != null ? config.sceneName : sceneNameToEnter;

    private void OnEnable()
    {
        if (Application.isPlaying && enterButton != null)
            enterButton.onClick.AddListener(EnterLevel);
        RefreshPresentation();
    }

    private void Start() { RefreshPresentation(); }

    private void OnDisable()
    {
        if (enterButton != null) enterButton.onClick.RemoveListener(EnterLevel);
    }

    private void Update()
    {
        // ExecuteAlways also previews config changes in the scene editor.
        RefreshPresentation();
    }

    public void RefreshPresentation()
    {
        isLocked = Application.isPlaying && levelId != SaveManager.SaveDataNames.NoneOrNull &&
            (SaveManager.Instance == null || SaveManager.Instance.getSaveData<int>(levelId) == 0);

        if (config != null)
        {
            if (title != null)
            {
                title.text = config.levelName;
                title.color = Color.white;
            }
            SetPortrait(protagonistPortrait, protagonistName, config.protagonistPortrait, config.protagonistName);
            SetPortrait(targetPortrait, targetName, config.targetPortrait, config.targetName);
            if (background != null) background.sprite = config.selectionBackground;
        }

        if (lockVisual != null) lockVisual.SetActive(isLocked);
        if (enterButton != null) enterButton.interactable = !isLocked && !string.IsNullOrWhiteSpace(SceneName);
        if (background != null) background.color = Gray(isLocked ? lockedBackgroundValue : unlockedBackgroundValue);
        if (protagonistPortrait != null) protagonistPortrait.color = Gray(isLocked ? lockedCharacterValue : 1f);
        if (targetPortrait != null) targetPortrait.color = Gray(isLocked ? lockedCharacterValue : 1f);
        FitLayout();
    }

    private static Color Gray(float value) { return new Color(value, value, value, 1f); }

    private static void SetPortrait(Image portrait, TMP_Text label, Sprite sprite, string displayName)
    {
        if (portrait != null)
        {
            portrait.sprite = sprite;
            portrait.enabled = sprite != null;
            // An empty portrait hides the complete slot, including its name plate.
            if (portrait.transform.parent != null)
                portrait.transform.parent.gameObject.SetActive(sprite != null);
        }
        if (label != null) label.text = displayName;
    }

    private void FitLayout()
    {
        Rect rect = ((RectTransform)transform).rect;
        if (content != null)
            content.localScale = Vector3.one * Mathf.Min(rect.width / 1920f, rect.height / 1080f);
        if (background == null) return;

        // Stretch uses zero offsets, not a fixed image size. A fixed sizeDelta
        // gets added to the parent's size when stretch anchors are selected.
        RectTransform backgroundRect = background.rectTransform;
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.pivot = Vector2.one * 0.5f;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        background.preserveAspect = false;
        bool flip = config != null && config.flipSelectionBackground;
        backgroundRect.localScale = new Vector3(flip ? -1f : 1f, 1f, 1f);
    }

    private void EnterLevel()
    {
        RefreshPresentation();
        if (!isLocked && !string.IsNullOrWhiteSpace(SceneName)) SceneManager.LoadScene(SceneName);
    }
}
