using UnityEngine;

// Keeps user settings available between scenes and game sessions.
public static class SettingsHolder
{
    private const string BGMVolumeKey = "BGMVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string AmbienceVolumeKey = "AmbienceVolume";

    public static float BGMVolume
    {
        get => LoadVolume(BGMVolumeKey, 0.4f);
        set => SaveVolume(BGMVolumeKey, value);
    }

    public static float SFXVolume
    {
        get => LoadVolume(SFXVolumeKey, 0.8f);
        set => SaveVolume(SFXVolumeKey, value);
    }

    public static float AmbienceVolume
    {
        get => LoadVolume(AmbienceVolumeKey, 0.4f);
        set => SaveVolume(AmbienceVolumeKey, value);
    }

    private static float LoadVolume(string key, float defaultVolume)
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(key, defaultVolume));
    }

    private static void SaveVolume(string key, float value)
    {
        PlayerPrefs.SetFloat(key, Mathf.Clamp01(value));
        PlayerPrefs.Save();
    }
}
