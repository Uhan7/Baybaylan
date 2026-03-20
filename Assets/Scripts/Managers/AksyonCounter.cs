using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

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

    [Header("Aksyon Variables")]
    [SerializeField, ReadOnly] private int currentAksyon = 1;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        //aksyonText.text = currentAksyon.ToString() + "/" + config.maxAksyon;
        SpawnAvailableAksyons();
    }

    // Helper Functions --------------------------------------------------------
    private void SpawnAvailableAksyons()
    {
        for (int i = 0; i < GameManager.Instance.config.maxAksyon; i++)
        {
            Instantiate(availableAksyonPrefab, availableContainer.transform);
            GameObject active = Instantiate(activeAksyonPrefab, activeContainer.transform);
            active.GetComponent<ImageFader>().SetAlpha(0);
        }
    }

    public void ConcludeAksyon()
    {
        // Set the alpha to be visible, -1 because aksyon starts at 1
        Transform child = activeContainer.transform.GetChild(currentAksyon - 1);
        child.GetComponent<ImageFader>().SetAlpha(1);

        // increment
        currentAksyon++;

        // Then check if it exceeded
        if (currentAksyon > GameManager.Instance.config.maxAksyon) GameManager.Instance.EndRound();
    }

    public bool HasRemainingAksyon() // Called on Submit Word
    {
        return currentAksyon <= GameManager.Instance.config.maxAksyon;
    }
}
