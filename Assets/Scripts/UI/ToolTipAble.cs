using UnityEngine; 
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

//slap this on an obj to let the tooltip display its info when hovered over
public class ToolTipAble : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private const float MouseTooltipGap = 18f;
    [SerializeField] protected GameObject tooltipObj;
    [SerializeField] public string tipText;
    [SerializeField] protected float ToolTipDelay = 1f;
    [SerializeField] protected bool followMouse = false;
    [SerializeField] protected Vector2 ToolTipPositionOffset = new Vector2(-18, -18);
    protected GameObject tooltipObjInstance;
    TMP_Text tooltipText;
    protected Canvas tooltipCanvas;
    protected RectTransform tooltipCanvasRect;
    protected float timer = 0f;
    protected bool isHovered = false;
    protected bool onetime = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        endHover();
    }

    protected virtual void OnDisable()
    {
        HideTooltip();
    }

    protected virtual void OnDestroy()
    {
        HideTooltip();
    }

    protected virtual void Update()
    {
        if (isHovered)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timer = 0f;
        }

        if(timer >= ToolTipDelay)
        {
            oneTime();
            if (!tooltipObjInstance)
            {
                onetime = false;
                return;
            }

            tooltipText.text = tipText;
            PositionTooltip();
        }
    }

    protected virtual void startHover()
    {
        if (!CreateTooltipInstance()) return;
        tooltipText = tooltipObjInstance.GetComponentInChildren<TMP_Text>();
    }

    protected bool CreateTooltipInstance()
    {
        Canvas sourceCanvas = GetComponentInParent<Canvas>();
        tooltipCanvas = sourceCanvas != null ? sourceCanvas.rootCanvas : null;
        if (!tooltipCanvas)
            tooltipCanvas = GameObject.FindFirstObjectByType<Canvas>();

        if (!tooltipCanvas || !tooltipObj) return false;

        tooltipCanvasRect = tooltipCanvas.transform as RectTransform;
        tooltipObjInstance = Instantiate(tooltipObj, tooltipCanvas.transform);
        tooltipObjInstance.SetActive(true);
        tooltipObjInstance.transform.SetAsLastSibling();

        // Position in the actual screen canvas, then render above the nested
        // Alahas selector canvas. Mixing the selector's world-space RectTransform
        // with screen mouse coordinates caused its X position to appear stuck.
        Canvas visualCanvas = tooltipObjInstance.GetComponent<Canvas>();
        if (visualCanvas == null)
            visualCanvas = tooltipObjInstance.AddComponent<Canvas>();
        visualCanvas.overrideSorting = true;
        visualCanvas.sortingOrder = Mathf.Max(
            tooltipCanvas.sortingOrder,
            sourceCanvas != null ? sourceCanvas.sortingOrder : 0) + 1;

        DisableTooltipRaycasts();
        return true;
    }

    // Tooltips are visual-only. If their Images accept raycasts, they can eat
    // the first click intended for controls underneath (such as MAGPATULOY).
    protected void DisableTooltipRaycasts()
    {
        if (!tooltipObjInstance) return;

        foreach (Graphic graphic in tooltipObjInstance.GetComponentsInChildren<Graphic>(true))
            graphic.raycastTarget = false;
    }

    protected void PositionTooltip()
    {
        Camera canvasCamera = tooltipCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : tooltipCanvas.worldCamera;
        Vector2 screenPosition = followMouse
            ? (Vector2)Input.mousePosition
            : RectTransformUtility.WorldToScreenPoint(canvasCamera, transform.position);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipCanvasRect, screenPosition, canvasCamera, out Vector2 canvasPosition))
            return;

        CanvasScaler canvasScaler = tooltipCanvas.GetComponent<CanvasScaler>();
        Vector2 referenceSize = canvasScaler
            ? canvasScaler.referenceResolution
            : tooltipCanvasRect.rect.size;
        Vector2 canvasSize = tooltipCanvasRect.rect.size;
        Vector2 responsiveOffset = new Vector2(
            ToolTipPositionOffset.x * canvasSize.x / referenceSize.x,
            ToolTipPositionOffset.y * canvasSize.y / referenceSize.y);

        RectTransform tooltipRect = tooltipObjInstance.transform as RectTransform;
        if (followMouse)
        {
            // Grow away from the nearest horizontal edge. A tooltip that always
            // grows left gets clamped in place over Tala's equipped slots, which
            // makes it look as though it is not following the cursor on X.
            bool growRight = canvasPosition.x < tooltipCanvasRect.rect.center.x;
            float horizontalGap = MouseTooltipGap * canvasSize.x / referenceSize.x;
            float verticalGap = MouseTooltipGap * canvasSize.y / referenceSize.y;

            tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
            tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
            tooltipRect.pivot = new Vector2(growRight ? 0f : 1f, 1f);
            responsiveOffset = new Vector2(
                growRight ? horizontalGap : -horizontalGap,
                -verticalGap);
        }
        tooltipRect.localPosition = canvasPosition + responsiveOffset;

        Canvas.ForceUpdateCanvases();
        Bounds tooltipBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
            tooltipCanvasRect, tooltipRect);
        Rect canvasBounds = tooltipCanvasRect.rect;
        Vector3 correction = Vector3.zero;

        if (tooltipBounds.min.x < canvasBounds.xMin)
            correction.x = canvasBounds.xMin - tooltipBounds.min.x;
        else if (tooltipBounds.max.x > canvasBounds.xMax)
            correction.x = canvasBounds.xMax - tooltipBounds.max.x;

        if (tooltipBounds.min.y < canvasBounds.yMin)
            correction.y = canvasBounds.yMin - tooltipBounds.min.y;
        else if (tooltipBounds.max.y > canvasBounds.yMax)
            correction.y = canvasBounds.yMax - tooltipBounds.max.y;

        tooltipRect.localPosition += correction;
    }

    public void HideTooltip()
    {
        Destroy(tooltipObjInstance);
        tooltipObjInstance = null;
        isHovered = false;
        timer = 0f;
        onetime = false;
    }

    void endHover()
    {
        HideTooltip();
    }

    protected virtual void oneTime()
    {
        if(onetime)
            return;
        onetime = true;

        startHover();
    }
}
