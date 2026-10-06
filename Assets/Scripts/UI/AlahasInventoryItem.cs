using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AlahasInventoryItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private AlahasSelectionController controller;
    private Image itemImage;
    private Canvas rootCanvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Animator animator;
    private AlahasInfoPopup tooltip;
    private Alahas alahas;
    private bool unlocked;
    private bool dragging;
    private bool suppressNextClick;
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
        rootCanvas = GetComponentInParent<Canvas>();
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
            ? (isEquipped ? new Color(1f, 1f, 1f, 0.45f) : Color.white)
            : new Color(1f, 1f, 1f, 0.7f);
        itemImage.color = normalColor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!unlocked || controller == null || rootCanvas == null || rectTransform == null) return;

        dragging = true;
        suppressNextClick = true;
        tooltip?.HideTooltip();
        animator?.SetBool("Hover", false);
        animator?.SetBool("Hold", false);
        canvasGroup.blocksRaycasts = false;
        CreateDragPreview(eventData);

        Color faded = normalColor;
        faded.a *= 0.35f;
        itemImage.color = faded;
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
        if (suppressNextClick)
        {
            suppressNextClick = false;
            return;
        }

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
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(CanvasGroup));
        dragPreview = previewObject.GetComponent<RectTransform>();
        dragPreview.SetParent(rootCanvas.transform, false);
        dragPreview.SetAsLastSibling();
        dragPreview.anchorMin = new Vector2(0.5f, 0.5f);
        dragPreview.anchorMax = new Vector2(0.5f, 0.5f);
        dragPreview.pivot = new Vector2(0.5f, 0.5f);
        dragPreview.sizeDelta = rectTransform.rect.size;

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
            : eventData.pressEventCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, eventData.position, eventCamera, out Vector2 localPoint))
            dragPreview.anchoredPosition = localPoint;
    }

    private void DestroyDragPreview()
    {
        if (dragPreview != null)
            Destroy(dragPreview.gameObject);
        dragPreview = null;
    }

    private void OnDisable()
    {
        DestroyDragPreview();
        dragging = false;
        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
    }
}
