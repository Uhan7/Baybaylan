using UnityEngine;

public class Alahas : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Alahas Info")]
    [SerializeField] private string alahasName = "Pangalan ng Alahas";
    [TextArea(2, 3), SerializeField] private string description = "Deskripsyon tungkol sa Alahas.";

    [Header("Alahas Properties")]
    [SerializeField] private string ambot;
}
