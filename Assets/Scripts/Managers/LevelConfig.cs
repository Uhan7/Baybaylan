using UnityEngine;
using System.Collections.Generic;

using NaughtyAttributes;

[System.Serializable]
public class ItinakdangTitikAksyon
{
    [ReadOnly, SerializeField] private int aksyon = 1;
    [SerializeField] public List<GameObject> tiles = new List<GameObject>();

    public void SetAksyonNumber(int value)
    {
        aksyon = value;
    }
}

[CreateAssetMenu]
public class LevelConfig : ScriptableObject
{
    [Header("Tiles")]
    [HideIf("hasItinakdangTitik"), SerializeField] public int tilesAmount;
    [HideIf("hasItinakdangTitik"), SerializeField] public List<GameObject> tilesSelection;
    [ShowIf("hasItinakdangTitik"), Tooltip("Each element is the exact tile pool for its numbered Aksyon."), SerializeField]
    public List<ItinakdangTitikAksyon> itinakdangTitikPerAksyon = new List<ItinakdangTitikAksyon>();

    // Kept serialized so existing level configs continue to work until their
    // per-Aksyon tile lists are filled in.
    [HideInInspector, SerializeField] public List<GameObject> predefinedTiles;

    [Header("Mahika")]
    [OnValueChanged("UpdateAksyonSettings")]
    public bool hasMultipleTargetMahika = false;
    [HideIf("hasMultipleTargetMahika")]
    [OnValueChanged("UpdateAksyonSettings")]
    [SerializeField] public int targetMahika = 100;
    [ShowIf("hasMultipleTargetMahika")]
    [OnValueChanged("UpdateAksyonSettings")]
    [SerializeField] public int[] multipleTargetMahika = new int[1] {100};

    [Header("Aksyon")]
    [OnValueChanged("UpdateAksyonSettings")]
    [SerializeField] public int maxAksyon = 5;
    [ShowIf("hasKaposNaAksyon")]
    [OnValueChanged("UpdateAksyonSettings")]
    [SerializeField] public int numKaposAksyon;

    [Header("Alahas")]
    [Min(1), Tooltip("Number of Alahas slots available to Tala in this level.")]
    [SerializeField] public int alahasSlotCount = 3;
    [SerializeField] public Alahas alahasAquiredAfterWin;

    [Header("Paghihigpit")]
    [Tooltip("Drop every Paghihigpit active in this level here.")]
    [OnValueChanged("UpdateAksyonSettings")]
    [SerializeField] public List<Paghihigpit> activePaghihigpit = new List<Paghihigpit>();

    [ShowIf("hasPartikularNaSalita")] [Header("Partikular na Salita")]
    [SerializeField] public string[] partikularNaSalita_wordList;

    public IReadOnlyList<GameObject> GetItinakdangTilesForAksyon(int aksyonNumber)
    {
        if (!HasPerAksyonTileSetup())
            return predefinedTiles != null
                ? predefinedTiles
                : System.Array.Empty<GameObject>();

        int index = aksyonNumber - 1;
        if (index < 0 || index >= itinakdangTitikPerAksyon.Count)
            return System.Array.Empty<GameObject>();

        ItinakdangTitikAksyon setup = itinakdangTitikPerAksyon[index];
        return setup != null && setup.tiles != null
            ? setup.tiles
            : System.Array.Empty<GameObject>();
    }

    public bool HasPaghihigpit(PaghihigpitTypes type)
    {
        return GetPaghihigpit(type) != null;
    }

    public Paghihigpit GetPaghihigpit(PaghihigpitTypes type)
    {
        if (activePaghihigpit == null) return null;

        foreach (Paghihigpit paghihigpit in activePaghihigpit)
        {
            if (paghihigpit != null && paghihigpit.paghihigpitType == type)
                return paghihigpit;
        }

        return null;
    }

    public string GetPartikularNaSalitaForAksyon(int aksyonNumber)
    {
        if (!HasPaghihigpit(PaghihigpitTypes.PartikularNaSalita) ||
            partikularNaSalita_wordList == null)
            return null;

        int index = aksyonNumber - 1;
        if (index < 0 || index >= partikularNaSalita_wordList.Length)
            return null;

        return partikularNaSalita_wordList[index];
    }

    private bool HasPerAksyonTileSetup()
    {
        if (itinakdangTitikPerAksyon == null) return false;

        foreach (ItinakdangTitikAksyon setup in itinakdangTitikPerAksyon)
        {
            if (setup != null && setup.tiles != null && setup.tiles.Count > 0)
                return true;
        }

        return false;
    }

    private void OnValidate()
    {
        UpdateAksyonSettings();
    }

    private void UpdateAksyonSettings()
    {
        maxAksyon = Mathf.Max(1, maxAksyon);
        alahasSlotCount = Mathf.Max(1, alahasSlotCount);

        // If ever the targetMahika is set to 0 elements, automatically create one entry with targetMahika = 100
        if (multipleTargetMahika.Length <= 0 ) multipleTargetMahika = new int[1] {targetMahika};

        // Logic for Sumpa ng Pitong Ulo
        if (HasPaghihigpit(PaghihigpitTypes.SumpaNgPitongUlo))
        {
            hasMultipleTargetMahika = true;
            maxAksyon = 7;
            if (multipleTargetMahika.Length != 7) multipleTargetMahika = new int[7] {targetMahika, targetMahika, targetMahika, targetMahika, targetMahika, targetMahika, targetMahika};
        }

        // Logic for Kapos na Aksyon
        if (numKaposAksyon >= maxAksyon) numKaposAksyon = Mathf.Max(0, maxAksyon-1);
        if (numKaposAksyon < 0) numKaposAksyon = 0;

        if (activePaghihigpit == null)
            activePaghihigpit = new List<Paghihigpit>();

        if (partikularNaSalita_wordList == null)
            partikularNaSalita_wordList = new string[maxAksyon];
        else if (partikularNaSalita_wordList.Length != maxAksyon)
            System.Array.Resize(ref partikularNaSalita_wordList, maxAksyon);

        if (itinakdangTitikPerAksyon == null)
            itinakdangTitikPerAksyon = new List<ItinakdangTitikAksyon>();

        while (itinakdangTitikPerAksyon.Count < maxAksyon)
            itinakdangTitikPerAksyon.Add(new ItinakdangTitikAksyon());

        while (itinakdangTitikPerAksyon.Count > maxAksyon)
            itinakdangTitikPerAksyon.RemoveAt(itinakdangTitikPerAksyon.Count - 1);

        for (int i = 0; i < itinakdangTitikPerAksyon.Count; i++)
        {
            if (itinakdangTitikPerAksyon[i] == null)
                itinakdangTitikPerAksyon[i] = new ItinakdangTitikAksyon();

            itinakdangTitikPerAksyon[i].SetAksyonNumber(i + 1);
        }
    }

    bool hasItinakdangTitik()
    {
        return HasPaghihigpit(PaghihigpitTypes.ItinakdangTitik);
    }

    bool hasPartikularNaSalita()
    {
        return HasPaghihigpit(PaghihigpitTypes.PartikularNaSalita);
    }

    bool hasKaposNaAksyon()
    {
        return HasPaghihigpit(PaghihigpitTypes.KaposNaAksyon);
    }

    bool hasSumpaNgPitongUlo()
    {
        return HasPaghihigpit(PaghihigpitTypes.SumpaNgPitongUlo);
    }
}
