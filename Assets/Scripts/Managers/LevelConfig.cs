using UnityEngine;
using System.Collections.Generic;

using NaughtyAttributes;

[CreateAssetMenu]
public class LevelConfig : ScriptableObject
{
    [Header("Tiles")]
    [HideIf("itinakdangTitik"), SerializeField] public int tilesAmount;
    [HideIf("itinakdangTitik"), SerializeField] public List<GameObject> tilesSelection;
    [ShowIf("itinakdangTitik"), SerializeField] public List<GameObject> predefinedTiles;

    [Header("Mahika")]
    [SerializeField] public int targetMahika = 100;

    [Header("Aksyon")]
    [SerializeField] public int maxAksyon = 5;

    // [Header("Alahas")]

    [Header("Paghihigpit")]
    [SerializeField] public bool itinakdangTitik = false;
    [SerializeField] public bool bawalUmulit = false;
}