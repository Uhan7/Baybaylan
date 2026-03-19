using UnityEngine;

[CreateAssetMenu]
public class Alahas : ScriptableObject
{
    // Variables ---------------------------------------------------------------
    [Header("Alahas Info")]
    [SerializeField] private string alahasName = "Pangalan ng Alahas";
    [TextArea(2, 2), SerializeField] private string description = "Deskripsyon tungkol sa Alahas.";

    [Header("Alahas Properties")]
    [SerializeField] public float something;
}
