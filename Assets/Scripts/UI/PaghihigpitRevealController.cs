using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PaghihigpitRevealController : MonoBehaviour
{
    [Header("Paghihigpit Card")]
    [SerializeField] private Image artwork;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI extraText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Flow")]
    [SerializeField] private Button proceedButton;
    [SerializeField] private TextMeshProUGUI proceedButtonText;
    [SerializeField] private int foregroundSortingOrder = 1100;

    private readonly List<Paghihigpit> revealQueue = new List<Paghihigpit>();
    private CanvasGroup canvasGroup;
    private TileSet pendingTileSet;
    private int revealIndex;
    private bool revealActive;

    private void Awake()
    {
        EnsureForegroundCanvas();
        canvasGroup = GetComponent<CanvasGroup>();
        SetVisible(false);

        if (proceedButton != null)
        {
            proceedButton.onClick.RemoveListener(Proceed);
            proceedButton.onClick.AddListener(Proceed);
        }
    }

    public static bool TryBeginReveal(TileSet tileSet)
    {
        PaghihigpitRevealController controller =
            FindFirstObjectByType<PaghihigpitRevealController>(FindObjectsInactive.Include);
        if (controller == null) return false;

        controller.BeginReveal(tileSet);
        return true;
    }

    public static bool HasRevealablePaghihigpit(LevelConfig config)
    {
        if (config == null) return false;

        if (config.activePaghihigpit != null)
            foreach (Paghihigpit paghihigpit in config.activePaghihigpit)
                if (paghihigpit != null) return true;

        if (config.nagpapalitAnyoForms == null) return false;
        foreach (NagpapalitAnyoForm form in config.nagpapalitAnyoForms)
        {
            if (form?.paghihigpit == null) continue;
            foreach (Paghihigpit paghihigpit in form.paghihigpit)
                if (paghihigpit != null) return true;
        }

        return false;
    }

    public void BeginReveal(TileSet tileSet)
    {
        if (revealActive) return;
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        pendingTileSet = tileSet;
        BuildRevealQueue();
        if (revealQueue.Count == 0)
        {
            FinishReveal();
            return;
        }

        revealIndex = 0;
        revealActive = true;
        transform.SetAsLastSibling();
        SetVisible(true);
        ShowCurrentPaghihigpit();
    }

    public void Proceed()
    {
        if (!revealActive) return;

        revealIndex++;
        if (revealIndex < revealQueue.Count)
        {
            ShowCurrentPaghihigpit();
            return;
        }

        FinishReveal();
    }

    private void BuildRevealQueue()
    {
        revealQueue.Clear();
        LevelConfig config = GameManager.Instance != null ? GameManager.Instance.config : null;
        if (config == null) return;

        AddUnique(config.activePaghihigpit);

        // Nagpapalit-anyo may introduce restrictions that are not active on
        // the first form. Reveal every possible form restriction up front so
        // the player understands the full level before choosing Alahas.
        if (config.nagpapalitAnyoForms == null) return;
        foreach (NagpapalitAnyoForm form in config.nagpapalitAnyoForms)
            if (form != null) AddUnique(form.paghihigpit);
    }

    private void AddUnique(IEnumerable<Paghihigpit> paghihigpitList)
    {
        if (paghihigpitList == null) return;
        foreach (Paghihigpit paghihigpit in paghihigpitList)
            if (paghihigpit != null && !revealQueue.Contains(paghihigpit))
                revealQueue.Add(paghihigpit);
    }

    private void ShowCurrentPaghihigpit()
    {
        Paghihigpit paghihigpit = revealQueue[revealIndex];
        if (artwork != null)
        {
            artwork.sprite = paghihigpit.paghihigpitSprite;
            artwork.enabled = artwork.sprite != null;
            artwork.preserveAspect = true;
        }

        if (nameText != null) nameText.text = paghihigpit.paghihigpitName;
        if (descriptionText != null) descriptionText.text = paghihigpit.description;
        if (extraText != null)
        {
            extraText.text = paghihigpit.extraText;
            extraText.gameObject.SetActive(!string.IsNullOrWhiteSpace(paghihigpit.extraText));
        }
        if (progressText != null) progressText.text = $"{revealIndex + 1} / {revealQueue.Count}";
        if (proceedButtonText != null)
            proceedButtonText.text = revealIndex == revealQueue.Count - 1
                ? "Pumili ng Alahas"
                : "Susunod";
    }

    private void FinishReveal()
    {
        revealActive = false;
        SetVisible(false);

        TileSet tileSet = pendingTileSet;
        pendingTileSet = null;
        tileSet?.ContinueToAlahasSelectionFromReveal();
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void EnsureForegroundCanvas()
    {
        Canvas revealCanvas = GetComponent<Canvas>();
        if (revealCanvas == null) revealCanvas = gameObject.AddComponent<Canvas>();
        revealCanvas.overrideSorting = true;
        revealCanvas.sortingOrder = foregroundSortingOrder;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }
}
