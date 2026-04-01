using UnityEngine;

public abstract class Alahas : ScriptableObject
{
    // Variables ---------------------------------------------------------------
    [Header("Alahas Info")]
    [SerializeField] private string alahasName = "Pangalan ng Alahas";
    [SerializeField] private Sprite alahasSprite;
    [TextArea(2, 2), SerializeField] private string description = "Deskripsyon tungkol sa Alahas.";

    // Helper Functions --------------------------------------------------------
    public abstract void ApplyAlahas(AlahasManager manager);
}
