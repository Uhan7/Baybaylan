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

        // Some prompts are nested in Main Game while scene-specific prompts
        // are added directly to the scene. They are visually one group but
        // are not guaranteed to be siblings, so clear every other prompt in
        // the loaded scene before this one fades in.
        HideOtherPromptsImmediately();
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

    private void HideOtherPromptsImmediately()
    {
        HelperUI[] helpers = FindObjectsByType<HelperUI>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (HelperUI helper in helpers)
        {
            if (helper == this || helper.gameObject.scene != gameObject.scene) continue;
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
