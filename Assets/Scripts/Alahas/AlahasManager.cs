using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;

public class AlahasManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Singleton")]
    public static AlahasManager Instance;

    [Header("Alahas References")]
    [SerializeField] private GameObject[] alahasSlots;
    //[HideInInspector] private int maxAlahas;

    [Header("Current Alahas")]
    [ReadOnly, SerializeField] public List<Alahas> heldAlahas;
    [HideInInspector] public int currentAlahasIndex = 0;

    [Header("Stat Upgrades")]
    [ReadOnly, SerializeField] public float goldenTileChance = 0;

    [Header("Other Alahas Info")]
    [SerializeField] public float goldenTileMultiplier = 2.5f;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //maxAlahas = alahasSlots.Length;
    }

    // Helper Functions --------------------------------------------------------

}
