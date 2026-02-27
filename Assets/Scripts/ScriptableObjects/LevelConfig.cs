using UnityEngine;
using System.Collections.Generic;

using NaughtyAttributes;

[CreateAssetMenu]
public class LevelConfig : ScriptableObject
{
    // [Header("Tile Slots")]
    // [SerializeField] private int tileSlotsAmount = 8;

    [Header("Tiles")]
    [SerializeField] public bool usePredefinedTiles;
    [HideIf("usePredefinedTiles"), SerializeField] public int tilesAmount;
    [HideIf("usePredefinedTiles"), SerializeField] public List<GameObject> tilesSelection;
    [ShowIf("usePredefinedTiles"), SerializeField] public List<GameObject> predefinedTiles;
}