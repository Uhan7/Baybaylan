using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

class DahonNgKawayanUI : MonoBehaviour
{
    [SerializeField] GameObject tileButtonPrefab;
    [SerializeField] Color selectedTileColor;
    [SerializeField] GameObject tileLayoutGroupParent;
    [SerializeField] GameObject mainUiParent;
    [SerializeField] GameObject spawnButton; //the button to open the main UI 
    public static DahonNgKawayanUI Instance;
    LevelConfig config;
    List<Image> tileImgs = new List<Image>();
    TextMeshProUGUI normalModText;
    TextMeshProUGUI shyModText;
    int totalTilesInSelection; //in hindsight this isnt used
    int tileSelectedIndex = 0;
    int prevSelectedIndex = 0;
    bool spawnedTiles = false; //used by the button spawning func as a flag
    bool usedSpawn = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Button dismissButton = mainUiParent.GetComponent<Button>();
        if (dismissButton == null)
            dismissButton = mainUiParent.AddComponent<Button>();

        dismissButton.transition = Selectable.Transition.None;
        dismissButton.targetGraphic = mainUiParent.GetComponent<Graphic>();
        dismissButton.onClick.RemoveListener(CloseSelection);
        dismissButton.onClick.AddListener(CloseSelection);

        mainUiParent.SetActive(false);
        if (spawnButton != null) spawnButton.SetActive(false);
    }

    void spawnTileButtons()
    {
        if(!config || spawnedTiles)
            return;

        int index = 0;

        foreach(GameObject obj in config.tilesSelection)
        {
            GameObject newButton = Instantiate(tileButtonPrefab, tileLayoutGroupParent.transform);

            Image newImg = newButton.transform.GetChild(2).GetComponent<Image>();
            newImg.sprite = obj.transform.GetChild(2).GetComponent<Image>().sprite;
            if(index == 0)
                newImg.color = selectedTileColor;
            else
                newImg.color = Color.white;
            tileImgs.Add(newImg);

            Button buttonComp = newButton.GetComponent<Button>();
            int temp = new int();
            temp = index;
            buttonComp.onClick.AddListener(() => selectIndex(temp));

            index++;
        } 

        //Debug.Log("final index: " + index + "    max index: " + totalTilesInSelection);

        spawnedTiles = true;
    }

    public void OpenSelection()
    {
        if (usedSpawn || config == null) return;
        if (AlahasSubManager.Instance == null || !AlahasSubManager.Instance.canCreateTile) return;

        spawnTileButtons();
        mainUiParent.SetActive(true);
    }

    // Kept so older scene/prefab event references do not break.
    public void spawnTileButton()
    {
        OpenSelection();
    }

    public void CloseSelection()
    {
        mainUiParent.SetActive(false);
    }

    //used by the finish button to spawn the tile 
    public void finishSelection()
    {
        usedSpawn = true;
        mainUiParent.SetActive(false);
        
        GameObject newTile = config.tilesSelection[tileSelectedIndex];
        TileSet.Instance.DahonNgKawayanSpawn(newTile);
    }

    void selectIndex(int index)
    {
        tileSelectedIndex = index;

        //Debug.Log("pressed index: " + index);
    }

    public void getLevelConfig(LevelConfig config)
    {
        this.config = config;

        totalTilesInSelection = 0;
        foreach(GameObject obj in config.tilesSelection)
            totalTilesInSelection++;
    }

    void Update()
    {
        //updates the selected tile's visuals
        if(tileSelectedIndex != prevSelectedIndex)
        {
            tileImgs[tileSelectedIndex].color = selectedTileColor;
            tileImgs[prevSelectedIndex].color = Color.white;
        }
        prevSelectedIndex = tileSelectedIndex;

    }
}
