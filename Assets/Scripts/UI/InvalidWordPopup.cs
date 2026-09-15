using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

class InvalidWordPopup : MonoBehaviour
{
    [SerializeField] GameObject parentObj;
    [SerializeField] GameObject popupPrefab;
    [SerializeField, Min(0f)] float fadeOutTime = 1.5f;
    [SerializeField, Min(0f), Tooltip("Seconds before the popup starts fading out.")]
    float fadeOutDelay = 0.5f;
    [SerializeField, Min(0f), Tooltip("Seconds before the popup starts moving up.")]
    float moveUpDelay = 0.5f;
    [SerializeField] float upDistance = 100f;
    [SerializeField] InvalidWordTypes.Messages invalidWordMessages = new InvalidWordTypes.Messages();

    public void ShowInvalidWordPopup(InvalidWordTypes.InvalidWordType type, string word)
    {
        GameObject popupInstance = Instantiate(popupPrefab, parentObj.transform);
        popupInstance.transform.localPosition = Vector3.zero; 
        TextMeshProUGUI invalidWordText = popupInstance.GetComponentInChildren<TextMeshProUGUI>();
        Image bgImg = popupInstance.GetComponentInChildren<Image>();

        invalidWordText.text = invalidWordMessages.GetInvalidWordMessage(type, word);

        StartCoroutine(FadeOut(popupInstance, invalidWordText, bgImg));
    }

    IEnumerator FadeOut(GameObject popupInstance, TextMeshProUGUI invalidWordText, Image bgImg)
    {
        float duration = Mathf.Max(0f, fadeOutTime);
        float fadeDelay = Mathf.Max(0f, fadeOutDelay);
        float movementDelay = Mathf.Max(0f, moveUpDelay);
        float lifetime = Mathf.Max(fadeDelay, movementDelay) + duration;
        Vector3 startPosition = popupInstance.transform.localPosition;
        Vector3 endPosition = startPosition + Vector3.up * upDistance;
        Color bgColor = bgImg.color;
        Color textColor = invalidWordText.color;
        float elapsed = 0f;

        while (true)
        {
            float fadeProgress = AnimationProgress(elapsed, fadeDelay, duration);
            float movementProgress = AnimationProgress(elapsed, movementDelay, duration);

            popupInstance.transform.localPosition = Vector3.Lerp(startPosition, endPosition, movementProgress);
            bgImg.color = new Color(bgColor.r, bgColor.g, bgColor.b, bgColor.a * (1f - fadeProgress));
            invalidWordText.color = new Color(textColor.r, textColor.g, textColor.b, textColor.a * (1f - fadeProgress));

            if (elapsed >= lifetime)
                break;

            yield return null;
            elapsed += Time.deltaTime;
        }

        Destroy(popupInstance);
    }

    static float AnimationProgress(float elapsed, float delay, float duration)
    {
        if (elapsed < delay)
            return 0f;

        return duration > 0f ? Mathf.Clamp01((elapsed - delay) / duration) : 1f;
    }
}
