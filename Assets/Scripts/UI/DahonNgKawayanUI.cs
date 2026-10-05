using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class DahonNgKawayanUI : MonoBehaviour
{
    [SerializeField] GameObject tileLayoutGroupParent;
    [SerializeField] GameObject mainUiParent;
    [SerializeField] GameObject spawnButton;

    public static DahonNgKawayanUI Instance;

    private readonly List<GameObject> tileChoices = new List<GameObject>();
    private LevelConfig config;
    private DahonNgKawayanAlahas activeAlahas;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ConfigureSelectionTray();
        mainUiParent.SetActive(false);
        if (spawnButton != null) spawnButton.SetActive(false);
    }

    private void ConfigureSelectionTray()
    {
        Image trayImage = mainUiParent.GetComponent<Image>();
        if (trayImage != null) trayImage.raycastTarget = false;

        RectTransform trayRect = mainUiParent.transform as RectTransform;
        if (trayRect != null)
        {
            trayRect.anchorMin = new Vector2(0.5f, 1f);
            trayRect.anchorMax = new Vector2(0.5f, 1f);
            trayRect.pivot = new Vector2(0.5f, 1f);
            trayRect.anchoredPosition = new Vector2(0f, -24f);
            trayRect.sizeDelta = new Vector2(1210f, 330f);
        }

        foreach (TextMeshProUGUI text in mainUiParent.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            text.raycastTarget = false;
            if (text.gameObject.name.Trim() != "select tile text") continue;

            text.text = "drag whatever tile u want";
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = new Vector2(0.5f, 1f);
            textRect.anchorMax = new Vector2(0.5f, 1f);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = new Vector2(0f, -48f);
            textRect.sizeDelta = new Vector2(700f, 60f);
            textRect.localScale = Vector3.one;
        }

        foreach (Button button in mainUiParent.GetComponentsInChildren<Button>(true))
            button.gameObject.SetActive(false);

        RectTransform layoutRect = tileLayoutGroupParent.transform as RectTransform;
        if (layoutRect != null)
        {
            layoutRect.anchorMin = new Vector2(0.5f, 1f);
            layoutRect.anchorMax = new Vector2(0.5f, 1f);
            layoutRect.pivot = new Vector2(0.5f, 0.5f);
            layoutRect.anchoredPosition = new Vector2(0f, -190f);
            layoutRect.sizeDelta = new Vector2(1000f, 220f);
        }
    }

    private void SpawnTileChoices()
    {
        ClearTileChoices();
        if (config == null || TileSet.Instance == null) return;

        foreach (GameObject tilePrefab in config.tilesSelection)
        {
            if (tilePrefab == null) continue;

            GameObject choice = Instantiate(tilePrefab, tileLayoutGroupParent.transform);
            choice.name = $"{tilePrefab.name} (Dahon Choice)";
            TileSet.Instance.PrepareDahonNgKawayanTile(choice);
            choice.AddComponent<DahonNgKawayanChoice>().Initialize(this);
            tileChoices.Add(choice);
        }
    }

    public void OpenSelection(DahonNgKawayanAlahas alahas)
    {
        if (mainUiParent.activeSelf)
        {
            CloseSelection();
            return;
        }

        if (alahas == null || config == null || AlahasManager.Instance == null ||
            !AlahasManager.Instance.CanActivate(alahas))
            return;

        activeAlahas = alahas;
        SpawnTileChoices();
        mainUiParent.SetActive(tileChoices.Count > 0);
    }

    // Kept so older scene/prefab event references do not break.
    public void OpenSelection()
    {
        DahonNgKawayanAlahas dahon = FindHeldDahon();
        if (dahon != null) OpenSelection(dahon);
    }

    // Kept so older scene/prefab event references do not break.
    public void spawnTileButton()
    {
        OpenSelection();
    }

    // The old confirm button is hidden; direct dropping completes the selection.
    public void finishSelection()
    {
    }

    public void CloseSelection()
    {
        ClearTileChoices();
        activeAlahas = null;
        mainUiParent.SetActive(false);
    }

    public void CompleteSelection(GameObject selectedTile)
    {
        if (selectedTile == null || activeAlahas == null || AlahasManager.Instance == null ||
            !AlahasManager.Instance.TryConsumeActivation(activeAlahas))
        {
            if (selectedTile != null) Destroy(selectedTile);
            CloseSelection();
            return;
        }

        tileChoices.Remove(selectedTile);
        DahonNgKawayanChoice choice = selectedTile.GetComponent<DahonNgKawayanChoice>();
        if (choice != null) choice.Complete();
        CloseSelection();
    }

    public void getLevelConfig(LevelConfig levelConfig)
    {
        config = levelConfig;
    }

    private DahonNgKawayanAlahas FindHeldDahon()
    {
        if (AlahasManager.Instance == null || AlahasManager.Instance.heldAlahas == null)
            return null;

        foreach (Alahas alahas in AlahasManager.Instance.heldAlahas)
            if (alahas is DahonNgKawayanAlahas dahon) return dahon;

        return null;
    }

    private void ClearTileChoices()
    {
        foreach (GameObject choice in tileChoices)
            if (choice != null) Destroy(choice);

        tileChoices.Clear();
    }
}

class DahonNgKawayanChoice : MonoBehaviour, IDragNotify
{
    private DahonNgKawayanUI owner;

    public void Initialize(DahonNgKawayanUI selectionOwner)
    {
        owner = selectionOwner;
    }

    public void Complete()
    {
        owner = null;
    }

    public void OnDragBegin()
    {
    }

    public void OnDragEnd()
    {
        if (owner == null) return;

        bool landedInTileSet = GetComponentInParent<TileSet>() != null;
        bool landedInSalita = GetComponentInParent<SalitaSlots>() != null;

        if (landedInTileSet || landedInSalita)
            owner?.CompleteSelection(gameObject);
    }
}
