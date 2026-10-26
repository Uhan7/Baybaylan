using UnityEngine; 
using UnityEngine.UI;
using TMPro;

public class AlahasInfoPopup : ToolTipAble
{
    public Alahas currentAlahas;
    [SerializeField] private Image alahasImage;
    [SerializeField] private GameObject activationBadge;
    [SerializeField] private TextMeshProUGUI activationCountText;
    [SerializeField] private GameObject extraBadge;
    [SerializeField] private TextMeshProUGUI extraBadgeText;
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

        if (alahasImage != null)
        {
            Sprite icon = alahas ? alahas.alahasSprite : null;
            alahasImage.sprite = icon;
            alahasImage.enabled = icon != null;
        }

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

        RefreshExtraBadge();
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

        if (currentAlahas is DaliriNiTarabusawAlahas daliri)
        {
            daliri.TryActivate();
            return;
        }

        if (currentAlahas is DahonNgKawayanAlahas dahon && DahonNgKawayanUI.Instance != null)
            DahonNgKawayanUI.Instance.OpenSelection(dahon);

        if(currentAlahas is PangilNiOryolAlahas pangil)
            pangil.refreshTiles();
    }

    public void SetSelectionMode(bool value)
    {
        selectionMode = value;
    }

    override protected void startHover()
    {
        if (!CreateTooltipInstance()) return;
        alahasName = tooltipObjInstance.transform.GetChild(1).transform.GetComponent<TextMeshProUGUI>();
        alahasDesc = tooltipObjInstance.transform.GetChild(2).transform.GetComponent<TextMeshProUGUI>();
        alahasExtra = tooltipObjInstance.transform.GetChild(3).transform.GetComponent<TextMeshProUGUI>();
    }

    override protected void Update()
    {
        RefreshExtraBadge();

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

    private void RefreshExtraBadge()
    {
        string badgeText = string.Empty;
        bool shouldShow = currentAlahas != null &&
            currentAlahas.TryGetExtraBadgeText(out badgeText);

        if (extraBadge != null && extraBadge.activeSelf != shouldShow)
            extraBadge.SetActive(shouldShow);

        if (extraBadgeText != null && shouldShow && extraBadgeText.text != badgeText)
            extraBadgeText.text = badgeText;
    }
}
