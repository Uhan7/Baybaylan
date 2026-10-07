using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AlahasInventoryItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private AlahasSelectionController controller;
    private Image itemImage;
    private Canvas sourceCanvas;
    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Animator animator;
    private AlahasInfoPopup tooltip;
    private Alahas alahas;
    private bool unlocked;
    private bool dragging;
    private RectTransform dragPreview;
    private Color normalColor = Color.white;

    public Alahas Alahas => alahas;
    public bool IsUnlocked => unlocked;

    public void Configure(
        AlahasSelectionController owner,
        Image image,
        Alahas representedAlahas,
        bool isUnlocked,
        Sprite lockedSprite,
        bool isEquipped)
    {
        controller = owner;
        itemImage = image;
        alahas = representedAlahas;
        unlocked = isUnlocked && alahas != null;
        sourceCanvas = GetComponentInParent<Canvas>();
        rootCanvas = sourceCanvas != null ? sourceCanvas.rootCanvas : null;
        rectTransform = transform as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        animator = GetComponent<Animator>();
        tooltip = GetComponent<AlahasInfoPopup>();

        if (tooltip != null)
        {
            tooltip.SetTooltipAlahas(unlocked ? alahas : null);
            tooltip.enabled = unlocked;
        }

        itemImage.sprite = unlocked ? alahas.alahasSprite : lockedSprite;
        itemImage.preserveAspect = true;
        itemImage.raycastTarget = true;

        normalColor = unlocked
            ? (isEquipped ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : Color.white)
            : new Color(1f, 1f, 1f, 0.7f);
        ApplyVisualState();
    }

    private void LateUpdate()
    {
        // Tile Base uses Write Defaults and includes color animation in one of
        // its states, so the Animator can restore this Image to white after
        // Configure runs. Reapply the selection tint after animation while
        // leaving its hover/hold scale animation intact.
        ApplyVisualState();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!unlocked || controller == null || rootCanvas == null || rectTransform == null) return;

        dragging = true;
        tooltip?.HideTooltip();
        animator?.SetBool("Hover", false);
        animator?.SetBool("Hold", false);
        canvasGroup.blocksRaycasts = false;
        CreateDragPreview(eventData);

        ApplyVisualState();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging || dragPreview == null) return;
        PositionDragPreview(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;

        controller.TryEquipAtScreenPosition(alahas, eventData.position, eventData.pressEventCamera);

        DestroyDragPreview();
        canvasGroup.blocksRaycasts = true;
        animator?.SetBool("Hold", false);
        controller.RefreshSelectionUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!dragging && unlocked && controller != null)
            controller.TryAutoEquip(alahas);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unlocked && !dragging)
            animator?.SetBool("Hover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        animator?.SetBool("Hover", false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (unlocked && !dragging)
            animator?.SetBool("Hold", true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        animator?.SetBool("Hold", false);
    }

    private void CreateDragPreview(PointerEventData eventData)
    {
        DestroyDragPreview();

        GameObject previewObject = new GameObject(
            $"{gameObject.name} Drag Preview",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup));
        dragPreview = previewObject.GetComponent<RectTransform>();
        dragPreview.SetParent(rootCanvas.transform, false);
        dragPreview.SetAsLastSibling();
        dragPreview.anchorMin = new Vector2(0.5f, 0.5f);
        dragPreview.anchorMax = new Vector2(0.5f, 0.5f);
        dragPreview.pivot = new Vector2(0.5f, 0.5f);
        dragPreview.sizeDelta = GetDragPreviewSize();

        Canvas previewCanvas = previewObject.GetComponent<Canvas>();
        previewCanvas.overrideSorting = true;
        previewCanvas.sortingOrder = sourceCanvas != null
            ? sourceCanvas.sortingOrder + 1
            : 1001;

        Image previewImage = previewObject.GetComponent<Image>();
        previewImage.sprite = itemImage.sprite;
        previewImage.material = itemImage.material;
        previewImage.type = itemImage.type;
        previewImage.preserveAspect = true;
        previewImage.raycastTarget = false;
        Color previewColor = normalColor;
        previewColor.a = Mathf.Max(0.9f, previewColor.a);
        previewImage.color = previewColor;

        CanvasGroup previewGroup = previewObject.GetComponent<CanvasGroup>();
        previewGroup.interactable = false;
        previewGroup.blocksRaycasts = false;

        PositionDragPreview(eventData);
    }

    private void PositionDragPreview(PointerEventData eventData)
    {
        if (dragPreview == null || rootCanvas == null) return;

        RectTransform canvasRect = rootCanvas.transform as RectTransform;
        Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;

        // Set the preview in world space. The project's root canvas uses a
        // bottom-left pivot, so assigning a centre-anchored local point caused
        // a large aspect-ratio-dependent offset from the cursor.
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvasRect, eventData.position, eventCamera, out Vector3 worldPoint))
            dragPreview.position = worldPoint;
    }

    private Vector2 GetDragPreviewSize()
    {
        if (rectTransform == null || rootCanvas == null)
            return rectTransform != null ? rectTransform.rect.size : Vector2.zero;

        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Camera sourceCamera = sourceCanvas != null &&
            sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? sourceCanvas.worldCamera
                : null;
        Vector2 bottomLeft = RectTransformUtility.WorldToScreenPoint(sourceCamera, corners[0]);
        Vector2 topRight = RectTransformUtility.WorldToScreenPoint(sourceCamera, corners[2]);
        float scaleFactor = Mathf.Max(0.0001f, rootCanvas.scaleFactor);

        return new Vector2(
            Mathf.Abs(topRight.x - bottomLeft.x) / scaleFactor,
            Mathf.Abs(topRight.y - bottomLeft.y) / scaleFactor);
    }

    private void DestroyDragPreview()
    {
        if (dragPreview != null)
            Destroy(dragPreview.gameObject);
        dragPreview = null;
    }

    private void ApplyVisualState()
    {
        if (itemImage == null) return;

        Color color = normalColor;
        if (dragging) color.a *= 0.35f;
        itemImage.color = color;
    }

    private void OnDisable()
    {
        DestroyDragPreview();
        dragging = false;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
    }
}
