using UnityEngine; 
using UnityEngine.UI;
using TMPro;

public class AlahasInfoPopup : ToolTipAble
{
    public Alahas currentAlahas;
    [SerializeField] private Image alahasImage;
    [SerializeField] private GameObject activationBadge;
    [SerializeField] private TextMeshProUGUI activationCountText;
    private Button alahasButton;
    TextMeshProUGUI alahasName;
    TextMeshProUGUI alahasDesc;
    TextMeshProUGUI alahasExtra;
    private bool selectionMode;

    private void Awake()
    {
        // Alahas details should track the pointer while choosing a loadout.
        followMouse = true;
        alahasButton = GetComponent<Button>();

        if (alahasButton != null)
        {
            alahasButton.onClick.RemoveListener(HandleAlahasClicked);
            alahasButton.onClick.AddListener(HandleAlahasClicked);
        }
    }

    public void SetAlahas(Alahas alahas)
    {
        currentAlahas = alahas;

        if (alahasImage == null) return;

        Sprite icon = alahas ? alahas.alahasSprite : null;
        alahasImage.sprite = icon;
        alahasImage.enabled = icon != null;

        bool shouldShowActivationCounter = alahas != null && alahas.showActivationCounter;

        if (activationBadge != null)
            activationBadge.SetActive(shouldShowActivationCounter);

        if (activationCountText != null && shouldShowActivationCounter)
        {
            int remaining = AlahasManager.Instance != null
                ? AlahasManager.Instance.GetRemainingActivations(alahas)
                : alahas != null ? Mathf.Max(0, alahas.maximumActivations) : 0;
            activationCountText.text = remaining.ToString();
        }
    }

    public void SetTooltipAlahas(Alahas alahas)
    {
        currentAlahas = alahas;
        if (alahas == null) HideTooltip();
    }

    private void HandleAlahasClicked()
    {
        if (selectionMode) return;

        if (!currentAlahas || AlahasManager.Instance == null ||
            !AlahasManager.Instance.CanActivate(currentAlahas))
            return;

        if (currentAlahas is DahonNgKawayanAlahas dahon && DahonNgKawayanUI.Instance != null)
            DahonNgKawayanUI.Instance.OpenSelection(dahon);
    }

    public void SetSelectionMode(bool value)
    {
        selectionMode = value;
    }

    override protected void startHover()
    {
        tooltipCanvas = GetComponentInParent<Canvas>();
        if (tooltipCanvas && !tooltipCanvas.overrideSorting)
            tooltipCanvas = tooltipCanvas.rootCanvas;
        if (!tooltipCanvas)
            tooltipCanvas = GameObject.FindFirstObjectByType<Canvas>();

        if (!tooltipCanvas) return;

        tooltipCanvasRect = tooltipCanvas.transform as RectTransform;
        tooltipObjInstance = Instantiate(tooltipObj, tooltipCanvas.transform);
        tooltipObjInstance.SetActive(true);
        DisableTooltipRaycasts();
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
}
