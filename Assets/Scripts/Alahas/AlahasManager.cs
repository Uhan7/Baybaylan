using UnityEngine;

public class AlahasManager : MonoBehaviour
{
    public static AlahasManager Instance;

    // Stats to Modify
    public float totalSomething;

    private void Awake()
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
