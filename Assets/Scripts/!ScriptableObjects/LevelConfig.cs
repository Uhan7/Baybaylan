using UnityEngine;
using System.Collections.Generic;

using NaughtyAttributes;

[CreateAssetMenu]
public class LevelConfig : ScriptableObject
{
    [Header("Tiles")]
    [SerializeField] public bool usePredefinedTiles;
    [HideIf("usePredefinedTiles"), SerializeField] public int tilesAmount;
    [HideIf("usePredefinedTiles"), SerializeField] public List<GameObject> tilesSelection;
    [ShowIf("usePredefinedTiles"), SerializeField] public List<GameObject> predefinedTiles;

    [Header("Mahika")]
    [SerializeField] public int targetMahika = 100;

    [Header("Aksyon")]
    [SerializeField] public int maxAksyon = 5;

    // [Header("Alahas")]

    [Header("Paghihigpit")]
    [SerializeField] public bool bawalUmulit = false;
}