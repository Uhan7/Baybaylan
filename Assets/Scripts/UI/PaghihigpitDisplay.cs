using UnityEngine;

class PaghihigpitDisplay : MonoBehaviour
{
    [SerializeField] GameObject layoutParentObj;
    [SerializeField] GameObject tilePrefab;
    LevelConfig config;

    void Start()
    {
        config = GameManager.Instance.config;

        setupTiles();
    }

    void setupTiles()
    {
        spawnTile(config.itinakdangTitik);
        spawnTile(config.bawalUmulit);
        spawnTile(config.partikularNaSalita);
    }

    void spawnTile(Paghihigpit paghihigpit)
    {
        if(!paghihigpit)
            return;

        GameObject newTile = Instantiate(tilePrefab, layoutParentObj.transform);
        newTile.GetComponent<PaghihigpitInfoPopup>().SetPaghihigpit(paghihigpit);
    }
}