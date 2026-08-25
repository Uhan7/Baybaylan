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
    }

    private void OnEnable()
    {
        if (!config) return;
        if (config.usePredefinedTiles) StartCoroutine(SpawnTiles(config.predefinedTiles.Count));
        else StartCoroutine(SpawnTiles(config.tilesAmount));
    }

    // Helper Functions --------------------------------------------------------
    private void SpawnTile(GameObject tilePrefab)
    {
        GameObject tile = Instantiate(tilePrefab, transform);
        tile.GetComponent<Draggable>().canvas = canvas;
        tile.GetComponent<Tile>().sfxSource = sfxSource;
        tile.GetComponent<Draggable>().sfxSource = sfxSource;
    }

    public IEnumerator SpawnTiles(int tilesAmount) // Can be called by SalitaSlots after valid word
    {
        int totalChance = 0;
        foreach (GameObject obj in config.tilesSelection) totalChance += obj.GetComponent<Tile>().GetChance();

        for (int i = 0; i < tilesAmount; i++)
        {
            GameObject tile = null;

            if (config.usePredefinedTiles)
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
                    Tile tileComp = obj.GetComponent<Tile>();
                    int effectiveChance = tileComp.GetChance();

                    if (AlahasManager.Instance.boostVowels && tileComp.isVowel) effectiveChance = Mathf.RoundToInt(effectiveChance * AlahasManager.Instance.vowelChanceMultiplier);

                    roll -= effectiveChance;

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
