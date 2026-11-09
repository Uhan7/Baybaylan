using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    public enum AudioChannel
    {
        BGM,
        SFX,
        Ambience
    }

    [SerializeField] private AudioChannel audioChannel;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (slider == null) slider = GetComponent<Slider>();

        float savedVolume;
        switch (audioChannel)
        {
            case AudioChannel.BGM:
                savedVolume = SettingsHolder.BGMVolume;
                break;
            case AudioChannel.Ambience:
                savedVolume = SettingsHolder.AmbienceVolume;
                break;
            default:
                savedVolume = SettingsHolder.SFXVolume;
                break;
        }

        slider.SetValueWithoutNotify(savedVolume);
    }

    public void SetBGMVolume(float volume)
    {
        SettingsHolder.BGMVolume = volume;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        SettingsHolder.SFXVolume = volume;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(volume);
    }

    public void SetAmbienceVolume(float volume)
    {
        SettingsHolder.AmbienceVolume = volume;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetAmbienceVolume(volume);
    }
}
