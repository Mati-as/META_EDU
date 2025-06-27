#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlayCountTracker
{
    static PlayCountTracker()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            int count = PlayerPrefs.GetInt("PlayCount", 0);
            count++;
            PlayerPrefs.SetInt("PlayCount", count);
            PlayerPrefs.Save();

            Debug.Log($"[PlayCountTracker] 에디터 플레이 횟수: {count}");
        }
    }
}
#endif