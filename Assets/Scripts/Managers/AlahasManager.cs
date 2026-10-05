using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;
using System;
using System.Linq;

//holds the equiped alahas, submanager handles the functionality 
public class AlahasManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Singleton")]
    [HideInInspector] public static AlahasManager Instance;

    [SerializeField] public int maxAlahasSlotCount;

    [Header("Alahas References")]
    [SerializeField] private GameObject[] alahasSlots;
    [SerializeField] private TextMeshProUGUI alahasNameText;
    [SerializeField] private TextMeshProUGUI alahasDescriptionText;

    [Header("Current Alahas")]
    [SerializeField] public List<Alahas> heldAlahas;
    [HideInInspector] public int currentAlahasIndex = 0;

    private readonly Dictionary<Alahas, int> remainingActivations = new Dictionary<Alahas, int>();

    [Header("Stat Upgrades")]
    // [ReadOnly, SerializeField] public float goldenTileChance = 0;
    // [ReadOnly, SerializeField] public bool boostVowels = false;

    [Header("Other Alahas Info")] // NOTE THAT THE CHANGES WE USE ARE IN INSPECTOR... PROBABLY CHANGE SOON
    // [SerializeField] public float goldenTileMultiplier = 2f;
    // [SerializeField] public float vowelScoreMultiplier = 4f;
    // [SerializeField] public float vowelChanceMultiplier = 4f;

    AlahasInfoPopup alahasInfoPopupScript;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        // if (alahasSlots != null && alahasSlots.Length > 0) Instance.alahasSlots = this.alahasSlots;
        // if (alahasNameText != null) Instance.alahasNameText = this.alahasNameText;
        // if (alahasDescriptionText != null) Instance.alahasDescriptionText = this.alahasDescriptionText;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResetAllAlahas(); //does nothing rn 
        ResetActivationCounts();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check for the presence of the game manager in the scene
        GameManager gameManagerInstance = GameObject.FindFirstObjectByType<GameManager>();
        if (!gameManagerInstance) return;

        ResetActivationCounts();

        CacheAlahasSlots();
        
        // I forgot where I saw this (probably Kotlin or something), you can put a '?' before the '.' to check if it is null
        alahasNameText = GameObject.FindGameObjectWithTag("Alahas Name Text")?.GetComponent<TextMeshProUGUI>();
        alahasDescriptionText = GameObject.FindGameObjectWithTag("Alahas Description Text")?.GetComponent<TextMeshProUGUI>();

        alahasInfoPopupScript = GameObject.FindObjectOfType<AlahasInfoPopup>(true);

        RefreshAlahasSlotsUI();
    }

    // Helper Functions --------------------------------------------------------
    public void RefreshAlahasSlotsUI()
    {
        // The HUD begins inactive while the intro dialogue runs, so the usual
        // tag lookup misses every slot. Include inactive popup components so
        // their images are ready when the HUD is activated.
        CacheAlahasSlots();

        foreach (GameObject slot in alahasSlots)
        {
            AlahasInfoPopup popup = slot != null ? slot.GetComponent<AlahasInfoPopup>() : null;
            if (popup != null) popup.SetAlahas(null);
        }

        if (heldAlahas == null) return;

        int visibleAlahasCount = Mathf.Min(heldAlahas.Count, alahasSlots.Length);
        for (int i = 0; i < visibleAlahasCount; i++)
        {
            if (!heldAlahas[i]) continue;

            if (alahasSlots[i] == null) continue;

            AlahasInfoPopup popup = alahasSlots[i].GetComponent<AlahasInfoPopup>();
            if (popup != null) popup.SetAlahas(heldAlahas[i]);

            // alahasSlots[index].GetComponent<Button>().onClick.RemoveAllListeners();
            // alahasSlots[index].GetComponent<Button>().onClick.AddListener(() => 
            // { 
            //     ChangeDescriptionUI(heldAlahas[index]); 
            //     alahasInfoPopupScript.openPopup();
            // });
        }
    }

    private void CacheAlahasSlots()
    {
        alahasSlots = FindObjectsByType<AlahasInfoPopup>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None)
            .Where(popup => popup.gameObject.scene.IsValid() && popup.CompareTag("Alahas Slot"))
            .OrderBy(popup => popup.gameObject.name)
            .Select(popup => popup.gameObject)
            .ToArray();
    }

    private void ChangeDescriptionUI(Alahas selectedAlahas)
    {
        if (alahasNameText != null) alahasNameText.text = selectedAlahas.alahasName;
        if (alahasDescriptionText != null) alahasDescriptionText.text = selectedAlahas.description;
    }

    private void ResetAllAlahas()
    {
        //goldenTileChance = 0;
        //boostVowels = false;
    }

    private void ResetActivationCounts()
    {
        remainingActivations.Clear();
        if (heldAlahas == null) return;

        foreach (Alahas alahas in heldAlahas)
        {
            if (!alahas || remainingActivations.ContainsKey(alahas)) continue;
            remainingActivations.Add(alahas, Mathf.Max(0, alahas.maximumActivations));
        }
    }

    public int GetRemainingActivations(Alahas alahas)
    {
        if (!alahas) return 0;

        if (!remainingActivations.TryGetValue(alahas, out int remaining))
        {
            remaining = Mathf.Max(0, alahas.maximumActivations);
            remainingActivations.Add(alahas, remaining);
        }

        return remaining;
    }

    public bool CanActivate(Alahas alahas)
    {
        return GetRemainingActivations(alahas) > 0;
    }

    public bool TryConsumeActivation(Alahas alahas)
    {
        int remaining = GetRemainingActivations(alahas);
        if (remaining <= 0) return false;

        remainingActivations[alahas] = remaining - 1;
        RefreshAlahasSlotsUI();
        return true;
    }

    public int getEmptySlotAmount()
    {
        int occupiedSlotCount = heldAlahas?.Count(alahas => alahas) ?? 0;
        int slotCapacity = maxAlahasSlotCount > 0
            ? maxAlahasSlotCount
            : alahasSlots?.Length ?? 0;

        return Mathf.Max(0, slotCapacity - occupiedSlotCount);
    }
}
