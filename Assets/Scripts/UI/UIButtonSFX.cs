using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Tile SFX")]
    [SerializeField] private AudioClip hoverSFX;
    [SerializeField] private AudioClip clickSFX;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CanPlay())
            Play(hoverSFX);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && CanPlay())
            Play(clickSFX);
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
