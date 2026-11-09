using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class ConfirmationPromptUI : MonoBehaviour
{
    [Header("Editable Prefab References")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI backButtonText;
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;

    private Action backAction;
    private Action continueAction;
    private readonly Vector3[] canvasCorners = new Vector3[4];

    public bool IsConfigured =>
        messageText != null &&
        backButton != null &&
        backButtonText != null &&
        continueButton != null &&
        continueButtonText != null;

    private void Awake()
    {
        if (!IsConfigured)
        {
            Debug.LogError(
                "Confirmation Prompt prefab references are incomplete. " +
                "Open the Alahas Inventory Container prefab to repair them.",
                this);
            return;
        }

        backButton.onClick.RemoveListener(InvokeBack);
        backButton.onClick.AddListener(InvokeBack);
        continueButton.onClick.RemoveListener(InvokeContinue);
        continueButton.onClick.AddListener(InvokeContinue);
    }

    public void Show(
        string message,
        Action onBack,
        Action onContinue)
    {
        if (!IsConfigured) return;

        FitToRootCanvas();
        backAction = onBack;
        continueAction = onContinue;
        messageText.text = message;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    private void LateUpdate()
    {
        // The selector's parent is a narrow sliding strip, not the full screen.
        // Keep this modal's dimmer and click blocker fitted to the root canvas,
        // including after a window resize, without changing its prefab hierarchy.
        FitToRootCanvas();
    }

    private void FitToRootCanvas()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform rect = transform as RectTransform;
        if (canvas == null || rect == null || rect.parent == null) return;
        RectTransform root = canvas.rootCanvas.transform as RectTransform;
        if (root == null || root == rect) return;

        root.GetWorldCorners(canvasCorners);
        Vector3 bottomLeft = rect.parent.InverseTransformPoint(canvasCorners[0]);
        Vector3 topRight = rect.parent.InverseTransformPoint(canvasCorners[2]);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localPosition = (bottomLeft + topRight) * 0.5f;
        rect.sizeDelta = new Vector2(topRight.x - bottomLeft.x, topRight.y - bottomLeft.y);
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
}
