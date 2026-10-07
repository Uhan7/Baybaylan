using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine.UI;

//salita slots for wehn submit button is pressed and scoring 
//tile mods are done in tile sets

//detects and changes gamestates for a clean ish implementation of items/alahas
class AlahasSubManager : MonoBehaviour
{
    //states from other scripts
    [ReadOnly, SerializeField] public bool dialogueEnded = false;
    [ReadOnly, SerializeField] public bool reccButtonPressed = false;

    //states to send to other scripts
    [ReadOnly, SerializeField] public bool boostVowels = false;
    [ReadOnly, SerializeField] public float vowelSpawnChanceIncrease = 0;
    [ReadOnly, SerializeField] public float vowelScoreMulti = 1f;
    [ReadOnly, SerializeField] public bool spawnGolds = false;
    [ReadOnly, SerializeField] public float goldSpawnChance = 0;
    [ReadOnly, SerializeField] public float goldScoreMulti = 1f;
    [ReadOnly, SerializeField] public bool toolTipTiles = false;
    [ReadOnly, SerializeField] public bool canCreateTile = false;
    [ReadOnly, SerializeField] public bool add3ExtraTiles = false;
    [ReadOnly, SerializeField] public int extraTilesToAdd = 0;
    [ReadOnly, SerializeField] public bool delete5Tiles = false;
    [ReadOnly, SerializeField] public int tilesToDelete = 0;
    [ReadOnly, SerializeField] public bool reccWordButtonActive = false;
    [ReadOnly, SerializeField] public float scoreMultiplier = 1f; //general use 
    [ReadOnly, SerializeField] public bool addExtraTurnAndTile = false;
    [ReadOnly, SerializeField] public int extraTurn = 0;
    [ReadOnly, SerializeField] public int extraTile = 0;
    //-------------------------------------------
    public static AlahasSubManager Instance;
    AlahasManager alahasManagerScript;
    List<Alahas> heldAlahas;
    public bool gameplayStarted { get; private set; } = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // TileSet begins Alahas effects after the pre-game loadout is confirmed.
    }

    public void BeginGameplay()
    {
        if (gameplayStarted) return;

        alahasManagerScript = AlahasManager.Instance;
        heldAlahas = alahasManagerScript != null
            ? alahasManagerScript.heldAlahas
            : new List<Alahas>();

        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i) continue;

            alahas.onStart();
        }

        gameplayStarted = true;
    }

    void Update()
    {
        if (!gameplayStarted) return;

        onUpdate();

        //updates all the needed bools 
        
    }

    public void onSubmit()
    {
        if (!gameplayStarted || heldAlahas == null) return;

        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i) continue;

            alahas.onSubmit();
        }

        // Some passives (notably Kuwintas) change their multiplier on submit.
        // Refresh it before ScoreSalita reads the value for its very first tile.
        ApplyPassiveEffects();
    }

    public void onTurnEnd()
    {
        if (!gameplayStarted || heldAlahas == null) return;

        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i) continue;

            alahas.onTurnEnd();
        }
    }

    public void onTilesRefreshed()
    {
        if (!gameplayStarted || heldAlahas == null) return;

        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i) continue;

            if (alahas is PakpakNiPahAlahas pakpak)
                pakpak.OnTilesRefreshed();
        }
    }

    void onUpdate()
    {
        ApplyPassiveEffects();

        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i)
                continue;

            if(alahas.triggerCondition() &&
                AlahasManager.Instance != null &&
                AlahasManager.Instance.TryConsumeActivation(alahas))
                alahas.onTriggerEffect();
        }
    }

    private void ApplyPassiveEffects()
    {
        ResetPassiveEffects();

        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i)
                continue;

            alahas.onUpdate();
        }
    }

    private void ResetPassiveEffects()
    {
        // These values describe the currently equipped passive Alahas. Reset
        // them before rebuilding the effects so an unequipped Luya (or another
        // passive) cannot leave stale values behind.
        boostVowels = false;
        vowelSpawnChanceIncrease = 0f;
        vowelScoreMulti = 1f;
        spawnGolds = false;
        goldSpawnChance = 0f;
        goldScoreMulti = 1f;
        toolTipTiles = false;
        scoreMultiplier = 1f;
    }

    public void spawnTiles(int amount)
    {
        StartCoroutine(TileSet.Instance.SpawnTiles(amount));
    }

    public bool TryFormRecommendedWord(int minTiles, int maxTiles)
    {
        if (!gameplayStarted || TileSet.Instance == null || GameManager.Instance == null)
            return false;

        SalitaSlots salitaSlots = FindFirstObjectByType<SalitaSlots>();
        if (salitaSlots == null) return false;

        List<Tile> tiles = new List<Tile>();
        AddDirectChildTiles(TileSet.Instance.transform, tiles);
        AddDirectChildTiles(salitaSlots.transform, tiles);

        int safeMin = Mathf.Max(1, minTiles);
        int safeMax = Mathf.Min(Mathf.Max(safeMin, maxTiles), tiles.Count);
        LevelConfig config = GameManager.Instance.config;
        if (config != null && config.HasPaghihigpit(PaghihigpitTypes.MahabangSalita))
            safeMin = Mathf.Max(safeMin, 4);

        List<string> candidates = BuildRecommendationCandidates(config);
        Shuffle(candidates);

        foreach (string candidate in candidates)
        {
            List<Tile> selectedTiles = new List<Tile>();
            bool requireMarkedConsonants = config != null &&
                config.HasPaghihigpit(PaghihigpitTypes.MarkaNgBaybayin);

            if (!TryMatchWord(
                    candidate,
                    0,
                    tiles,
                    selectedTiles,
                    safeMin,
                    safeMax,
                    requireMarkedConsonants))
                continue;

            FormWord(salitaSlots, selectedTiles, candidate);
            return true;
        }

        Debug.LogWarning(
            "Daliri ni Tarabusaw could not form a valid word from the current tiles.",
            this);
        return false;
    }

    // Kept for older UnityEvent/script callers while activation now comes from
    // clicking the equipped Alahas itself.
    public void reccWordFunc(int minTiles, int maxTiles)
    {
        TryFormRecommendedWord(minTiles, maxTiles);
    }

    private static void AddDirectChildTiles(Transform parent, List<Tile> destination)
    {
        foreach (Transform child in parent)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null && !string.IsNullOrEmpty(tile.latinText))
                destination.Add(tile);
        }
    }

    private List<string> BuildRecommendationCandidates(LevelConfig config)
    {
        List<string> candidates = new List<string>();

        if (config != null && config.HasPaghihigpit(PaghihigpitTypes.PartikularNaSalita))
        {
            int currentAksyon = AksyonCounter.Instance != null
                ? AksyonCounter.Instance.GetCurrentAksyon()
                : 1;
            string particularWord = config.GetPartikularNaSalitaForAksyon(currentAksyon);
            if (!string.IsNullOrWhiteSpace(particularWord))
                candidates.Add(particularWord.Trim());
            return candidates;
        }

        bool forbidRepeatedWords = config != null &&
            config.HasPaghihigpit(PaghihigpitTypes.BawalUmulit);

        foreach (string word in GameManager.Instance.validWords)
        {
            if (string.IsNullOrWhiteSpace(word)) continue;
            if (forbidRepeatedWords && GameManager.Instance.wordsUsed.Contains(word)) continue;
            candidates.Add(word.Trim());
        }

        return candidates;
    }

    private static bool TryMatchWord(
        string word,
        int characterIndex,
        List<Tile> availableTiles,
        List<Tile> selectedTiles,
        int minTiles,
        int maxTiles,
        bool requireMarkedConsonants)
    {
        if (characterIndex == word.Length)
            return selectedTiles.Count >= minTiles && selectedTiles.Count <= maxTiles;

        if (selectedTiles.Count >= maxTiles) return false;

        foreach (Tile tile in availableTiles)
        {
            if (tile == null || selectedTiles.Contains(tile)) continue;
            if (requireMarkedConsonants && !tile.isVowel &&
                tile.GetCurrentCharMod() == Tile.Diacritic.None)
                continue;

            string tileText = tile.latinText;
            if (string.IsNullOrEmpty(tileText) ||
                characterIndex + tileText.Length > word.Length)
                continue;

            if (string.Compare(
                    word,
                    characterIndex,
                    tileText,
                    0,
                    tileText.Length,
                    StringComparison.OrdinalIgnoreCase) != 0)
                continue;

            selectedTiles.Add(tile);
            if (TryMatchWord(
                    word,
                    characterIndex + tileText.Length,
                    availableTiles,
                    selectedTiles,
                    minTiles,
                    maxTiles,
                    requireMarkedConsonants))
                return true;
            selectedTiles.RemoveAt(selectedTiles.Count - 1);
        }

        return false;
    }

    private static void FormWord(
        SalitaSlots salitaSlots,
        List<Tile> selectedTiles,
        string word)
    {
        Transform tileSet = TileSet.Instance.transform;

        for (int i = salitaSlots.transform.childCount - 1; i >= 0; i--)
        {
            Tile tile = salitaSlots.transform.GetChild(i).GetComponent<Tile>();
            if (tile == null) continue;
            tile.transform.SetParent(tileSet, false);
            tile.ChangeSprite(false);
        }

        foreach (Tile tile in selectedTiles)
        {
            tile.transform.SetParent(salitaSlots.transform, false);
            tile.transform.SetAsLastSibling();
            tile.ChangeSprite(true);
        }

        salitaSlots.ShowRecommendedWord(word);

        if (tileSet is RectTransform tileSetRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(tileSetRect);
        if (salitaSlots.transform is RectTransform salitaRect)
            LayoutRebuilder.ForceRebuildLayoutImmediate(salitaRect);
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
