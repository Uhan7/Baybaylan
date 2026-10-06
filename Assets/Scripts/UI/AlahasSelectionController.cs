using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class AlahasSelectionController : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject fallbackSlotPanel;
    [SerializeField] private List<Alahas> allAlahas = new List<Alahas>();
    [SerializeField] private Sprite lockedSprite;

    [Header("Flow")]
    [SerializeField] private Button proceedButton;
    [SerializeField] private RectTransform slidingRoot;
    [SerializeField] private int foregroundSortingOrder = 1000;
    [Min(0f), SerializeField] private float slideDistance = 900f;
    [Min(0f), SerializeField] private float slideDuration = 0.45f;
    [SerializeField] private AnimationCurve slideCurve = null;
    [Min(0f), SerializeField] private float gameplayRevealDelay = 0.2f;
    [Min(0f), SerializeField] private float gameplayRevealDuration = 1f;
    [SerializeField] private AnimationCurve gameplayRevealCurve = null;

    private readonly List<AlahasInventoryItem> inventoryItems = new List<AlahasInventoryItem>();
    private readonly List<AlahasSelectionSlot> selectionSlots = new List<AlahasSelectionSlot>();
    private readonly List<SideUI> forcedSidePanels = new List<SideUI>();
    private readonly List<HiddenGraphicState> hiddenGraphics = new List<HiddenGraphicState>();
    private readonly List<HiddenSelectableState> hiddenSelectables = new List<HiddenSelectableState>();
    private List<Alahas> draftSlots = new List<Alahas>();
    private CanvasGroup canvasGroup;
    private TileSet pendingTileSet;
    private SceneController pendingSceneController;
    private string pendingSceneName;
    private Coroutine slideRoutine;
    private Vector2 hiddenPosition;
    private Vector2 shownPosition;
    private bool selectionActive;
    private bool proceedRequested;

    private sealed class HiddenGraphicState
    {
        public Graphic graphic;
        public bool enabled;
        public Color color;
    }

    private sealed class HiddenSelectableState
    {
        public Selectable selectable;
        public bool interactable;
    }

    private void Awake()
    {
        EnsureForegroundCanvas();
        canvasGroup = GetComponent<CanvasGroup>();
        if (slidingRoot == null) slidingRoot = transform as RectTransform;

        hiddenPosition = slidingRoot != null ? slidingRoot.anchoredPosition : Vector2.zero;
        shownPosition = hiddenPosition + Vector2.left * slideDistance;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (proceedButton != null)
        {
            proceedButton.onClick.RemoveListener(Proceed);
            proceedButton.onClick.AddListener(Proceed);
        }
    }

    public static bool TryBeginSelection(TileSet tileSet)
    {
        AlahasSelectionController controller = FindFirstObjectByType<AlahasSelectionController>(
            FindObjectsInactive.Include);
        if (controller == null) return false;

        controller.BeginSelection(tileSet);
        return true;
    }

    public void BeginSelection(TileSet tileSet)
    {
        if (selectionActive) return;
        if (AlahasManager.Instance == null)
        {
            Debug.LogWarning("Skipping Alahas selection because no AlahasManager is available.", this);
            tileSet?.BeginGameplay();
            return;
        }

        selectionActive = true;
        proceedRequested = false;
        pendingTileSet = tileSet;
        CacheInventoryItems();
        CacheSelectionSlots();

        int slotCount = Mathf.Max(1, selectionSlots.Count);
        draftSlots = AlahasManager.Instance.GetLoadoutSnapshot(slotCount);

        foreach (AlahasSelectionSlot slot in selectionSlots)
        {
            SideUI sidePanel = slot.GetComponentInParent<SideUI>(true);
            if (sidePanel != null && !forcedSidePanels.Contains(sidePanel))
            {
                forcedSidePanels.Add(sidePanel);
                sidePanel.SetForcedOpen(true);
            }
        }

        HideGameplayHud();
        RefreshSelectionUI();
        transform.SetAsLastSibling();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        StartSlide(shownPosition, false);
    }

    public bool BeginSelectionBeforeSceneSwap(
        SceneController sceneController,
        string sceneName)
    {
        if (sceneController == null || string.IsNullOrEmpty(sceneName))
            return false;

        pendingSceneController = sceneController;
        pendingSceneName = sceneName;

        // Some intro scenes activate their gameplay canvas immediately before
        // requesting the next scene. Its TileSet may already have opened this
        // selector, so simply turn that active selection into the scene gate.
        if (selectionActive) return true;

        BeginSelection(null);

        if (selectionActive) return true;

        pendingSceneController = null;
        pendingSceneName = null;
        return false;
    }

    public void TryAutoEquip(Alahas alahas)
    {
        if (!selectionActive || !IsUnlocked(alahas)) return;
        if (draftSlots.Contains(alahas))
        {
            Remove(alahas);
            return;
        }

        TryEquipInFirstAvailableBlock(alahas);
    }

    public void TryEquip(Alahas alahas, int preferredSlot)
    {
        if (!selectionActive || !IsUnlocked(alahas)) return;

        List<Alahas> candidate = new List<Alahas>(draftSlots);
        RemoveFrom(candidate, alahas);

        if (preferredSlot >= 0 && preferredSlot < candidate.Count && candidate[preferredSlot] != null)
            RemoveFrom(candidate, candidate[preferredSlot]);

        int slotsNeeded = Mathf.Max(1, alahas.numberOfSlotsNeeded);
        int startIndex = IsBlockFree(candidate, preferredSlot, slotsNeeded)
            ? preferredSlot
            : FindFirstFreeBlock(candidate, slotsNeeded);

        if (startIndex < 0) return;

        for (int i = 0; i < slotsNeeded; i++)
            candidate[startIndex + i] = alahas;

        draftSlots = candidate;
        RefreshSelectionUI();
    }

    public void RemoveAt(int slotIndex)
    {
        if (!selectionActive || slotIndex < 0 || slotIndex >= draftSlots.Count) return;
        Remove(draftSlots[slotIndex]);
    }

    public bool TryEquipAtScreenPosition(Alahas alahas, Vector2 screenPosition, Camera eventCamera)
    {
        if (!selectionActive || !IsUnlocked(alahas)) return false;

        for (int i = 0; i < selectionSlots.Count; i++)
        {
            RectTransform slotRect = selectionSlots[i].transform as RectTransform;
            if (slotRect == null ||
                !RectTransformUtility.RectangleContainsScreenPoint(slotRect, screenPosition, eventCamera))
                continue;

            TryEquip(alahas, i);
            return true;
        }

        return false;
    }

    public void RefreshSelectionUI()
    {
        for (int i = 0; i < selectionSlots.Count; i++)
        {
            AlahasInfoPopup popup = selectionSlots[i].GetComponent<AlahasInfoPopup>();
            popup?.SetAlahas(i < draftSlots.Count ? draftSlots[i] : null);
        }

        TalaAlahasHolder inventory = TalaAlahasHolder.Instance;
        List<Alahas> displayAlahas = BuildDisplayOrder(inventory);

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            Alahas alahas = i < displayAlahas.Count ? displayAlahas[i] : null;
            bool unlocked = alahas != null &&
                (draftSlots.Contains(alahas) ||
                 (inventory != null && inventory.IsUnlocked(alahas)));
            inventoryItems[i].Configure(
                this,
                inventoryItems[i].GetComponent<Image>(),
                alahas,
                unlocked,
                lockedSprite,
                alahas != null && draftSlots.Contains(alahas));
        }
    }

    private List<Alahas> BuildDisplayOrder(TalaAlahasHolder inventory)
    {
        List<Alahas> displayAlahas = new List<Alahas>();

        // Keep the currently equipped Alahas first and in slot order. This is
        // more useful than alphabetical sorting and keeps Balahibo (the usual
        // starting Alahas) in the first inventory cell.
        AddUnique(displayAlahas, draftSlots.Where(IsUnlocked));

        if (inventory != null && inventory.availableAlahas != null)
            AddUnique(displayAlahas, inventory.availableAlahas.Where(inventory.IsUnlocked));

        AddUnique(displayAlahas, allAlahas.Where(alahas => alahas != null));
        return displayAlahas;
    }

    private static void AddUnique(List<Alahas> destination, IEnumerable<Alahas> source)
    {
        foreach (Alahas alahas in source)
            if (alahas != null && !destination.Contains(alahas))
                destination.Add(alahas);
    }

    public void Proceed()
    {
        if (!selectionActive || proceedRequested) return;
        proceedRequested = true;
        selectionActive = false;

        foreach (ToolTipAble tooltip in GetComponentsInChildren<ToolTipAble>(true))
            tooltip.HideTooltip();

        AlahasManager.Instance.SetLoadout(draftSlots);

        foreach (AlahasSelectionSlot slot in selectionSlots)
            slot.EndSelection();
        foreach (SideUI sidePanel in forcedSidePanels)
            if (sidePanel != null) sidePanel.SetForcedOpen(false);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        StartSlide(hiddenPosition, true);
    }

    private void FinishSelection()
    {
        canvasGroup.alpha = 0f;

        if (pendingSceneController != null)
        {
            SceneController sceneController = pendingSceneController;
            string sceneName = pendingSceneName;
            pendingSceneController = null;
            pendingSceneName = null;
            RestoreGameplayHud();
            sceneController.SwapWrapper(sceneName);
            return;
        }

        TileSet tileSet = pendingTileSet;
        pendingTileSet = null;
        StartCoroutine(RevealGameplayHud(tileSet));
    }

    private void OnDestroy()
    {
        RestoreGameplayHud();
    }

    private void EnsureForegroundCanvas()
    {
        Canvas selectionCanvas = GetComponent<Canvas>();
        if (selectionCanvas == null) selectionCanvas = gameObject.AddComponent<Canvas>();
        selectionCanvas.overrideSorting = true;
        selectionCanvas.sortingOrder = foregroundSortingOrder;

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();
    }

    private void HideGameplayHud()
    {
        RestoreGameplayHud();

        Canvas selectorCanvas = GetComponent<Canvas>();
        Canvas rootCanvas = selectorCanvas != null ? selectorCanvas.rootCanvas : null;
        if (rootCanvas == null) return;

        HashSet<Transform> visibleRoots = new HashSet<Transform> { transform };
        Transform sceneBackgrounds = rootCanvas.transform.Find("Backgrounds");
        if (sceneBackgrounds != null)
            visibleRoots.Add(sceneBackgrounds);

        foreach (SideUI sidePanel in forcedSidePanels)
            if (sidePanel != null) visibleRoots.Add(sidePanel.transform);
        foreach (AlahasSelectionSlot slot in selectionSlots)
            visibleRoots.Add(slot.transform);

        foreach (Graphic graphic in rootCanvas.GetComponentsInChildren<Graphic>(true))
        {
            if (IsInsideVisibleRoot(graphic.transform, visibleRoots)) continue;

            hiddenGraphics.Add(new HiddenGraphicState
                { graphic = graphic, enabled = graphic.enabled, color = graphic.color });
            graphic.enabled = false;
        }

        foreach (Selectable selectable in rootCanvas.GetComponentsInChildren<Selectable>(true))
        {
            if (IsInsideVisibleRoot(selectable.transform, visibleRoots)) continue;

            hiddenSelectables.Add(new HiddenSelectableState
                { selectable = selectable, interactable = selectable.interactable });
            selectable.interactable = false;
        }
    }

    private void RestoreGameplayHud()
    {
        foreach (HiddenGraphicState state in hiddenGraphics)
        {
            if (state.graphic == null) continue;
            state.graphic.enabled = state.enabled;
            state.graphic.color = state.color;
        }
        hiddenGraphics.Clear();

        foreach (HiddenSelectableState state in hiddenSelectables)
            if (state.selectable != null) state.selectable.interactable = state.interactable;
        hiddenSelectables.Clear();
    }

    private IEnumerator RevealGameplayHud(TileSet tileSet)
    {
        if (gameplayRevealDelay > 0f)
            yield return new WaitForSeconds(gameplayRevealDelay);

        foreach (HiddenGraphicState state in hiddenGraphics)
        {
            if (state.graphic == null) continue;
            state.graphic.enabled = state.enabled;
            if (!state.enabled) continue;

            Color transparent = state.color;
            transparent.a = 0f;
            state.graphic.color = transparent;
        }

        tileSet?.BeginGameplay();

        float duration = Mathf.Max(0f, gameplayRevealDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float linear = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float eased = gameplayRevealCurve != null && gameplayRevealCurve.length > 0
                ? gameplayRevealCurve.Evaluate(linear)
                : Mathf.SmoothStep(0f, 1f, linear);

            foreach (HiddenGraphicState state in hiddenGraphics)
            {
                if (state.graphic == null || !state.enabled) continue;
                Color color = state.color;
                color.a *= eased;
                state.graphic.color = color;
            }

            yield return null;
        }

        foreach (HiddenGraphicState state in hiddenGraphics)
        {
            if (state.graphic == null) continue;
            state.graphic.enabled = state.enabled;
            state.graphic.color = state.color;
        }
        hiddenGraphics.Clear();

        foreach (HiddenSelectableState state in hiddenSelectables)
            if (state.selectable != null) state.selectable.interactable = state.interactable;
        hiddenSelectables.Clear();
    }

    private static bool IsInsideVisibleRoot(
        Transform candidate,
        HashSet<Transform> visibleRoots)
    {
        if (candidate == null) return false;

        foreach (Transform visibleRoot in visibleRoots)
            if (visibleRoot != null &&
                (candidate == visibleRoot || candidate.IsChildOf(visibleRoot)))
                return true;

        return false;
    }

    private void CacheInventoryItems()
    {
        inventoryItems.Clear();
        if (inventoryGrid == null) return;

        for (int i = 0; i < inventoryGrid.childCount; i++)
        {
            Transform child = inventoryGrid.GetChild(i);
            Image image = child.GetComponent<Image>();
            if (image == null) continue;

            AlahasInventoryItem item = child.GetComponent<AlahasInventoryItem>();
            if (item == null) item = child.gameObject.AddComponent<AlahasInventoryItem>();
            inventoryItems.Add(item);
        }
    }

    private void CacheSelectionSlots()
    {
        selectionSlots.Clear();
        forcedSidePanels.Clear();

        AlahasInfoPopup[] allPopups = FindObjectsByType<AlahasInfoPopup>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .Where(popup => popup.gameObject.scene.IsValid() && popup.CompareTag("Alahas Slot"))
            .ToArray();

        AlahasInfoPopup[] externalPopups = allPopups
            .Where(popup => !popup.transform.IsChildOf(transform))
            .OrderBy(popup => popup.gameObject.name)
            .ToArray();

        bool useFallbackSlots = externalPopups.Length == 0;
        if (fallbackSlotPanel != null)
            fallbackSlotPanel.SetActive(useFallbackSlots);

        AlahasInfoPopup[] popups = useFallbackSlots
            ? allPopups
                .Where(popup => popup.transform.IsChildOf(transform))
                .OrderBy(popup => popup.gameObject.name)
                .ToArray()
            : externalPopups;

        for (int i = 0; i < popups.Length; i++)
        {
            AlahasSelectionSlot slot = popups[i].GetComponent<AlahasSelectionSlot>();
            if (slot == null) slot = popups[i].gameObject.AddComponent<AlahasSelectionSlot>();
            slot.Configure(this, popups[i], i);
            selectionSlots.Add(slot);
        }
    }

    private void TryEquipInFirstAvailableBlock(Alahas alahas)
    {
        int slotsNeeded = Mathf.Max(1, alahas.numberOfSlotsNeeded);
        int startIndex = FindFirstFreeBlock(draftSlots, slotsNeeded);
        if (startIndex < 0) return;

        for (int i = 0; i < slotsNeeded; i++)
            draftSlots[startIndex + i] = alahas;

        RefreshSelectionUI();
    }

    private void Remove(Alahas alahas)
    {
        if (alahas == null) return;
        RemoveFrom(draftSlots, alahas);
        RefreshSelectionUI();
    }

    private static void RemoveFrom(List<Alahas> slots, Alahas alahas)
    {
        for (int i = 0; i < slots.Count; i++)
            if (slots[i] == alahas) slots[i] = null;
    }

    private static int FindFirstFreeBlock(List<Alahas> slots, int length)
    {
        for (int i = 0; i <= slots.Count - length; i++)
            if (IsBlockFree(slots, i, length)) return i;
        return -1;
    }

    private static bool IsBlockFree(List<Alahas> slots, int startIndex, int length)
    {
        if (startIndex < 0 || startIndex + length > slots.Count) return false;
        for (int i = 0; i < length; i++)
            if (slots[startIndex + i] != null) return false;
        return true;
    }

    private bool IsUnlocked(Alahas alahas)
    {
        if (!alahas) return false;
        return draftSlots.Contains(alahas) ||
            (TalaAlahasHolder.Instance != null &&
             TalaAlahasHolder.Instance.IsUnlocked(alahas));
    }

    private void StartSlide(Vector2 target, bool finishWhenDone)
    {
        if (slidingRoot == null)
        {
            if (finishWhenDone) FinishSelection();
            return;
        }

        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideTo(target, finishWhenDone));
    }

    private IEnumerator SlideTo(Vector2 target, bool finishWhenDone)
    {
        Vector2 start = slidingRoot.anchoredPosition;
        float duration = Mathf.Max(0f, slideDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float linear = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float eased = slideCurve != null && slideCurve.length > 0
                ? slideCurve.Evaluate(linear)
                : Mathf.SmoothStep(0f, 1f, linear);
            slidingRoot.anchoredPosition = Vector2.LerpUnclamped(start, target, eased);
            yield return null;
        }

        slidingRoot.anchoredPosition = target;
        slideRoutine = null;
        if (finishWhenDone) FinishSelection();
    }
}
