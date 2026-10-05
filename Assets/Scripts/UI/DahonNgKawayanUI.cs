using System.Collections.Generic;
using UnityEngine;

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
        mainUiParent.SetActive(false);
        if (spawnButton != null) spawnButton.SetActive(false);
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
