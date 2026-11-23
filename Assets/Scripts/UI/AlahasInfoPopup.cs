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
    [Header("Clickable Alahas Glow")]
    [SerializeField] private Image clickableHalo;
    [SerializeField] private UILightOverlay borderGlow;
    [SerializeField, Min(0.1f)] private float glowPeriod = 4f;
    [SerializeField, Range(0f, 1f)] private float minimumGlow = 0.15f;
    [SerializeField, Range(0f, 1f)] private float maximumGlow = 0.9f;
    private Graphic[] slotGraphics;
    private float[] originalAlphas;
    private bool? wasExhausted;

    private void Awake()
    {
        // Alahas details should track the pointer while choosing a loadout.
        followMouse = true;
        alahasButton = GetComponent<Button>();
        slotGraphics = GetComponentsInChildren<Graphic>(true);
        originalAlphas = new float[slotGraphics.Length];
        for (int i = 0; i < slotGraphics.Length; i++)
            originalAlphas[i] = slotGraphics[i].color.a;

        if (alahasButton != null)
        {
            alahasButton.onClick.RemoveListener(HandleAlahasClicked);
            alahasButton.onClick.AddListener(HandleAlahasClicked);
        }
        RefreshInteraction();
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
        RefreshInteraction();
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
        RefreshInteraction();
    }

    private void RefreshInteraction()
    {
        // Passive effects never act as HUD buttons. The picker still needs
        // button feedback for equipping/removing any type of Alahas.
        bool activatable = currentAlahas is DahonNgKawayanAlahas ||
            currentAlahas is DaliriNiTarabusawAlahas ||
            currentAlahas is PangilNiOryolAlahas;
        bool hasUses = currentAlahas != null &&
            (AlahasManager.Instance != null
                ? AlahasManager.Instance.CanActivate(currentAlahas)
                : currentAlahas.maximumActivations > 0);
        bool exhausted = !selectionMode && activatable && !hasUses;
        if (clickableHalo != null)
        {
            clickableHalo.enabled = !selectionMode && activatable && hasUses;
            float pulse = (1f - Mathf.Cos(Time.unscaledTime * Mathf.PI * 2f /
                Mathf.Max(0.1f, glowPeriod))) * 0.5f;
            if (borderGlow != null)
                borderGlow.SetStrength(Mathf.Lerp(minimumGlow, maximumGlow, pulse));
        }
        if (alahasButton != null)
        {
            bool clickable = selectionMode || (activatable && hasUses);
            alahasButton.interactable = clickable;
            alahasButton.enabled = clickable;
        }

        if (slotGraphics == null || wasExhausted == exhausted) return;
        wasExhausted = exhausted;
        for (int i = 0; i < slotGraphics.Length; i++)
        {
            if (slotGraphics[i] == null || slotGraphics[i] == clickableHalo) continue;
            Color color = slotGraphics[i].color;
            color.a = originalAlphas[i] * (exhausted ? 0.5f : 1f);
            slotGraphics[i].color = color;
        }
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
        RefreshInteraction();
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
