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
    [SerializeField] GameObject normalModButton;
    [SerializeField] GameObject shyModButton;
    public static DahonNgKawayanUI Instance;
    LevelConfig config;
    List<Image> tileImgs = new List<Image>();
    TextMeshProUGUI normalModText;
    TextMeshProUGUI shyModText;
    int totalTilesInSelection; //in hindsight this isnt used
    int tileSelectedIndex = 0;
    int prevSelectedIndex = 0;
    bool spawnedTiles = false; //used by the button spawning func as a flag
    bool shySelected = false;
    bool usedSpawn = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        normalModText = normalModButton.GetComponent<TextMeshProUGUI>();
        shyModText = shyModButton.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        mainUiParent.SetActive(false);
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

    //used by the button to open the main UI
    public void spawnTileButton()
    {
        mainUiParent.SetActive(true);
    }

    //used by the mod buttons
    public void modTileButton(bool isShySelected)
    {
        shySelected = isShySelected;
    }

    //used by the finish button to spawn the tile 
    public void finishSelection()
    {
        usedSpawn = true;
        mainUiParent.SetActive(false);
        
        GameObject newTile = config.tilesSelection[tileSelectedIndex];
        TileSet.Instance.DahonNgKawayanSpawn(newTile, shySelected);
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

        //updates the mod button's visuals
        if(shySelected)
        {
            shyModText.color = selectedTileColor;
            normalModText.color = Color.black;
        }
        else
        {
            shyModText.color = Color.black;
            normalModText.color = selectedTileColor;
        }

        //for odd reasons, these funcs are needed in update
        if(!AlahasSubManager.Instance.canCreateTile || usedSpawn)
            spawnButton.SetActive(false);
        else    
            spawnButton.SetActive(true);

        spawnTileButtons();
    }
}