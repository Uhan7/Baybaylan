using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BirdHeadFallController : MonoBehaviour
{
    [Header("Heads in Mahika Bar Order")]
    [Tooltip("Element 0 falls after bar 1, element 1 after bar 2, and so on.")]
    [SerializeField] private Image[] heads = new Image[0];

    [Header("Fall")]
    [Tooltip("Seconds after Indarapatra's hit before the head starts falling. Uses game time, like the attack.")]
    [SerializeField, Min(0f)] private float headFallDelay = 0.1f;
    [Tooltip("Downward acceleration in UI units per second squared.")]
    [SerializeField, Min(0f)] private float gravity = 900f;
    [Tooltip("Downward speed when the head starts falling, in UI units per second.")]
    [SerializeField, Min(0f)] private float initialFallSpeed = 0f;

    [Header("Fade")]
    [Tooltip("Seconds of falling before fading starts. Zero fades immediately.")]
    [SerializeField, Min(0f)] private float fadeDelay = 0f;
    [Tooltip("Seconds to become fully transparent while falling. Uses game time, like the attack.")]
    [SerializeField, Min(0f)] private float fadeDuration = 0.8f;

    private bool[] fallenHeads;

    private void Awake()
    {
        fallenHeads = new bool[heads.Length];
    }

    // Runs alongside the attack; word submission waits for it before dialogue or round ending.
    public IEnumerator PlayHeadForCompletedBar(int completedBarCount, float attackHitDelay = 0f)
    {
        int index = completedBarCount - 1;
        if (index < 0 || index >= heads.Length || fallenHeads[index]) yield break;

        Image head = heads[index];
        if (head == null) yield break;
        fallenHeads[index] = true;

        float fallStartDelay = Mathf.Max(0f, attackHitDelay) + Mathf.Max(0f, headFallDelay);
        if (fallStartDelay > 0f) yield return new WaitForSeconds(fallStartDelay);
        if (head == null) yield break;

        RectTransform rect = head.rectTransform;
        Vector2 startPosition = rect.anchoredPosition;
        Color startColor = head.color;
        float delay = Mathf.Max(0f, fadeDelay);
        float duration = Mathf.Max(0f, fadeDuration);
        float elapsed = 0f;

        while (elapsed < delay + duration)
        {
            yield return null;
            if (head == null) yield break;

            elapsed = Mathf.Min(elapsed + Time.deltaTime, delay + duration);
            float distance = initialFallSpeed * elapsed + 0.5f * gravity * elapsed * elapsed;
            rect.anchoredPosition = startPosition + Vector2.down * distance;

            float fade = duration > 0f ? Mathf.Clamp01((elapsed - delay) / duration) : 1f;
            head.color = new Color(startColor.r, startColor.g, startColor.b, startColor.a * (1f - fade));
        }

        head.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        head.gameObject.SetActive(false);
    }
}
