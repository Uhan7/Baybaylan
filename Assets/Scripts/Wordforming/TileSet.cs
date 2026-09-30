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
        SpawnInitialTiles();
    }

    private void OnEnable()
    {
        if (!config) return;
        SpawnInitialTiles();
    }

    private void SpawnInitialTiles()
    {
        int tilesAmount = config.tilesAmount;
        if(AlahasSubManager.Instance.add3ExtraTiles)
            tilesAmount += AlahasSubManager.Instance.extraTilesToAdd;

        StartCoroutine(SpawnTiles(tilesAmount));
    }

    void Update()
    {
        if(AlahasSubManager.Instance.delete5Tiles)
            PakpakNiPahDelete();
    }

    // Helper Functions --------------------------------------------------------
    private void SpawnTile(GameObject tilePrefab)
    {
        GameObject tile = Instantiate(tilePrefab, transform);
        Tile tileScript = tile.GetComponent<Tile>();
        tile.GetComponent<Draggable>().canvas = canvas;
        tileScript.sfxSource = sfxSource;
        tile.GetComponent<Draggable>().sfxSource = sfxSource;

        applyVowelBoost(tileScript);
        applyGold(tileScript);
        applyToolTip(tileScript);
    }

    void PakpakNiPahDelete()
    {
        for(int i = 0; i < AlahasSubManager.Instance.tilesToDelete; i++)
            if(transform.childCount > 0)
                deleteTile(0, true);
    }

    void deleteTile(int childIndex, bool isRandom)
    {
        int index = childIndex;
        if(isRandom)
            index = Random.Range(0, transform.childCount);

        Destroy(transform.GetChild(index).gameObject);
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

            SpawnTile(tile);
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
