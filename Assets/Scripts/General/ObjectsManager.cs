using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ObjectsManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Objects Reference")]
    [SerializeField] private GameObject[] objectsToActivate;
    [SerializeField] private GameObject[] objectsToDeactivate;

    // Main Functions ----------------------------------------------------------

    // Helper Functions --------------------------------------------------------
    public void ActivateObjects()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj == null) continue;
            obj.SetActive(true);
        }
    }

    public void DeactivateObjects()
    {
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj == null) continue;
            obj.SetActive(false);
        }
    }
}
