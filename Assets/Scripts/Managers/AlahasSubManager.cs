using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using NaughtyAttributes;

//salita slots for wehn submit button is pressed and scoring 
//tile mods are done in tile sets

//detects and changes gamestates for a clean ish implementation of items/alahas
class AlahasSubManager : MonoBehaviour
{
    //states from other scripts
    [ReadOnly, SerializeField] public bool dialogueEnded = false;
    [ReadOnly, SerializeField] public bool reccButtonPressed = false;

    //states to send to other scripts
    [ReadOnly, SerializeField] public bool boostVowels = false;
    [ReadOnly, SerializeField] public float vowelSpawnChanceIncrease = 0;
    [ReadOnly, SerializeField] public float vowelScoreMulti = 1f;
    [ReadOnly, SerializeField] public bool spawnGolds = false;
    [ReadOnly, SerializeField] public float goldSpawnChance = 0;
    [ReadOnly, SerializeField] public float goldScoreMulti = 1f;
    [ReadOnly, SerializeField] public bool toolTipTiles = false;
    [ReadOnly, SerializeField] public bool canCreateTile = false;
    [ReadOnly, SerializeField] public bool add3ExtraTiles = false;
    [ReadOnly, SerializeField] public int extraTilesToAdd = 0;
    [ReadOnly, SerializeField] public bool delete5Tiles = false;
    [ReadOnly, SerializeField] public int tilesToDelete = 0;
    [ReadOnly, SerializeField] public bool reccWordButtonActive = false;
    //-------------------------------------------
    public static AlahasSubManager Instance;
    AlahasManager alahasManagerScript;
    List<Alahas> heldAlahas;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        alahasManagerScript = AlahasManager.Instance;
        heldAlahas = alahasManagerScript.heldAlahas;

        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i) continue;

            alahas.onStart();
        }
    }

    void Update()
    {
        onUpdate();

        //updates all the needed bools 
        
    }

    public void onSubmit()
    {
        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i) continue;

            alahas.onSubmit();
        }
    }

    public void onTurnEnd()
    {
        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i) continue;

            alahas.onTurnEnd();
        }
    }

    public void onTilesRefreshed()
    {
        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i) continue;

            if (alahas is PakpakNiPahAlahas pakpak)
                pakpak.OnTilesRefreshed();
        }
    }

    void onUpdate()
    {
        for (int i = 0; i < heldAlahas.Count; i++)
        {
            Alahas alahas = heldAlahas[i];
            if (!alahas || heldAlahas.IndexOf(alahas) != i)
                continue;

            alahas.onUpdate();
            if(alahas.triggerCondition() &&
                AlahasManager.Instance != null &&
                AlahasManager.Instance.TryConsumeActivation(alahas))
                alahas.onTriggerEffect();
        }
    }

    public void spawnTiles(int amount)
    {
        StartCoroutine(TileSet.Instance.SpawnTiles(amount));
    }

    public void reccWordFunc(int minTiles, int maxTiles)
    {
        List<Tile> tiles = new List<Tile>();
        int amount = Random.Range(minTiles, maxTiles + 1);

        foreach(Transform child in TileSet.Instance.transform)
        {
            Tile tileScript = child.GetComponent<Tile>();
            tiles.Add(tileScript);
        }

        bool foundWord = false;
        List<Tile> selectedTiles = new List<Tile>();
        List<string> word = new List<string>();
        while(!foundWord)
        {
            //gets 'amount' number of tiles from the tile set
            for(int i = 0; i < amount; i++)
            {
                int index = Random.Range(0, tiles.Count);
                selectedTiles.Add(tiles[index]);
            }
            
            //gets the latin text of each tile into a list
            foreach(Tile tile in selectedTiles)
                word.Add(tile.latinText);

            //goes thru every unique combo for the list of latin text and checks if it is a valid word
            do
            {
                if(GameManager.Instance.validWords.Contains(string.Join("", word)))
                {
                    foundWord = true;
                    break;
                }
            }
            while (NextPermutation(word));

            if(!foundWord)
            {
                selectedTiles.Clear();
                word.Clear();
            }
        }

        string finalWord = string.Join("", word);
        //Debug.Log("Recommended word: " + finalWord);
        ReccomendWordButton.Instance.reccWord = finalWord.ToUpper();
    }

    //by gpt
    static bool NextPermutation<T>(List<T> list) where T : IComparable<T>
    {
        // Find the largest index i where list[i] < list[i + 1]
        int i = list.Count - 2;

        while (i >= 0 && list[i].CompareTo(list[i + 1]) >= 0)
            i--;

        // No more permutations
        if (i < 0)
            return false;

        // Find the largest index j where list[i] < list[j]
        int j = list.Count - 1;

        while (list[i].CompareTo(list[j]) >= 0)
            j--;

        // Swap i and j
        (list[i], list[j]) = (list[j], list[i]);

        // Reverse everything after i
        list.Reverse(i + 1, list.Count - i - 1);

        return true;
    }
}
