using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static GameManager Instance;

    [Header("Score")]
    [SerializeField] private int currentMahika = 0;
    [SerializeField] private int targetMahika = 100;
    [SerializeField] private TextMeshProUGUI mahikaText;
    [SerializeField] private Image mahikaBarFill;

    [Header("Wordlist")]
    [SerializeField] private TextAsset wordlist;
    [HideInInspector] public HashSet<string> validWords = new HashSet<string>();

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
        mahikaBarFill.fillAmount = (float) currentMahika / targetMahika;

        mahikaText.text = currentMahika.ToString() + "/" + targetMahika.ToString();
    }

    private void LoadWordlist()
    {
        string[] words = wordlist.text.Split("\n");

        foreach (string word in words) validWords.Add(word);
    }
}