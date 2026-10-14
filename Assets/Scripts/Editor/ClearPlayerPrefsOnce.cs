#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

internal static class ClearPlayerPrefsOnce
{
    [InitializeOnLoadMethod]
    private static void Clear()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[Fresh Save Test] Cleared Baybaylan PlayerPrefs.");
    }
}
#endif
