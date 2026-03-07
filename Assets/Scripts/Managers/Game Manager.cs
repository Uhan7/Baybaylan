using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static GameManager Instance;

    [Header("Configurations")]
    [SerializeField] public LevelConfig config; // References whole game

    [Header("Score")]
    [SerializeField] private int currentMahika = 0;
    [SerializeField] private TextMeshProUGUI mahikaText;
    [SerializeField] private Image mahikaBarFill;

    [Header("Wordlist")]
    [SerializeField] private TextAsset wordlist;
    [HideInInspector] public HashSet<string> validWords = new HashSet<string>();
    [SerializeField] public List<string> wordsUsed = new List<string>();

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadWordlist();
    }

    private void Start()
    {
        ChangeMahika(0);
    }

    // Helper Functions --------------------------------------------------------
    public void ChangeMahika(int score)
    {
        currentMahika += score;
        mahikaBarFill.fillAmount = (float) currentMahika / config.targetMahika;

        mahikaText.text = currentMahika.ToString() + "/" + config.targetMahika.ToString();
    }

    private void LoadWordlist()
    {
        string[] words = wordlist.text.Split("\n");

        foreach (string word in words) validWords.Add(word);
    }
}