using UnityEngine;
using System.Collections.Generic;

//holds the unlocked alahas to be used else where
public class TalaAlahasHolder : MonoBehaviour
{
    [HideInInspector] public static TalaAlahasHolder Instance;
    public List<Alahas> availableAlahas = new List<Alahas>();

    public bool IsUnlocked(Alahas alahas)
    {
        if (!alahas) return false;
        if (availableAlahas != null && availableAlahas.Contains(alahas)) return true;

        if (AlahasManager.Instance != null &&
            AlahasManager.Instance.heldAlahas != null &&
            AlahasManager.Instance.heldAlahas.Contains(alahas))
            return true;

        // Legacy levels can contain more than one manager prefab. Check every
        // surviving scene-authored manager so its unlock list is not hidden by
        // singleton initialization order.
        AlahasManager[] sceneManagers = FindObjectsByType<AlahasManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (AlahasManager manager in sceneManagers)
            if (manager != null && manager.heldAlahas != null &&
                manager.heldAlahas.Contains(alahas))
                return true;

        return false;
    }

    public void Unlock(Alahas alahas)
    {
        if (!alahas) return;
        if (availableAlahas == null) availableAlahas = new List<Alahas>();
        if (!availableAlahas.Contains(alahas)) availableAlahas.Add(alahas);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (availableAlahas != null)
                foreach (Alahas alahas in availableAlahas)
                    Instance.Unlock(alahas);
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
    }
}
