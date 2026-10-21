using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HelperUI : MonoBehaviour
{
    // Variables ---------------------------------------------------------------
    [Header("UI References")]
    [SerializeField] private CanvasGroup promptCanvasGroup;

    [Header("Fade Behavior")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Routine Overwrite")]
    [HideInInspector] private Coroutine FadeRoutine;

    // Main Functions ----------------------------------------------------------
    private void Start()
    {
        HideImmediately();
    }

    // Helper Functions --------------------------------------------------------
    public void FadePromptWrapper(float desiredAlpha)
    {
        if (!gameObject.activeInHierarchy) return;
        if (FadeRoutine != null) StopCoroutine(FadeRoutine);

        FadeRoutine = StartCoroutine(FadePrompt(desiredAlpha));
    }

    public void FadeIn()
    {
        if (!gameObject.activeInHierarchy) return;

        // Tutorial prompts share a parent and are intended to be mutually
        // exclusive. A fast submission can advance to the next prompt before
        // the previous prompt's fade has finished, leaving both visible and
        // making their text/images overlap. Clear the siblings first so only
        // this prompt participates in the incoming fade.
        HideSiblingPromptsImmediately();
        FadePromptWrapper(1);
    }

    public void FadeOut()
    {
        if (!gameObject.activeInHierarchy) return;
        FadePromptWrapper(0);
    }

    public void HideImmediately()
    {
        if (FadeRoutine != null)
        {
            StopCoroutine(FadeRoutine);
            FadeRoutine = null;
        }

        if (promptCanvasGroup != null)
            promptCanvasGroup.alpha = 0f;
    }

    public static void HideAllImmediately()
    {
        HelperUI[] helpers = FindObjectsByType<HelperUI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (HelperUI helper in helpers)
            helper.HideImmediately();
    }

    private void HideSiblingPromptsImmediately()
    {
        Transform promptParent = transform.parent;
        if (promptParent == null) return;

        HelperUI[] siblingHelpers = promptParent.GetComponentsInChildren<HelperUI>(true);
        foreach (HelperUI helper in siblingHelpers)
        {
            if (helper == this || helper.transform.parent != promptParent) continue;
            helper.HideImmediately();
        }
    }

    private IEnumerator FadePrompt(float desiredAlpha)
    {
        if (promptCanvasGroup == null)
        {
            FadeRoutine = null;
            yield break;
        }

        if (fadeDuration <= 0f)
        {
            promptCanvasGroup.alpha = desiredAlpha;
            FadeRoutine = null;
            yield break;
        }

        float elapsedTime = 0f;
        float t = 0;
        float originalCanvasGroupAlpha = promptCanvasGroup.alpha;

        while (elapsedTime <= fadeDuration)
        {
            t = elapsedTime / fadeDuration;

            promptCanvasGroup.alpha = Mathf.Lerp(originalCanvasGroupAlpha, desiredAlpha, t);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        promptCanvasGroup.alpha = desiredAlpha;
        FadeRoutine = null;
    }
}
