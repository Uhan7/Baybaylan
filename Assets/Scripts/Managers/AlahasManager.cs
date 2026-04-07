using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using NaughtyAttributes;

public class AlahasManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Singleton")]
    [HideInInspector] public static AlahasManager Instance;

    [Header("Alahas References")]
    [SerializeField] private GameObject[] alahasSlots;
    [SerializeField] private TextMeshProUGUI alahasNameText;
    [SerializeField] private TextMeshProUGUI alahasDescriptionText;

    [Header("Current Alahas")]
    [ReadOnly, SerializeField] public List<Alahas> heldAlahas;
    [HideInInspector] public int currentAlahasIndex = 0;

    [Header("Stat Upgrades")]
    [ReadOnly, SerializeField] public float goldenTileChance = 0;
    [ReadOnly, SerializeField] public bool boostVowels = false;

    [Header("Other Alahas Info")]
    [SerializeField] public float goldenTileMultiplier = 2.5f;
    [SerializeField] public float vowelScoreMultiplier = 5f;
    [SerializeField] public float vowelChanceMultiplier = 400f;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // if (alahasSlots != null && alahasSlots.Length > 0) Instance.alahasSlots = this.alahasSlots;
            // if (alahasNameText != null) Instance.alahasNameText = this.alahasNameText;
            // if (alahasDescriptionText != null) Instance.alahasDescriptionText = this.alahasDescriptionText;

            Destroy(gameObject);
            return;
        }

        Instance = this;

        SceneManager.sceneLoaded += OnSceneLoaded;

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // PLEASE CHANGE THIS... THIS IS HORRIBLE CODING... BUT I AM TIRED
        if (scene.name == "Alahas Scene") return;

        //alahasSlots = GameObject.FindGameObjectsWithTag("Alahas Slot");
        alahasSlots[0] = GameObject.Find("Alahas Box 1");
        alahasSlots[1] = GameObject.Find("Alahas Box 2");
        alahasSlots[2] = GameObject.Find("Alahas Box 3");
        alahasNameText = GameObject.Find("Alahas Name Text").GetComponent<TextMeshProUGUI>();
        alahasDescriptionText = GameObject.Find("Alahas Description Text").GetComponent<TextMeshProUGUI>();
        // END OF THE HORRIBLE THING

        if (heldAlahas != null && heldAlahas.Count > 0) SetAlahasSlotsUI();
    }

    // Helper Functions --------------------------------------------------------
    private void SetAlahasSlotsUI()
    {
        for (int i = 0; i < heldAlahas.Count; i++)
        {
            int index = i;

            alahasSlots[index].transform.GetChild(0).GetComponent<Image>().sprite = heldAlahas[index].alahasSprite;

            alahasSlots[index].GetComponent<Button>().onClick.RemoveAllListeners();
            alahasSlots[index].GetComponent<Button>().onClick.AddListener(() => { ChangeDescriptionUI(heldAlahas[index]); });
        }
    }

    private void ChangeDescriptionUI(Alahas selectedAlahas)
    {
        if (alahasNameText != null) alahasNameText.text = selectedAlahas.alahasName;
        if (alahasDescriptionText != null) alahasDescriptionText.text = selectedAlahas.description;
    }
}
