using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Collections;

public class AksyonCounter : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static AksyonCounter Instance;

    [Header("GameObject References")]
    [SerializeField] private GameObject availableContainer;
    [SerializeField] private GameObject activeContainer;
    [SerializeField] private GameObject availableAksyonPrefab;
    [SerializeField] private GameObject activeAksyonPrefab;
    [SerializeField] private GameObject kaposAksyonPrefab;

    [Header("Aksyon Variables")]
    [SerializeField, ReadOnly] private int maxAksyon = 0;
    [SerializeField, ReadOnly] private int maxAvailableAksyon = 0;
    [SerializeField, ReadOnly] private int numKaposAksyon = 0;
    [SerializeField, ReadOnly] private int currentAksyon = 1;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(waitForGameStart());
    }

    IEnumerator waitForGameStart()
    {
        yield return new WaitUntil(() => AlahasSubManager.Instance.gameplayStarted);
        
        maxAksyon = GameManager.Instance.config.maxAksyon;
        if(AlahasSubManager.Instance.addExtraTurnAndTile)
            maxAksyon += AlahasSubManager.Instance.extraTurn;
        if (GameManager.Instance.config.HasPaghihigpit(PaghihigpitTypes.KaposNaAksyon))
        {
            numKaposAksyon = GameManager.Instance.config.numKaposAksyon;
            maxAvailableAksyon = (maxAksyon - numKaposAksyon);
        }
        else
        {
            maxAvailableAksyon = maxAksyon;
        }

        //aksyonText.text = currentAksyon.ToString() + "/" + config.maxAksyon;
        SpawnAvailableAksyons();
        GameManager.Instance.InvokeEventsOnAksyon(currentAksyon);
    }

    // Helper Functions --------------------------------------------------------
    private void SpawnAvailableAksyons()
    {
        for (int i = 0; i < maxAvailableAksyon; i++)
        {
            Instantiate(availableAksyonPrefab, availableContainer.transform);
            GameObject active = Instantiate(activeAksyonPrefab, activeContainer.transform);
            active.GetComponent<ImageFader>().SetAlpha(0);
        }
        for (int i = 0; i < numKaposAksyon; i++)
        {
            Instantiate(availableAksyonPrefab, availableContainer.transform);
            Instantiate(kaposAksyonPrefab, activeContainer.transform);
        }
    }

    public void ConcludeAksyon()
    {
        // Set the alpha to be visible, -1 because aksyon starts at 1
        Transform child = activeContainer.transform.GetChild(currentAksyon - 1);
        child.GetComponent<ImageFader>().SetAlpha(1);

        // increment
        currentAksyon++;

        if (currentAksyon <= maxAvailableAksyon && MahikaManager.Instance.GetMahikaPercent() < 1f) GameManager.Instance.InvokeEventsOnAksyon(currentAksyon);
    }

    public bool HasRemainingAksyon() // Called on Submit Word
    {
        return currentAksyon <= maxAvailableAksyon;
    }

    public int GetCurrentAksyon()
    {
        return currentAksyon;
    }
}
