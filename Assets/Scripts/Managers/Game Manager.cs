using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;

public class GameManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static GameManager Instance;

    [Header("Configurations")]
    [SerializeField] public LevelConfig config; // References whole game

    [Header("Mahika")]
    [SerializeField] private int currentMahika = 0;
    [ReadOnly, SerializeField] private int targetMahika;
    [ReadOnly, SerializeField] public float mahikaPercent; // Used in BackgroundsManager.cs
    [SerializeField] private TextMeshProUGUI mahikaText;
    [SerializeField] private Image mahikaBarFill;

    [Header("Aksyon")]
    [SerializeField] public int currentAksyon = 1;
    [SerializeField] private TextMeshProUGUI aksyonText;

    [Header("Wordlists")]
    [SerializeField] private TextAsset[] wordlists;
    [HideInInspector] public HashSet<string> validWords = new HashSet<string>();
    [SerializeField] public List<string> wordsUsed = new List<string>();

    [Header("Other Screens")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

    // Main Functions ----------------------------------------------------------
    private void OnValidate()
    {
        targetMahika = config.targetMahika;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadWordlist();
    }

    private void Start()
    {
        ChangeMahika(0);
        StartAksyon();
    }

    // Helper Functions --------------------------------------------------------
    public void ChangeMahika(int score)
    {
        currentMahika += score;
        if (currentMahika >= targetMahika) currentMahika = targetMahika;

        mahikaPercent = (float)currentMahika / config.targetMahika;
        mahikaBarFill.fillAmount = mahikaPercent;

        mahikaText.text = currentMahika.ToString() + "/" + config.targetMahika.ToString();
    }

    public void StartAksyon()
    {
        // dialogues start... blablabla
        aksyonText.text = currentAksyon.ToString() + "/" + config.maxAksyon;
    }

    public void ConcludeAksyon()
    {
        currentAksyon++;
        if (currentAksyon <= config.maxAksyon) aksyonText.text = currentAksyon.ToString() + " / " + config.maxAksyon;

        if (currentMahika >= config.targetMahika) EndRound();
    }

    private void LoadWordlist()
    {
        foreach (TextAsset wordlist in wordlists)
        {
            string[] words = wordlist.text.Split("\n");
            foreach (string word in words) validWords.Add(word);
        }
    }

    public void EndRound()
    {
        bool didWin = currentMahika >= config.targetMahika;

        BackgroundsManager.Instance.ShowEndingBG(didWin);

        if (didWin == true)
        {
            winScreen.SetActive(true);
        }

        else
        {
            loseScreen.SetActive(true);
        }
    }
}