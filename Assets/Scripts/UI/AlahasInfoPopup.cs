using UnityEngine; 
using UnityEngine.UI;
using TMPro;

class AlahasInfoPopup : ToolTipAble
{
    public Alahas currentAlahas;
    [SerializeField] private Image alahasImage;
    TextMeshProUGUI alahasName;
    TextMeshProUGUI alahasDesc;
    TextMeshProUGUI alahasExtra;

    public void SetAlahas(Alahas alahas)
    {
        currentAlahas = alahas;
        alahasImage.sprite = alahas ? alahas.alahasSprite : null;
    }

    override protected void startHover()
    {
        tooltipCanvas = GetComponentInParent<Canvas>();
        if (tooltipCanvas) tooltipCanvas = tooltipCanvas.rootCanvas;
        else tooltipCanvas = GameObject.FindFirstObjectByType<Canvas>();

        if (!tooltipCanvas) return;

        tooltipCanvasRect = tooltipCanvas.transform as RectTransform;
        tooltipObjInstance = Instantiate(tooltipObj, tooltipCanvas.transform);
        tooltipObjInstance.SetActive(true);
        alahasName = tooltipObjInstance.transform.GetChild(1).transform.GetComponent<TextMeshProUGUI>();
        alahasDesc = tooltipObjInstance.transform.GetChild(2).transform.GetComponent<TextMeshProUGUI>();
        alahasExtra = tooltipObjInstance.transform.GetChild(3).transform.GetComponent<TextMeshProUGUI>();
    }

    override protected void Update()
    {
        if(!currentAlahas) return;

        if (isHovered) timer += Time.deltaTime;
        else timer = 0f;

        if(timer >= ToolTipDelay)
        {
            base.oneTime();
            if (!tooltipObjInstance)
            {
                onetime = false;
                return;
            }

            alahasName.text = currentAlahas.alahasName;
            alahasDesc.text = currentAlahas.description;
            alahasExtra.text = currentAlahas.extraText;

            PositionTooltip();
        }
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
}
