using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
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
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
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
    [SerializeField, Min(0f)] private float jiggleDuration = 0.1f;

    [Header("Press Scale")]
    [Tooltip("Scale used while the tile is pressed, like a button.")]
    [SerializeField, Range(0.5f, 1f)] private float pressedScale = 0.985f;
    [SerializeField, Min(0f)] private float pressScaleDuration = 0.1f;

    [Header("Tile Particles")]
    [SerializeField] private ParticleSystem dragParticles;
    [SerializeField] private ParticleSystem dropParticles;
    [SerializeField] private ParticleSystem clickParticles;
    [SerializeField] private ParticleSystem scoreParticles;

    [Header("Storage Values")]
    [HideInInspector] private Transform originalParent;
    [HideInInspector] private int originalSiblingIndex;
    [HideInInspector] private GameObject dragPlaceholder;
    private readonly List<RaycastResult> dragRaycastResults = new List<RaycastResult>();

    [Header("Flags")]
    [HideInInspector] private IDragNotify dragNotify;
    [HideInInspector] public bool isBeingDragged; // For IDragNotify

    [HideInInspector] private Coroutine jiggleRoutine;
    [HideInInspector] private Coroutine angleJiggleSequenceRoutine;
    [HideInInspector] private Coroutine pressScaleRoutine;
    [HideInInspector] private float currentJiggleAngle;
    [HideInInspector] private bool controlsJiggleRotation;
    [HideInInspector] private Vector3 baseLocalScale;
    [HideInInspector] private float currentPressScale = 1f;
    [HideInInspector] private bool controlsPressScale;
    [HideInInspector] private int suppressHoverSFXUntilFrame = -1;

    // Main Functions ----------------------------------------------------------
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        layoutElement = GetComponent<LayoutElement>();
        anim = GetComponent<Animator>();
        baseLocalScale = rectTransform.localScale;
    }

    private void LateUpdate()
    {
        // Apply interaction feedback after the Animator. Press scale is a
        // multiplier of the current animated size, so hover and press blend
        // instead of snapping between unrelated absolute scales.
        if (controlsJiggleRotation)
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, currentJiggleAngle);

        if (controlsPressScale)
        {
            Vector3 animatedScale = rectTransform.localScale;
            rectTransform.localScale = new Vector3(
                animatedScale.x * currentPressScale,
                animatedScale.y * currentPressScale,
                animatedScale.z);
        }
    }

    private void OnDisable()
    {
        if (jiggleRoutine != null) StopCoroutine(jiggleRoutine);
        if (angleJiggleSequenceRoutine != null) StopCoroutine(angleJiggleSequenceRoutine);
        if (pressScaleRoutine != null) StopCoroutine(pressScaleRoutine);
        RemoveDragPlaceholder();

        jiggleRoutine = null;
        angleJiggleSequenceRoutine = null;
        pressScaleRoutine = null;
        currentJiggleAngle = 0f;
        controlsJiggleRotation = false;
        currentPressScale = 1f;
        controlsPressScale = false;
        suppressHoverSFXUntilFrame = -1;

        if (rectTransform != null)
        {
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = baseLocalScale;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (InteractionLocked) return;

        StopAngleJiggleSequence();

        ToolTipAble tooltip = GetComponentInChildren<ToolTipAble>();
        if (tooltip) tooltip.HideTooltip();

        if (dragNotify == null) dragNotify = GetComponent<IDragNotify>();

        anyBeingDragged = true;
        isBeingDragged = true;
        canvasGroup.blocksRaycasts = false;
        layoutElement.ignoreLayout = true;

        anim.SetBool("Hover", false);
        anim.SetBool("Hold", false);
        ScaleTo(pressedScale);
        JiggleTo(GetRandomJiggleAngle());

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        CreateDragPlaceholder();
        transform.SetParent(canvas.transform);
        PlayParticleBurst(dragParticles);

        sfxSource.PlayOneShot(dragSFX);

        dragNotify?.OnDragBegin();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (InteractionLocked || !isBeingDragged) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        UpdateDropPreview(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isBeingDragged) return;

        StopAngleJiggleSequence();

        anyBeingDragged = false;
        isBeingDragged = false;
        // Restoring raycasts can make Unity report a fresh pointer-enter on
        // the tile under the cursor. Ignore only that immediate hover sound.
        suppressHoverSFXUntilFrame = Time.frameCount + 1;
        canvasGroup.blocksRaycasts = true;
        layoutElement.ignoreLayout = false;
        anim.SetBool("Hold", false);
        ScaleTo(1f);
        JiggleTo(0f);

        if (transform.parent == canvas.transform)
        {
            transform.SetParent(originalParent);
            transform.SetSiblingIndex(originalSiblingIndex);
            RegisterSmoothLayoutStart(originalParent);
        }

        RemoveDragPlaceholder();

        dragNotify?.OnDragEnd();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (InteractionLocked || anyBeingDragged) return;

        if (Time.frameCount > suppressHoverSFXUntilFrame)
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

        ScaleTo(pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ScaleTo(1f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InteractionLocked || anyBeingDragged || isBeingDragged) return;

        PlayParticleBurst(clickParticles);
        PlayAngleJiggle();
    }

    // Helper Functions --------------------------------------------------------
    public static void SetInteractionLocked(bool locked)
    {
        InteractionLocked = locked;
    }

    public void PlayDropParticles()
    {
        PlayParticleBurst(dropParticles);
    }

    public void PlayScoreParticles()
    {
        PlayParticleBurst(scoreParticles, true);
    }

    public void PlayScoreFeedback()
    {
        PlayScoreParticles();
        PlayAngleJiggle();
    }

    private void PlayParticleBurst(
        ParticleSystem particleTemplate,
        bool emitBurstImmediately = false)
    {
        if (particleTemplate == null) return;

        // Render transient tile feedback above layout masks and sibling UI.
        // The nested systems remain editable templates and are never played.
        Transform effectParent = canvas != null
            ? canvas.transform
            : particleTemplate.transform.parent;
        GameObject effectObject = Instantiate(particleTemplate.gameObject, effectParent);
        effectObject.name = particleTemplate.gameObject.name + " (Runtime)";

        RectTransform effectRect = effectObject.transform as RectTransform;
        if (effectRect != null)
        {
            effectRect.position = rectTransform.TransformPoint(rectTransform.rect.center);
            effectRect.rotation = Quaternion.identity;
            effectRect.localScale = Vector3.one;
            effectRect.SetAsLastSibling();
        }

        ParticleSystem runtimeParticles = effectObject.GetComponent<ParticleSystem>();
        runtimeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = runtimeParticles.main;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
        main.stopAction = ParticleSystemStopAction.None;

        UIParticle uiParticle = effectObject.GetComponent<UIParticle>();
        if (uiParticle != null)
        {
            // Runtime clones need to rebuild their UI renderers before the
            // package can simulate and draw them on the canvas.
            uiParticle.RefreshParticles();

            if (emitBurstImmediately)
            {
                // UIParticle can miss a burst scheduled exactly at time zero
                // when a system is cloned mid-frame. Emit the configured score
                // burst ourselves so it appears on the same scoring beat as
                // the tile jiggle and pitched tick.
                int burstCount = GetImmediateBurstCount(runtimeParticles);
                ParticleSystem.EmissionModule emission = runtimeParticles.emission;
                emission.enabled = false;
                runtimeParticles.Play(false);
                runtimeParticles.Emit(burstCount);
                uiParticle.Resume();
            }
            else
            {
                uiParticle.Play();
            }
        }
        else
        {
            runtimeParticles.Play(true);
        }

        float destroyDelay = main.duration
            + main.startDelay.constantMax
            + main.startLifetime.constantMax
            + 0.25f;
        Destroy(effectObject, destroyDelay);
    }

    private static int GetImmediateBurstCount(ParticleSystem particles)
    {
        ParticleSystem.EmissionModule emission = particles.emission;
        int configuredBurstCount = emission.burstCount;
        if (configuredBurstCount <= 0) return 1;

        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[configuredBurstCount];
        emission.GetBursts(bursts);

        int particleCount = 0;
        foreach (ParticleSystem.Burst burst in bursts)
        {
            if (burst.time > 0.001f) continue;
            particleCount += Mathf.CeilToInt(burst.count.constantMax);
        }

        return Mathf.Max(1, particleCount);
    }

    public void PlayAngleJiggle()
    {
        StopAngleJiggleSequence();
        angleJiggleSequenceRoutine = StartCoroutine(PlayAngleJiggleSequence());
    }

    public RectTransform GetDragPlaceholder()
    {
        return dragPlaceholder != null
            ? dragPlaceholder.transform as RectTransform
            : null;
    }

    public int GetPreviewSiblingIndex(Transform targetParent, int fallbackIndex)
    {
        RectTransform placeholder = GetDragPlaceholder();
        return placeholder != null && placeholder.parent == targetParent
            ? placeholder.GetSiblingIndex()
            : fallbackIndex;
    }

    public void RegisterSmoothLayoutStart(Transform targetParent)
    {
        if (targetParent == null) return;

        SmoothGridLayoutGroup smoothLayout =
            targetParent.GetComponent<SmoothGridLayoutGroup>();
        if (smoothLayout != null)
            smoothLayout.SetCurrentPosition(rectTransform, rectTransform.anchoredPosition);
    }

    private void CreateDragPlaceholder()
    {
        RemoveDragPlaceholder();

        dragPlaceholder = new GameObject("Tile Drop Preview", typeof(RectTransform));
        RectTransform placeholderRect = dragPlaceholder.transform as RectTransform;
        placeholderRect.SetParent(originalParent, false);
        placeholderRect.SetSiblingIndex(originalSiblingIndex);
        placeholderRect.sizeDelta = rectTransform.rect.size;
    }

    private void UpdateDropPreview(PointerEventData eventData)
    {
        dragRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, dragRaycastResults);

        foreach (RaycastResult raycastResult in dragRaycastResults)
        {
            DropZone dropZone = raycastResult.gameObject.GetComponentInParent<DropZone>();
            if (dropZone != null && dropZone.PreviewInsertion(this, eventData)) return;
        }

        RectTransform placeholder = GetDragPlaceholder();
        if (placeholder == null || originalParent == null) return;

        placeholder.SetParent(originalParent, false);
        placeholder.SetSiblingIndex(originalSiblingIndex);
    }

    private void RemoveDragPlaceholder()
    {
        if (dragPlaceholder == null) return;

        dragPlaceholder.SetActive(false);
        Destroy(dragPlaceholder);
        dragPlaceholder = null;
    }

    private float GetRandomJiggleAngle()
    {
        float magnitude = Random.Range(maxJiggleAngle * 0.5f, maxJiggleAngle);
        return Random.value < 0.5f ? -magnitude : magnitude;
    }

    private IEnumerator PlayAngleJiggleSequence()
    {
        JiggleTo(GetRandomJiggleAngle());

        if (jiggleDuration > 0f)
            yield return new WaitForSeconds(jiggleDuration);
        else
            yield return null;

        JiggleTo(0f);

        if (jiggleDuration > 0f)
            yield return new WaitForSeconds(jiggleDuration);

        angleJiggleSequenceRoutine = null;
    }

    private void StopAngleJiggleSequence()
    {
        if (angleJiggleSequenceRoutine == null) return;

        StopCoroutine(angleJiggleSequenceRoutine);
        angleJiggleSequenceRoutine = null;
    }

    private void JiggleTo(float targetAngle)
    {
        if (jiggleRoutine != null) StopCoroutine(jiggleRoutine);
        jiggleRoutine = StartCoroutine(AnimateJiggle(targetAngle));
    }

    private void ScaleTo(float targetScale)
    {
        if (pressScaleRoutine != null) StopCoroutine(pressScaleRoutine);
        pressScaleRoutine = StartCoroutine(AnimatePressScale(targetScale));
    }

    private IEnumerator AnimatePressScale(float targetScale)
    {
        controlsPressScale = true;

        float startScale = currentPressScale;
        float elapsedTime = 0f;

        while (elapsedTime < pressScaleDuration)
        {
            float progress = pressScaleDuration <= 0f
                ? 1f
                : elapsedTime / pressScaleDuration;
            currentPressScale = Mathf.Lerp(
                startScale,
                targetScale,
                Mathf.SmoothStep(0f, 1f, progress));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        currentPressScale = targetScale;
        // Keep applying a neutral x1 multiplier after release. This lets the
        // Animator remain the source of the hover size without a one-frame
        // reset to the prefab's base scale.
        controlsPressScale = true;
        pressScaleRoutine = null;
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
