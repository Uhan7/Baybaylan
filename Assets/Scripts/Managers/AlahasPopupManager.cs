using UnityEngine;
using System;
using System.Collections.Generic;
using NaughtyAttributes;

public class AlahasPopupManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Singleton")]
    [HideInInspector] public static AlahasPopupManager Instance;

    [Header("Actions")]
    [HideInInspector] public Action OnSelectedPopup;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    // Helper Functions --------------------------------------------------------

}
