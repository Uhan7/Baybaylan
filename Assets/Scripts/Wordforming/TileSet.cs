using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

[RequireComponent(typeof(DropZone))]
public class TileSet : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    public static TileSet Instance;

    [Header("Constants")]
    [HideInInspector] private const float SPAWN_TIME_BETWEEN_TILES = 0.15f;
    [HideInInspector] private const int m_hanginHabagatNumTilesChanged = 3;

    [Header("Configurations")]
    [HideInInspector] private LevelConfig config;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip spawnSFX; // Should be stored in tile soon
    [SerializeField] private AudioClip despawnSFX; // Should be stored in tile soon

    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private AlahasSelectionController alahasSelectionPrefab;

    [Header("Alahas Selection Flow")]
    [Tooltip("Wait for the first Dialogue Set to call ShowAlahasSelection before gameplay begins.")]
    [SerializeField] private bool waitForFirstDialogue = true;

    private bool waitingForDialogueBeforeCurrentAksyonTiles;
    private Coroutine dialogueReleasedSpawnRoutine;
    private Coroutine pakpakDeleteRoutine;
    private readonly List<Tile> pakpakFadeTargets = new List<Tile>();
    private int pakpakNormalTilesRemoved;
    private float pakpakTemporaryTileSpawnDelay = 0.5f;
    private bool gameplayStarted;

    public bool WaitingForDialogueBeforeCurrentAksyonTiles => waitingForDialogueBeforeCurrentAksyonTiles;

    // Main Functions ----------------------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Draggable.SetInteractionLocked(false);
        config = GameManager.Instance.config;

        if (waitForFirstDialogue) return;

        ShowAlahasSelection();
    }

    // UnityEvent entry point for the first Dialogue Set -> Event After Dialogue.
    // Keeping this parameterless makes the Inspector hookup reliable.
    public void ShowAlahasSelection()
    {
        if (gameplayStarted) return;

        if (!AlahasSelectionController.IsSelectionAllowedInActiveScene())
        {
            Debug.LogWarning(
                $"[Alahas Selection] BLOCKED TileSet.ShowAlahasSelection UnityEvent " +
                $"in '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'.",
                this);
            return;
        }

        SceneController sceneController = FindFirstObjectByType<SceneController>(
            FindObjectsInactive.Include);
        if (sceneController != null && !sceneController.AllowsAlahasSelection)
        {
            Debug.Log(
                $"[Alahas Selection] Ignored TileSet.ShowAlahasSelection UnityEvent " +
                $"in '{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}' because " +
                $"Allow Alahas Selection is disabled on SceneController.",
                this);
            return;
        }

        Debug.Log(
            $"[Alahas Selection] OPEN requested by TileSet.ShowAlahasSelection. " +
            $"Expected source: the gameplay scene's first Dialogue Set -> Event After Dialogue.",
            this);

        if (AlahasSelectionController.TryBeginSelection(this)) return;

        if (alahasSelectionPrefab != null && canvas != null)
        {
            Canvas parentCanvas = canvas.rootCanvas != null ? canvas.rootCanvas : canvas;
            AlahasSelectionController selector = Instantiate(
                alahasSelectionPrefab,
                parentCanvas.transform);
            selector.BeginSelection(this);
            return;
        }

        Debug.LogWarning(
            "No Alahas selection screen was found or assigned; starting gameplay directly.",
            this);
        BeginGameplay();
    }

    public void BeginGameplay()
    {
        if (gameplayStarted) return;
        gameplayStarted = true;

        if (AlahasSubManager.Instance != null)
            AlahasSubManager.Instance.BeginGameplay();

        StartCoroutine(SpawnInitialTiles());
    }

    private IEnumerator SpawnInitialTiles()
    {
        // Let the Alahas sub-manager initialize first, regardless of component
        // Start order, then add Pakpak's temporary tiles after the normal pool.
        yield return null;
        int extraTiles = AlahasSubManager.Instance != null ? AlahasSubManager.Instance.extraTile : 0;
        yield return SpawnTiles(config.tilesAmount + extraTiles);
        yield return SpawnPakpakTemporaryTiles();
    }

    // Helper Functions --------------------------------------------------------
    private void SpawnTile(GameObject tilePrefab, bool isTemporary = false)
    {
        GameObject tile = Instantiate(tilePrefab, transform);
        Tile tileScript = tile.GetComponent<Tile>();
        tileScript.isTemp = isTemporary;
        tile.GetComponent<Draggable>().canvas = canvas;
        tileScript.sfxSource = sfxSource;
        tile.GetComponent<Draggable>().sfxSource = sfxSource;

        applyVowelBoost(tileScript);
        applyGold(tileScript);
        applyToolTip(tileScript);
    }

    public void StartPakpakTileCountdown(
        int amount,
        float fadeDuration,
        AnimationCurve fadeCurve,
        float deleteInterval)
    {
        if (pakpakDeleteRoutine != null) return;

        List<Tile> availableTiles = new List<Tile>();
        foreach (Transform child in transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null) availableTiles.Add(tile);
        }

        pakpakFadeTargets.Clear();
        int targetCount = Mathf.Min(amount, availableTiles.Count);
        for (int i = 0; i < targetCount; i++)
        {
            int index = Random.Range(0, availableTiles.Count);
            Tile target = availableTiles[index];
            availableTiles.RemoveAt(index);
            target.BeginPakpakFade();
            pakpakFadeTargets.Add(target);
        }

        pakpakDeleteRoutine = StartCoroutine(PakpakCountdown(
            fadeDuration,
            fadeCurve,
            deleteInterval));
    }

    public void CancelPakpakTileCountdown()
    {
        if (pakpakDeleteRoutine != null)
        {
            StopCoroutine(pakpakDeleteRoutine);
            pakpakDeleteRoutine = null;
        }

        foreach (Tile tile in pakpakFadeTargets)
            if (tile != null) tile.CancelPakpakFade();

        pakpakFadeTargets.Clear();
    }

    private IEnumerator PakpakCountdown(
        float fadeDuration,
        AnimationCurve fadeCurve,
        float deleteInterval)
    {
        float elapsed = 0f;
        float safeDuration = Mathf.Max(0f, fadeDuration);

        while (elapsed < safeDuration)
        {
            elapsed += Time.deltaTime;
            float linearProgress = safeDuration > 0f ? elapsed / safeDuration : 1f;
            float fadeProgress = fadeCurve != null
                ? Mathf.Clamp01(fadeCurve.Evaluate(linearProgress))
                : linearProgress;

            foreach (Tile tile in pakpakFadeTargets)
                if (tile != null) tile.SetPakpakFadeProgress(fadeProgress);

            yield return null;
        }

        foreach (Tile tile in pakpakFadeTargets)
        {
            if (tile == null) continue;

            if (tile != null && !tile.isTemp)
                pakpakNormalTilesRemoved++;

            Destroy(tile.gameObject);

            if (deleteInterval > 0f)
                yield return new WaitForSeconds(deleteInterval);
            else
                yield return null;
        }

        pakpakFadeTargets.Clear();
        pakpakDeleteRoutine = null;
    }

    public IEnumerator RemovePakpakTemporaryTiles()
    {
        bool removedAny = false;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Tile tile = transform.GetChild(i).GetComponent<Tile>();
            if (tile == null || !tile.isTemp) continue;

            Destroy(tile.gameObject);
            removedAny = true;
        }

        if (removedAny)
            yield return null;
    }

    public int ConsumePakpakNormalTilesRemoved()
    {
        int amount = pakpakNormalTilesRemoved;
        pakpakNormalTilesRemoved = 0;
        return amount;
    }

    public IEnumerator SpawnPakpakTemporaryTiles()
    {
        if (AlahasSubManager.Instance == null ||
            !AlahasSubManager.Instance.add3ExtraTiles ||
            AlahasSubManager.Instance.extraTilesToAdd <= 0)
            yield break;

        if (pakpakTemporaryTileSpawnDelay > 0f)
            yield return new WaitForSeconds(pakpakTemporaryTileSpawnDelay);

        yield return SpawnRandomTiles(AlahasSubManager.Instance.extraTilesToAdd, true);
        AlahasSubManager.Instance.onTilesRefreshed();
    }

    public void DahonNgKawayanSpawn(GameObject tilePrefab)
    {
        GameObject tile = Instantiate(tilePrefab, transform);
        PrepareDahonNgKawayanTile(tile);
    }

    public void PrepareDahonNgKawayanTile(GameObject tile)
    {
        if (tile == null) return;

        Tile tileScript = tile.GetComponent<Tile>();
        Draggable draggable = tile.GetComponent<Draggable>();
        if (tileScript == null || draggable == null) return;

        draggable.canvas = canvas;
        tileScript.sfxSource = sfxSource;
        draggable.sfxSource = sfxSource;

        // Dahon choices always identify their Baybayin letter on hover,
        // independently of whether Balahibo ni Amihan is equipped.
        tileScript.isToolTipped = true;
        //include whatever func applies shy 
    }

    private int GetSpawnWeight(Tile tile)
    {
        int weight = tile.GetChance();
        if (AlahasSubManager.Instance.boostVowels && tile.isVowel)
            weight = Mathf.RoundToInt(weight * AlahasSubManager.Instance.vowelSpawnChanceIncrease);

        return weight;
    }

    void applyToolTip(Tile script)
    {
        if(AlahasSubManager.Instance.toolTipTiles)
        {
            script.isToolTipped = true;
        }
    }

    void applyVowelBoost(Tile script)
    {
        if(script.isVowel && AlahasSubManager.Instance.boostVowels)
        {
            script.isVowelBoosted = true;
            script.scoreMultiplier *= AlahasSubManager.Instance.vowelScoreMulti;
        }
    }

    void applyGold(Tile script)
    {
        if(AlahasSubManager.Instance.spawnGolds && Random.value < AlahasSubManager.Instance.goldSpawnChance)
        {
            script.isGold = true;
            script.scoreMultiplier *= AlahasSubManager.Instance.goldScoreMulti;
        }
    }

    public IEnumerator SpawnTiles(int _tilesAmount) // Can be called by SalitaSlots after valid word
    {
        int tilesAmount = _tilesAmount;
        if (config.HasPaghihigpit(PaghihigpitTypes.HanginHabagat))
        {
            int remainingTiles = GetRemainingTiles();
            int numTilesToRemove;
            if (remainingTiles > 3) numTilesToRemove = 3;
            else numTilesToRemove = remainingTiles;
            
            yield return StartCoroutine(RemoveRandomTiles(numTilesToRemove));
            tilesAmount += numTilesToRemove;
        }

        if (config.HasPaghihigpit(PaghihigpitTypes.ItinakdangTitik))
        {
            int currentAksyon = AksyonCounter.Instance != null
                ? AksyonCounter.Instance.GetCurrentAksyon()
                : 1;

            yield return SpawnItinakdangTiles(currentAksyon);
            yield break;
        }

        yield return SpawnRandomTiles(tilesAmount, false);
    }

    private IEnumerator SpawnRandomTiles(int tilesAmount, bool isTemporary)
    {

        int totalChance = 0;
        foreach (GameObject obj in config.tilesSelection) totalChance += GetSpawnWeight(obj.GetComponent<Tile>());

        if (totalChance <= 0)
        {
            Debug.LogWarning("TileSet cannot spawn random tiles because the configured total spawn chance is zero.", this);
            yield break;
        }

        for (int i = 0; i < tilesAmount; i++)
        {
            GameObject tile = null;

            int roll = Random.Range(0, totalChance);

            foreach (GameObject obj in config.tilesSelection)
            {
                roll -= GetSpawnWeight(obj.GetComponent<Tile>());

                if (roll < 0)
                {
                    tile = obj;
                    break;
                }
            }

            SpawnTile(tile, isTemporary);
            sfxSource.PlayOneShot(spawnSFX);

            yield return new WaitForSeconds(0.08f);
        }
    }

    public IEnumerator ReplaceWithCurrentAksyonTiles()
    {
        yield return ClearTiles();

        int currentAksyon = AksyonCounter.Instance != null
            ? AksyonCounter.Instance.GetCurrentAksyon()
            : 1;

        yield return SpawnItinakdangTiles(currentAksyon);
    }

    public IEnumerator ClearTiles()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Tile>() != null)
                Destroy(child.gameObject);
        }

        // Destroy is applied at the end of the frame. Wait before adding the
        // next tile pool to the same layout group.
        yield return null;
    }

    // Call this first from GameManager -> Event On Aksyon, before starting the
    // dialogue that should hold back this Aksyon's tiles.
    public void WaitForDialogueBeforeCurrentAksyonTiles()
    {
        waitingForDialogueBeforeCurrentAksyonTiles = true;
    }

    // UnityEvent entry point for Dialogue Set -> Event After Dialogue. The
    // coroutine also handles a dialogue that finishes before old tiles clear.
    public void SpawnCurrentAksyonTiles()
    {
        if (dialogueReleasedSpawnRoutine != null) return;
        dialogueReleasedSpawnRoutine = StartCoroutine(SpawnCurrentAksyonTilesWhenReady());
    }

    private IEnumerator SpawnCurrentAksyonTilesWhenReady()
    {
        while (HasTiles())
            yield return null;

        int currentAksyon = AksyonCounter.Instance != null
            ? AksyonCounter.Instance.GetCurrentAksyon()
            : 1;

        yield return SpawnItinakdangTiles(currentAksyon);
        yield return SpawnPakpakTemporaryTiles();

        waitingForDialogueBeforeCurrentAksyonTiles = false;
        dialogueReleasedSpawnRoutine = null;
    }

    private bool HasTiles()
    {
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Tile>() != null)
                return true;
        }

        return false;
    }

    private IEnumerator SpawnItinakdangTiles(int aksyonNumber)
    {
        IReadOnlyList<GameObject> selectedTiles = config.GetItinakdangTilesForAksyon(aksyonNumber);

        if (selectedTiles.Count == 0)
        {
            Debug.LogWarning($"No Itinakdang Titik tiles are configured for Aksyon {aksyonNumber}.", config);
            yield break;
        }

        foreach (GameObject tilePrefab in selectedTiles)
        {
            if (tilePrefab == null)
            {
                Debug.LogWarning($"Aksyon {aksyonNumber} has an empty tile entry. It was skipped.", config);
                continue;
            }

            SpawnTile(tilePrefab);
            sfxSource.PlayOneShot(spawnSFX);
            yield return new WaitForSeconds(SPAWN_TIME_BETWEEN_TILES);
        }
    }

    // Paghihigpit: Hangin Habagat Functions --------------------------------------------------------
    private int GetRemainingTiles()
    {
        return transform.childCount;
    }
    private IEnumerator RemoveRandomTiles(int _numTilesToRemove)
    {
        for (int i = 0; i < _numTilesToRemove; i++)
        {
            // Get Random Tile
            int randomTileIndex = Random.Range(0, GetRemainingTiles());
            Transform randomTransform = transform.GetChild(randomTileIndex);
            GameObject randomTile = randomTransform.gameObject;

            // Play Tile Destruction Animation
            randomTile.TryGetComponent<Tile>(out var tileComponent);
            tileComponent.PlayBlowAwayAnimation();
            sfxSource.PlayOneShot(despawnSFX);
            yield return new WaitForSeconds(5 * SPAWN_TIME_BETWEEN_TILES);

            // Destroy Tile
            Destroy(randomTile);
            yield return new WaitForSeconds(SPAWN_TIME_BETWEEN_TILES);
        }
        yield return new WaitForSeconds(3 * SPAWN_TIME_BETWEEN_TILES);
    }
}
