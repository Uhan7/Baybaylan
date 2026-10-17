using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Setup -----------------------------------------------------------------------
public interface IDragNotify
{
    void OnDragBegin();
    void OnDragEnd();
}

// Main Class ------------------------------------------------------------------
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    // Variables ---------------------------------------------------------------
    [Header("Events")]
    [HideInInspector] public static bool anyBeingDragged = false;
    public static bool InteractionLocked { get; private set; }

    [Header("Components")]
    [HideInInspector] private RectTransform rectTransform;
    [HideInInspector] private CanvasGroup canvasGroup;
    [HideInInspector] private LayoutElement layoutElement;
    [HideInInspector] private Animator anim;

    [Header("References")]
    [SerializeField] public Canvas canvas; // To be set by spawners (TileSet.cs)

    [Header("Audio")]
    [HideInInspector] public AudioSource sfxSource; // To be set by spawners (TileSet.cs)
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip dragSFX;

    [Header("Jiggle")]
    [Tooltip("Maximum left or right tilt in degrees. The chosen angle is always between half and all of this value.")]
    [SerializeField, Min(0f)] private float maxJiggleAngle = 10f;
    [SerializeField, Min(0f)] private float jiggleDuration = 0.0667f;

    [Header("Tile Particles")]
    [SerializeField] private ParticleSystem dragParticles;
    [SerializeField] private ParticleSystem dropParticles;
    [SerializeField] private ParticleSystem scoreParticles;

    [Header("Storage Values")]
    [HideInInspector] private Transform originalParent;
    [HideInInspector] private int originalSiblingIndex;

    [Header("Flags")]
    [HideInInspector] private IDragNotify dragNotify;
    [HideInInspector] public bool isBeingDragged; // For IDragNotify

    [HideInInspector] private Coroutine jiggleRoutine;
    [HideInInspector] private float currentJiggleAngle;
    [HideInInspector] private bool controlsJiggleRotation;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layoutElement = GetComponent<LayoutElement>();
        anim = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        // The existing hold clips contain a fixed -10 degree rotation. Apply the
        // chosen angle after the Animator so the scale animation still plays.
        if (controlsJiggleRotation)
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentJiggleAngle);
    }

    private void OnDisable()
    {
        if (jiggleRoutine != null) StopCoroutine(jiggleRoutine);

        jiggleRoutine = null;
        currentJiggleAngle = 0f;
        controlsJiggleRotation = false;

        if (rectTransform != null)
            rectTransform.localRotation = Quaternion.identity;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (InteractionLocked) return;

        ToolTipAble tooltip = GetComponentInChildren<ToolTipAble>();
        if (tooltip) tooltip.HideTooltip();

        if (dragNotify == null) dragNotify = GetComponent<IDragNotify>();

        anyBeingDragged = true;
        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
        layoutElement.ignoreLayout = true;

        anim.SetBool("Hover", false);
        anim.SetBool("Hold", false);
        JiggleTo(0f);

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        transform.SetParent(canvas.transform);

        sfxSource.PlayOneShot(dragSFX);

        dragNotify?.OnDragBegin();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InteractionLocked || !isBeingDragged) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isBeingDragged) return;

        anyBeingDragged = false;
        isBeingDragged = false;
        canvasGroup.blocksRaycasts = true;
        layoutElement.ignoreLayout = false;
        JiggleTo(0f);

        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
        }

        dragNotify?.OnDragEnd();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InteractionLocked || anyBeingDragged) return;

        sfxSource.PlayOneShot(hoverSFX);
        anim.SetBool("Hover", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        anim.SetBool("Hover", false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (InteractionLocked || anyBeingDragged) return;

        anim.SetBool("Hold", true);
        JiggleTo(GetRandomJiggleAngle());
        PlayParticles(dragParticles);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        anim.SetBool("Hold", false);
        JiggleTo(0f);
    }

    // Helper Functions --------------------------------------------------------
    public static void SetInteractionLocked(bool locked)
    {
        InteractionLocked = locked;
    }

    public void PlayDropParticles()
    {
        PlayParticles(dropParticles);
    }

    public void PlayScoreParticles()
    {
        PlayParticles(scoreParticles);
    }

    private static void PlayParticles(ParticleSystem particles)
    {
        if (particles == null) return;

        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.Play(true);
    }

    private float GetRandomJiggleAngle()
    {
        float magnitude = Random.Range(maxJiggleAngle * 0.5f, maxJiggleAngle);
        return Random.value < 0.5f ? -magnitude : magnitude;
    }

    private void JiggleTo(float targetAngle)
    {
        if (jiggleRoutine != null) StopCoroutine(jiggleRoutine);
        jiggleRoutine = StartCoroutine(AnimateJiggle(targetAngle));
    }

    private IEnumerator AnimateJiggle(float targetAngle)
    {
        controlsJiggleRotation = true;

        float startAngle = currentJiggleAngle;
        float elapsedTime = 0f;

        while (elapsedTime < jiggleDuration)
        {
            float progress = jiggleDuration <= 0f ? 1f : elapsedTime / jiggleDuration;
            currentJiggleAngle = Mathf.Lerp(startAngle, targetAngle, Mathf.SmoothStep(0f, 1f, progress));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentJiggleAngle = targetAngle;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentJiggleAngle);
        controlsJiggleRotation = !Mathf.Approximately(targetAngle, 0f);
        jiggleRoutine = null;
    }
}
