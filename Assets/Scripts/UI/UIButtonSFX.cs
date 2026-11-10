using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerClickHandler
{
    [Header("Tile SFX")]
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;

    private Button button;
    private bool interactableOnPress;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanPlay() && !HasPointerSound(EventTriggerType.PointerEnter))
            Play(hoverSFX);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            interactableOnPress = CanPlay();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // The Button's click callback can disable it before this handler runs
        // (for example, submitting a word starts scoring immediately).
        bool canPlayClick = interactableOnPress;
        interactableOnPress = false;
        if (canPlayClick &&
            !HasPointerSound(EventTriggerType.PointerClick) && !HasSoundCallback(button.onClick))
            Play(clickSFX);
    }

    private void OnDisable()
    {
        interactableOnPress = false;
    }

    // Use an onClick callback for buttons that close their own popup, so the
    // sound plays before disabling the popup clears the pointer-press state.
    public void PlayClickSound()
    {
        Play(clickSFX);
    }

    // Some older buttons already play their audio through Inspector events.
    // Keep those sounds and avoid layering the default clip over them.
    private bool HasPointerSound(EventTriggerType eventType)
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger == null || !trigger.isActiveAndEnabled) return false;
        foreach (EventTrigger.Entry entry in trigger.triggers)
            if (entry.eventID == eventType && HasSoundCallback(entry.callback)) return true;
        return false;
    }

    private static bool HasSoundCallback(UnityEventBase callbacks)
    {
        for (int i = 0; i < callbacks.GetPersistentEventCount(); i++)
        {
            if (callbacks.GetPersistentListenerState(i) == UnityEventCallState.Off) continue;
            Object target = callbacks.GetPersistentTarget(i);
            string method = callbacks.GetPersistentMethodName(i);
            if ((target is AudioSource && method == "PlayOneShot") ||
                (target is AudioManager && method == "PlaySFX") ||
                (target is UIButtonSFX && method == nameof(PlayClickSound))) return true;
        }
        return false;
    }

    private bool CanPlay()
    {
        return button != null && button.IsInteractable();
    }

    private static void Play(AudioClip clip)
    {
        if (clip == null || AudioManager.Instance == null || AudioManager.Instance.sfxSource == null)
            return;

        AudioManager.Instance.sfxSource.PlayOneShot(clip);
    }
}
