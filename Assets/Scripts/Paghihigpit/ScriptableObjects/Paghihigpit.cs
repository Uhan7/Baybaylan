using UnityEngine;

public enum PaghihigpitTypes
{
    ItinakdangTitik,
    BawalUmulit,
    PartikularNaSalita
}

[CreateAssetMenu]
public class Paghihigpit : ScriptableObject
{
    // Variables ---------------------------------------------------------------
    [Header("Paghihigpit Info")]
    [SerializeField] public PaghihigpitTypes paghihigpitType;
    [SerializeField] public string paghihigpitName = "Pangalan ng Paghihigpit";
    [SerializeField] public Sprite paghihigpitSprite;
    [TextArea(2, 2), SerializeField] public string description = "Deskripsyon tungkol sa Paghihigpit.";
    [TextArea(2, 2), SerializeField] public string extraText = "Extra text tungkol sa Paghihigpit.";
}
