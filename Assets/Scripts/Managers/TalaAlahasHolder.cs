using UnityEngine;
using System.Collections.Generic;

//holds the unlocked alahas to be used else where
class TalaAlahasHolder : MonoBehaviour
{
    [HideInInspector] public static TalaAlahasHolder Instance;
    public List<Alahas> availableAlahas = new List<Alahas>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}