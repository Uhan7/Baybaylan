using UnityEngine;
using UnityEngine.UI;

public class BackgroundsManager : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Instance")]
    [HideInInspector] public static BackgroundsManager Instance;

    [Header("Background Objects")]
    [SerializeField] private GameObject purifiedBackground;
    [SerializeField] private GameObject corruptedBackground;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Helper Functions --------------------------------------------------------
    public void AdjustCorruptedBG()
    {
        float newAlphaValue;
        if (GameManager.Instance.mahikaPercent <= 1) newAlphaValue = (float)(1 - (0.5 * GameManager.Instance.mahikaPercent));
        else newAlphaValue = 1;

        corruptedBackground.GetComponent<ImageFader>().FadeTo(newAlphaValue, 1f);
    }

    public void ShowEndingBG(bool didWin)
    {
        if (didWin) corruptedBackground.GetComponent<ImageFader>().FadeTo(0, 1.5f);
        else corruptedBackground.GetComponent<ImageFader>().FadeTo(1, 1.5f);
    }
}
