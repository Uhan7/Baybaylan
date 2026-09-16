using UnityEngine;

public abstract class Alahas : ScriptableObject
{
    // Variables ---------------------------------------------------------------
    [Header("Alahas Info")]
    [SerializeField] public string alahasName = "Pangalan ng Alahas";
    [SerializeField] public Sprite alahasSprite;
    [TextArea(2, 2), SerializeField] public string description = "Deskripsyon tungkol sa Alahas.";
    [TextArea(2, 2), SerializeField] public string extraText = "Extra text tungkol sa Alahas.";

    // Helper Functions --------------------------------------------------------
    public abstract void ApplyAlahas();
    public abstract void RemoveAlahas();

    public void AddAlahasToList()
    {
        AlahasManager.Instance.heldAlahas.Add(this);
    }

    public void RemoveAlahasFromList()
    {
        AlahasManager.Instance.heldAlahas.Remove(this);
    }
}