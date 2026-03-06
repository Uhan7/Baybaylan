using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TileSet : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Configurations")]
    [HideInInspector] private LevelConfig config;

    [Header("References")]
    [SerializeField] private Canvas canvas;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        config = GameManager.Instance.config;
    }

    private void Start()
    {
        if (config.usePredefinedTiles) SpawnPredefinedTiles();
        else SpawnRandomTiles();
    }

    // Helper Functions --------------------------------------------------------
    private void SpawnPredefinedTiles()
    {
        foreach (GameObject tile in config.predefinedTiles)
        {
            GameObject spawnedTile = Instantiate(tile, transform);
            spawnedTile.GetComponent<Draggable>().canvas = canvas;
        }
    }

    private void SpawnRandomTiles()
    {
        for (int i = 0; i < config.tilesAmount; i++)
        {
            int randomIndex = Random.Range(0, config.tilesSelection.Count);
            GameObject tile = Instantiate(config.tilesSelection[randomIndex], transform);
            tile.GetComponent<Draggable>().canvas = canvas;
        }
    }

    public IEnumerator SpawnRandomTiles(int tilesAmount) // Called after valid word
    {
        for (int i = 0; i < tilesAmount; i++)
        {
            int randomIndex = Random.Range(0, config.tilesSelection.Count);
            GameObject tile = Instantiate(config.tilesSelection[randomIndex], transform);
            tile.GetComponent<Draggable>().canvas = canvas;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
