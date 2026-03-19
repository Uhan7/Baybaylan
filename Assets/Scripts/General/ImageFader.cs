using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageFader : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private Image targetImage;

    [Header("Routine References")]
    [HideInInspector] private Coroutine fadeRoutine;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
    }

    // Helper Functions --------------------------------------------------------
    public void FadeTo(float desiredAlpha, float duration)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Fade(desiredAlpha, duration));
    }

    private IEnumerator Fade(float desiredAlpha, float duration)
    {
        float elapsedTime = 0f;

        Color color = targetImage.color;
        float startAlpha = color.a;

        while (elapsedTime <= duration)
        {
            float t = elapsedTime / duration;

            float newAlpha = Mathf.Lerp(startAlpha, desiredAlpha, t);
            targetImage.color = new Color(color.r, color.g, color.b, newAlpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetImage.color = new Color(color.r, color.g, color.b, desiredAlpha);
    }
}