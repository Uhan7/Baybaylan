using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

class DahonNgKawayanUI : MonoBehaviour
{
    private static readonly string[] LetterOrder =
    {
        "BA", "KA", "DA", "GA", "HA", "LA", "MA", "NA", "NGA",
        "PA", "RA", "SA", "TA", "WA", "YA", "A", "I", "U"
    };

    [SerializeField] GameObject tileLayoutGroupParent;
    [SerializeField] GameObject mainUiParent;
    [SerializeField] GameObject spawnButton;

    public static DahonNgKawayanUI Instance;

    private readonly List<GameObject> tileChoices = new List<GameObject>();
    private LevelConfig config;
    private DahonNgKawayanAlahas activeAlahas;
    private bool pointerStartedOnTileChoice;
    private bool closePending;
    private int openedFrame = -1;

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

    void Update()
    {
        if (!mainUiParent.activeSelf || Time.frameCount == openedFrame) return;

        if (Input.GetMouseButtonUp(0) && !closePending)
            StartCoroutine(CloseAfterPointerRelease());
    }

    private IEnumerator CloseAfterPointerRelease()
    {
        closePending = true;
        yield return null;

        if (mainUiParent.activeSelf && !pointerStartedOnTileChoice)
            CloseSelection();

        pointerStartedOnTileChoice = false;
        closePending = false;
    }

    private void SpawnTileChoices()
    {
        ClearTileChoices();
        if (config == null || TileSet.Instance == null) return;

        List<GameObject> orderedTilePrefabs = new List<GameObject>(config.tilesSelection);
        orderedTilePrefabs.Sort(CompareTilePrefabs);

        foreach (GameObject tilePrefab in orderedTilePrefabs)
        {
            if (tilePrefab == null) continue;

            GameObject choice = Instantiate(tilePrefab, tileLayoutGroupParent.transform);
            choice.name = $"{tilePrefab.name} (Dahon Choice)";
            TileSet.Instance.PrepareDahonNgKawayanTile(choice);
            choice.AddComponent<DahonNgKawayanChoice>().Initialize(this);
            tileChoices.Add(choice);
        }
    }

    private static int CompareTilePrefabs(GameObject left, GameObject right)
    {
        int orderComparison = GetTileOrderIndex(left).CompareTo(GetTileOrderIndex(right));
        if (orderComparison != 0) return orderComparison;

        string leftName = left != null ? left.name : string.Empty;
        string rightName = right != null ? right.name : string.Empty;
        return string.CompareOrdinal(leftName, rightName);
    }

    private static int GetTileOrderIndex(GameObject tilePrefab)
    {
        if (tilePrefab == null) return int.MaxValue;

        Tile tile = tilePrefab.GetComponent<Tile>();
        string letter = tile == null
            ? tilePrefab.name
            : tile.isVowel
                ? tile.vowel
                : tile.rootConsonant + "a";
        letter = letter.ToUpperInvariant();

        for (int i = 0; i < LetterOrder.Length; i++)
            if (LetterOrder[i] == letter) return i;

        return int.MaxValue;
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
        pointerStartedOnTileChoice = false;
        mainUiParent.SetActive(tileChoices.Count > 0);
        openedFrame = Time.frameCount;
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
        pointerStartedOnTileChoice = false;
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

    public void NotifyTilePointerDown()
    {
        pointerStartedOnTileChoice = true;
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

class DahonNgKawayanChoice : MonoBehaviour, IDragNotify, IPointerDownHandler
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
        owner?.NotifyTilePointerDown();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        owner?.NotifyTilePointerDown();
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
