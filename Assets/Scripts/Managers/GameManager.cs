using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;
using System;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    private class AksyonEvent
    {
        public int aksyonNumber = 1;
        public UnityEvent eventOnAksyon;
    }

    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static GameManager Instance;

    [Header("Configurations")]
    [SerializeField] public LevelConfig config; // References whole game

    [Header("Wordlists")]
    [SerializeField] private TextAsset[] wordlists;
    [HideInInspector] public HashSet<string> validWords = new HashSet<string>();
    [SerializeField] public List<string> wordsUsed = new List<string>();

    //[Header("Other Screens")]
    //[SerializeField] private GameObject winScreen;
    //[SerializeField] private GameObject loseScreen;

    [Header("Events")]
    [SerializeField] private UnityEvent eventOnWin;
    [SerializeField] private UnityEvent eventOnLose;
    [SerializeField] private List<AksyonEvent> eventsOnAksyon = new List<AksyonEvent>();

    [Header("Should not be here but idgaf")]
    [SerializeField] private AudioClip winSFX;
    [SerializeField] private AudioClip loseSFX;
    [SerializeField] private AudioClip winAmbience;

    // Main Functions ----------------------------------------------------------

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadWordlist();
    }

    private void Start()
    {
        if (config == null) return;

        // Initialize Mahika Manager
        if (MahikaManager.Instance != null)
            MahikaManager.Instance.Initialize(config);

        if (DahonNgKawayanUI.Instance != null)
            DahonNgKawayanUI.Instance.getLevelConfig(config);
    }

    // Helper Functions --------------------------------------------------------
    public void aquireAlahasAfterWin()
    {
        if(!config.alahasAquiredAfterWin)
            return;

        if (TalaAlahasHolder.Instance != null)
            TalaAlahasHolder.Instance.Unlock(config.alahasAquiredAfterWin);
    }

    public void InvokeEventsOnAksyon(int aksyonNumber)
    {
        foreach (AksyonEvent aksyonEvent in eventsOnAksyon)
        {
            if (aksyonEvent != null && aksyonEvent.aksyonNumber == aksyonNumber)
                aksyonEvent.eventOnAksyon?.Invoke();
        }
    }

    private void LoadWordlist()
    {
        foreach (TextAsset wordlist in wordlists)
        {
            foreach (string rawWord in wordlist.text.Split('\n'))
            {
                string word = rawWord.Trim();
                if (!string.IsNullOrEmpty(word)) validWords.Add(word);
            }
        }
        //print(validWords.Count);
    }

    public void EndRound()
    {
        bool didWin = MahikaManager.Instance.DidWin();

        if (BackgroundsManager.Instance != null) BackgroundsManager.Instance.ShowEndingBG(didWin);

        if (didWin == true)
        {
            AudioManager.Instance.bgmSource.resource = winAmbience;
            AudioManager.Instance.bgmSource.Play();
            AudioManager.Instance.sfxSource.PlayOneShot(winSFX);

            eventOnWin?.Invoke();

            //winScreen.SetActive(true);
        }

        else
        {
            AudioManager.Instance.bgmSource.Stop();
            AudioManager.Instance.sfxSource.PlayOneShot(loseSFX);

            eventOnLose?.Invoke();

            //loseScreen.SetActive(true);
        }
    }
}
