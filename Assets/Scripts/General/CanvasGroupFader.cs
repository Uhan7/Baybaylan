using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupFader : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("Components")]
    [HideInInspector] private CanvasGroup targetGroup;

    [Header("Routine References")]
    [HideInInspector] private Coroutine fadeRoutine;

    [Header("Properties")]
    [SerializeField] private bool fadeOnEnable = false;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        if (targetGroup == null) targetGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (fadeOnEnable)
        {
            SetAlpha(0f);
            FadeTo(1f, 0.25f);
        }
    }

    // Helper Functions --------------------------------------------------------
    public void FadeTo(float desiredAlpha, float duration)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Fade(desiredAlpha, duration));
    }

    public void FadeTo(float desiredAlpha)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Fade(desiredAlpha, 1));
    }

    public void SetAlpha(float newAlpha)
    {
        targetGroup.alpha = newAlpha;
    }

    private IEnumerator Fade(float desiredAlpha, float duration)
    {
        float elapsedTime = 0f;
        float startAlpha = targetGroup.alpha;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            targetGroup.alpha = Mathf.Lerp(startAlpha, desiredAlpha, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        targetGroup.alpha = desiredAlpha;
    }
}