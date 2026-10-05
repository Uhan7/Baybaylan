using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DropZone))]
public class TileSet : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    public static TileSet Instance;

    [Header("Constants")]
    [HideInInspector] private const float SPAWN_TIME_BETWEEN_TILES = 0.15f;

    [Header("Configurations")]
    [HideInInspector] private LevelConfig config;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip spawnSFX; // Should be stored in tile soon

    [Header("References")]
    [SerializeField] private Canvas canvas;

    private bool waitingForDialogueBeforeCurrentAksyonTiles;
    private Coroutine dialogueReleasedSpawnRoutine;
    private Coroutine pakpakDeleteRoutine;
    private int pakpakNormalTilesRemoved;

    public bool WaitingForDialogueBeforeCurrentAksyonTiles => waitingForDialogueBeforeCurrentAksyonTiles;

    // Main Functions ----------------------------------------------------------
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        config = GameManager.Instance.config;
        StartCoroutine(SpawnInitialTiles());
    }

    private IEnumerator SpawnInitialTiles()
    {
        // Let the Alahas sub-manager initialize first, regardless of component
        // Start order, then add Pakpak's temporary tiles after the normal pool.
        yield return null;
        yield return SpawnTiles(config.tilesAmount);
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

    public void StartPakpakTileDeletion(int amount, float interval)
    {
        if (pakpakDeleteRoutine != null) return;
        pakpakDeleteRoutine = StartCoroutine(DeletePakpakTilesOneByOne(amount, interval));
    }

    private IEnumerator DeletePakpakTilesOneByOne(int amount, float interval)
    {
        for (int i = 0; i < amount && transform.childCount > 0; i++)
        {
            int index = Random.Range(0, transform.childCount);
            Tile tile = transform.GetChild(index).GetComponent<Tile>();

            if (tile != null && !tile.isTemp)
                pakpakNormalTilesRemoved++;

            Destroy(transform.GetChild(index).gameObject);

            if (i < amount - 1)
                yield return interval > 0f ? new WaitForSeconds(interval) : null;
        }

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

        yield return SpawnRandomTiles(AlahasSubManager.Instance.extraTilesToAdd, true);
        AlahasSubManager.Instance.onTilesRefreshed();
    }

    public void DahonNgKawayanSpawn(GameObject tilePrefab)
    {
        GameObject tile = Instantiate(tilePrefab, transform);
        Tile tileScript = tile.GetComponent<Tile>();
        tile.GetComponent<Draggable>().canvas = canvas;
        tileScript.sfxSource = sfxSource;
        tile.GetComponent<Draggable>().sfxSource = sfxSource;

        applyToolTip(tileScript);
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

    public IEnumerator SpawnTiles(int tilesAmount) // Can be called by SalitaSlots after valid word
    {
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
}
