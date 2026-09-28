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
    [SerializeField] public int targetMahika = 100;

    [Header("Aksyon")]
    [OnValueChanged("UpdateAksyonSettings")]
    [SerializeField] public int maxAksyon = 5;

    [Header("Alahas")]
    [SerializeField] public Alahas alahasAquiredAfterWin;

    [Header("Paghihigpit")]
    [SerializeField] public Paghihigpit itinakdangTitik;
    [SerializeField] public Paghihigpit bawalUmulit;

    [OnValueChanged("UpdateAksyonSettings")]
    [SerializeField] public Paghihigpit partikularNaSalita;

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
        return itinakdangTitik != null;
    }

    bool hasPartikularNaSalita()
    {
        return partikularNaSalita != null;
    }
}
