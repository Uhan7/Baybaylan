using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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
    private void Awake()
    {
        config = GameManager.Instance.config;
    }

    private void Start()
    {
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

            if (config.usePredefinedTiles) tile = config.predefinedTiles[i];
            else
            {
                int roll = Random.Range(0, totalChance);

                foreach (GameObject obj in config.tilesSelection)
                {
                    Tile tileComp = obj.GetComponent<Tile>();
                    roll -= tileComp.GetChance();

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
