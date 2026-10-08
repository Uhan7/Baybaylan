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
            //Instance.AbsorbSceneLoadout(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // transform.SetParent(null, true);
        // DontDestroyOnLoad(gameObject);

        ResetAllAlahas(); //does nothing rn 

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        getHeldAlahasFromSave();
        ResetActivationCounts();
    }

    void getHeldAlahasFromSave()
    {
        List<Alahas> allAlahas = TalaAlahasHolder.Instance.allAlahas;
        heldAlahas.Clear();
        heldAlahas.AddRange(Enumerable.Repeat(default(Alahas), 6)); 

        for(int i = 0; i < maxAlahasSlotCount; i++)
        {
            switch(i)
            {
                case 0:
                    heldAlahas[i] = getSpecificAlahasFromSave(allAlahas, SaveManager.SaveDataNames.AlahasSlot1);
                    break;
                case 1:
                    heldAlahas[i] = getSpecificAlahasFromSave(allAlahas, SaveManager.SaveDataNames.AlahasSlot2);
                    break;
                case 2:
                    heldAlahas[i] = getSpecificAlahasFromSave(allAlahas, SaveManager.SaveDataNames.AlahasSlot3);
                    break;
                case 3:
                    heldAlahas[i] = getSpecificAlahasFromSave(allAlahas, SaveManager.SaveDataNames.AlahasSlot4);
                    break;
                case 4:
                    heldAlahas[i] = getSpecificAlahasFromSave(allAlahas, SaveManager.SaveDataNames.AlahasSlot5);
                    break;
                case 5:
                    heldAlahas[i] = getSpecificAlahasFromSave(allAlahas, SaveManager.SaveDataNames.AlahasSlot6);
                    break;
            }
        }
    }

    Alahas getSpecificAlahasFromSave(List<Alahas> list, SaveManager.SaveDataNames slotName)
    {
        string data = SaveManager.Instance.getSaveData<string>(slotName);
        foreach(Alahas alahas in list)
        {
            if(alahas.saveDataName.ToString() == data)
                return alahas;
        }

        return null;
    }

    private void AbsorbSceneLoadout(AlahasManager sceneManager)
    {
        if (sceneManager == null) return;

        if (maxAlahasSlotCount <= 0)
            maxAlahasSlotCount = sceneManager.maxAlahasSlotCount;

        bool currentLoadoutHasAlahas = heldAlahas != null && heldAlahas.Any(alahas => alahas);
        bool sceneLoadoutHasAlahas = sceneManager.heldAlahas != null &&
            sceneManager.heldAlahas.Any(alahas => alahas);

        // Several older scenes contain both the persistent manager prefab and
        // a scene-authored manager with the intended starting Alahas. Keep a
        // player's existing selection, but import the authored list when the
        // persistent manager is still empty.
        if (!currentLoadoutHasAlahas && sceneLoadoutHasAlahas)
        {
            heldAlahas = new List<Alahas>(sceneManager.heldAlahas);
            ResetActivationCounts();
        }
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
        ApplyLevelSlotCapacity();
        
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
        ApplyLevelSlotCapacity();

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
            .Where(popup =>
                popup.gameObject.scene.IsValid() &&
                popup.CompareTag("Alahas Slot") &&
                popup.GetComponentInParent<AlahasSelectionController>(true) == null)
            .OrderBy(popup => popup.gameObject.name)
            .Select(popup => popup.gameObject)
            .ToArray();
    }

    private void ApplyLevelSlotCapacity()
    {
        LevelConfig levelConfig = GameManager.Instance != null
            ? GameManager.Instance.config
            : null;
        if (levelConfig != null)
            maxAlahasSlotCount = Mathf.Max(1, levelConfig.alahasSlotCount);

        if (alahasSlots == null || maxAlahasSlotCount <= 0) return;

        for (int i = 0; i < alahasSlots.Length; i++)
            if (alahasSlots[i] != null)
                alahasSlots[i].SetActive(i < maxAlahasSlotCount);
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

    public void SetLoadout(IReadOnlyList<Alahas> selectedSlots)
    {
        if (heldAlahas == null)
            heldAlahas = new List<Alahas>();

        heldAlahas.Clear();

        int slotCapacity = Mathf.Max(0, maxAlahasSlotCount);
        if (slotCapacity == 0)
            slotCapacity = selectedSlots != null ? selectedSlots.Count : 0;

        for (int i = 0; i < slotCapacity; i++)
        {
            Alahas selectedAlahas = selectedSlots != null && i < selectedSlots.Count
                ? selectedSlots[i]
                : null;
            heldAlahas.Add(selectedAlahas);
        }

        ResetActivationCounts();
        RefreshAlahasSlotsUI();
        saveNewAlahasEquip(selectedSlots, slotCapacity);
    }

    void saveNewAlahasEquip(IReadOnlyList<Alahas> selectedSlots, int slotCapacity)
    {
        for(int i = 0; i < 6; i++)
        {
            string name = "";
            if(i < slotCapacity && selectedSlots[i])
                name = selectedSlots[i].saveDataName.ToString();

            switch(i)
            {
                case 0:
                    SaveManager.Instance.saveData(SaveManager.SaveDataNames.AlahasSlot1, name);
                    break;
                case 1:
                    SaveManager.Instance.saveData(SaveManager.SaveDataNames.AlahasSlot2, name);
                    break;
                case 2:
                    SaveManager.Instance.saveData(SaveManager.SaveDataNames.AlahasSlot3, name);
                    break;
                case 3:
                    SaveManager.Instance.saveData(SaveManager.SaveDataNames.AlahasSlot4, name);
                    break;
                case 4:
                    SaveManager.Instance.saveData(SaveManager.SaveDataNames.AlahasSlot5, name);
                    break;
                case 5:
                    SaveManager.Instance.saveData(SaveManager.SaveDataNames.AlahasSlot6, name);
                    break;
            }
        }
    }

    public List<Alahas> GetLoadoutSnapshot(int slotCount)
    {
        int capacity = Mathf.Max(0, slotCount);
        List<Alahas> snapshot = new List<Alahas>(capacity);

        for (int i = 0; i < capacity; i++)
            snapshot.Add(heldAlahas != null && i < heldAlahas.Count ? heldAlahas[i] : null);

        return snapshot;
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
