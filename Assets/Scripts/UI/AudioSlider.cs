using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    public enum AudioChannel
    {
        BGM,
        SFX
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

        float savedVolume = audioChannel == AudioChannel.BGM
            ? SettingsHolder.BGMVolume
            : SettingsHolder.SFXVolume;

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
}
