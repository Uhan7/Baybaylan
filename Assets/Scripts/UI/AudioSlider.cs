using UnityEngine;
using UnityEngine.UI;

public class AudioSlider : MonoBehaviour
{
    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }   

    public void SetBGMVolume(float volume)
    {
        SettingsHolder.BGMVolume = volume;
        AudioManager.Instance.SetBGMVolume(volume);
    }

    public void SetSFXVolume(float volume)
    {
        SettingsHolder.SFXVolume = volume;
        AudioManager.Instance.SetSFXVolume(volume);
    }
}