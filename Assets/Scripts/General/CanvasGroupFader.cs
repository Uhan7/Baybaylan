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

    public void FadeIn(float duration)
    {
        targetGroup.interactable = true;
        targetGroup.blocksRaycasts = true;
        FadeTo(1f, duration);
    }

    public void FadeOut(float duration)
    {
        // A transparent CanvasGroup can still intercept UI pointer events. Stop
        // the outgoing UI from blocking the game as soon as its fade begins.
        targetGroup.interactable = false;
        targetGroup.blocksRaycasts = false;
        FadeTo(0f, duration);
    }

    public void FadeOutAndDeactivate(float duration)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        targetGroup.interactable = false;
        targetGroup.blocksRaycasts = false;
        fadeRoutine = StartCoroutine(FadeOutAndDeactivateRoutine(duration));
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

    private IEnumerator FadeOutAndDeactivateRoutine(float duration)
    {
        yield return Fade(0f, duration);
        fadeRoutine = null;
        gameObject.SetActive(false);
    }
}
