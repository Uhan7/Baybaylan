using UnityEngine; 
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

//slap this on an obj to let the tooltip display its info when hovered over
class ToolTipAble : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected GameObject tooltipObj;
    [SerializeField] public string tipText;
    [SerializeField] protected float ToolTipDelay = 1f;
    [SerializeField] protected bool followMouse = false;
    [SerializeField] protected Vector2 ToolTipPositionOffset = new Vector2(300, 100);
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
        tooltipCanvas = GetComponentInParent<Canvas>();
        if (tooltipCanvas) tooltipCanvas = tooltipCanvas.rootCanvas;
        else tooltipCanvas = GameObject.FindFirstObjectByType<Canvas>();

        if (!tooltipCanvas) return;

        tooltipCanvasRect = tooltipCanvas.transform as RectTransform;
        tooltipObjInstance = Instantiate(tooltipObj, tooltipCanvas.transform);
        tooltipObjInstance.SetActive(true);
        tooltipText = tooltipObjInstance.GetComponentInChildren<TMP_Text>();
    }

    void PositionTooltip()
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

    void endHover()
    {
        Destroy(tooltipObjInstance);
        onetime = false;
    }

    protected virtual void oneTime()
    {
        if(onetime)
            return;
        onetime = true;

        startHover();
    }
}
