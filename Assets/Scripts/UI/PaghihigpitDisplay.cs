using UnityEngine;

class PaghihigpitDisplay : MonoBehaviour
{
    [SerializeField] GameObject layoutParentObj;
    [SerializeField] GameObject tilePrefab;
    LevelConfig config;

    void Start()
    {
        if (GameManager.Instance == null) return;

        config = GameManager.Instance.config;
        if (config == null) return;

        setupTiles();
    }

    void setupTiles()
    {
        if (config.activePaghihigpit == null) return;

        foreach (Paghihigpit paghihigpit in config.activePaghihigpit)
            spawnTile(paghihigpit);
    }

    void spawnTile(Paghihigpit paghihigpit)
    {
        if(!paghihigpit)
            return;

        GameObject newTile = Instantiate(tilePrefab, layoutParentObj.transform);
        newTile.GetComponent<PaghihigpitInfoPopup>().SetPaghihigpit(paghihigpit);
    }
}
