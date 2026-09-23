using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DropZone))]
public class TileSet : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Configurations")]
    [HideInInspector] private LevelConfig config;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip spawnSFX; // Should be stored in tile soon

    [Header("References")]
    [SerializeField] private Canvas canvas;

    // Main Functions ----------------------------------------------------------
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
        if (config.itinakdangTitik) StartCoroutine(SpawnTiles(config.predefinedTiles.Count));
        else StartCoroutine(SpawnTiles(config.tilesAmount));
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
        int totalChance = 0;
        foreach (GameObject obj in config.tilesSelection) totalChance += GetSpawnWeight(obj.GetComponent<Tile>());

        for (int i = 0; i < tilesAmount; i++)
        {
            GameObject tile = null;

            if (config.itinakdangTitik)
            {
                foreach (var candidate in config.predefinedTiles)
                {
                    Tile candidateTile = candidate.GetComponent<Tile>();
                    bool isValid = true;

                    foreach (Transform child in transform)
                    {
                        Tile childTile = child.GetComponent<Tile>();

                        if (candidateTile.isVowel && childTile.isVowel)
                        {
                            if (candidateTile.vowel == childTile.vowel)
                            {
                                isValid = false;
                                break;
                            }
                        }
                        else
                        {
                            if (candidateTile.rootConsonant == childTile.rootConsonant)
                            {
                                isValid = false;
                                break;
                            }
                        }
                    }

                    if (isValid)
                    {
                        tile = candidate;
                        break;
                    }
                }
            }
            else
            {
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
            }

            SpawnTile(tile);
            sfxSource.PlayOneShot(spawnSFX);

            yield return new WaitForSeconds(0.08f);
        }
    }
}
