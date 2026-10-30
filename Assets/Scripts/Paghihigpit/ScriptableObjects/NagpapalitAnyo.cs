using UnityEngine;
using UnityEngine.UI;
using System;

[CreateAssetMenu(menuName = "Paghihigpit/Nagpapalit-anyo")]
class NagpapalitAnyoPaghihigpit : Paghihigpit
{
    [Serializable] 
    public struct Bosses
    {
        public string name;
        public Sprite sprite;
        public Paghihigpit[] paghihigpit;
    }

    [Header("Include the current boss in index 0")]
    [SerializeField] public Bosses[] bosses;
}