using UnityEngine;

// Keeps user settings available between scenes and game sessions.
public static class SettingsHolder
{
    private const string BGMVolumeKey = "BGMVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const float DefaultVolume = 1f;

    public static float BGMVolume
    {
        get => LoadVolume(BGMVolumeKey);
        set => SaveVolume(BGMVolumeKey, value);
    }

    public static float SFXVolume
    {
        get => LoadVolume(SFXVolumeKey);
        set => SaveVolume(SFXVolumeKey, value);
    }

    private static float LoadVolume(string key)
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(key, DefaultVolume));
    }

    private static void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }
}
