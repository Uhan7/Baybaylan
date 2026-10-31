using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ConfirmationPromptUI : MonoBehaviour
{
    private TextMeshProUGUI messageText;
    private TextMeshProUGUI backButtonText;
    private TextMeshProUGUI continueButtonText;
    private Button backButton;
    private Button continueButton;
    private Action backAction;
    private Action continueAction;

    public static ConfirmationPromptUI Create()
    {
        GameObject root = new GameObject(
            "Confirmation Prompt",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(Image),
            typeof(ConfirmationPromptUI));

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 32000;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image dimmer = root.GetComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.72f);
        dimmer.raycastTarget = true;

        ConfirmationPromptUI prompt = root.GetComponent<ConfirmationPromptUI>();
        prompt.BuildVisuals();
        root.SetActive(false);
        return prompt;
    }

    public void Show(
        string message,
        string backLabel,
        string continueLabel,
        Action onBack,
        Action onContinue)
    {
        backAction = onBack;
        continueAction = onContinue;
        messageText.text = message;
        backButtonText.text = backLabel;
        continueButtonText.text = continueLabel;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    public void Hide()
    {
        backAction = null;
        continueAction = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) InvokeBack();
    }

    private void BuildVisuals()
    {
        RectTransform panel = CreateImage(
            transform,
            "Prompt Panel",
            new Color(0.035f, 0.055f, 0.075f, 0.98f));
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(820f, 380f);

        messageText = CreateText(panel, "Message", 38f);
        RectTransform messageRect = messageText.rectTransform;
        messageRect.anchorMin = new Vector2(0.08f, 0.38f);
        messageRect.anchorMax = new Vector2(0.92f, 0.88f);
        messageRect.offsetMin = messageRect.offsetMax = Vector2.zero;

        backButton = CreateButton(
            panel,
            "Go Back Button",
            new Color(0.22f, 0.25f, 0.29f, 1f),
            out backButtonText);
        PositionButton(backButton, new Vector2(-175f, -115f));
        backButton.onClick.AddListener(InvokeBack);

        continueButton = CreateButton(
            panel,
            "Continue Button",
            new Color(0.72f, 0.48f, 0.12f, 1f),
            out continueButtonText);
        PositionButton(continueButton, new Vector2(175f, -115f));
        continueButton.onClick.AddListener(InvokeContinue);
    }

    private void InvokeBack()
    {
        Action callback = backAction;
        Hide();
        callback?.Invoke();
    }

    private void InvokeContinue()
    {
        Action callback = continueAction;
        Hide();
        callback?.Invoke();
    }

    private static RectTransform CreateImage(
        Transform parent,
        string objectName,
        Color color)
    {
        GameObject child = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image image = child.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = true;
        return rect;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string objectName,
        float fontSize)
    {
        GameObject child = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.SetParent(parent, false);

        TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = 20f;
        text.fontSizeMax = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(
        Transform parent,
        string objectName,
        Color color,
        out TextMeshProUGUI label)
    {
        RectTransform rect = CreateImage(parent, objectName, color);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<Image>();

        label = CreateText(rect, "Label", 30f);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12f, 6f);
        labelRect.offsetMax = new Vector2(-12f, -6f);
        return button;
    }

    private static void PositionButton(Button button, Vector2 position)
    {
        RectTransform rect = button.transform as RectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(290f, 78f);
    }
}
