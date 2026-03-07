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
        if (config.usePredefinedTiles) StartCoroutine(SpawnTiles(config.predefinedTiles.Count));
        else StartCoroutine(SpawnTiles(config.tilesAmount));
    }

    // Helper Functions --------------------------------------------------------
    private void SpawnTile(GameObject tilePrefab)
    {
        GameObject tile = Instantiate(tilePrefab, transform);
        tile.GetComponent<Draggable>().canvas = canvas;
    }

    public IEnumerator SpawnTiles(int tilesAmount) // Can be called by Salita Slots after valid word
    {
        for (int i = 0; i < tilesAmount; i++)
        {
            GameObject tile;

            if (config.usePredefinedTiles) tile = config.predefinedTiles[i];
            else
            {
                int randomIndex = Random.Range(0, config.tilesSelection.Count);
                tile = config.predefinedTiles[randomIndex];
            }

            SpawnTile(tile);

            yield return new WaitForSeconds(0.2f);
        }
    }
}
