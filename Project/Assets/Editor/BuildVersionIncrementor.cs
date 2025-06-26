#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildVersionIncrementor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        int currentBuild = PlayerPrefs.GetInt("buildVersion", 0);
        currentBuild++;
        PlayerPrefs.SetInt("buildVersion", currentBuild);
        PlayerPrefs.Save();

        Debug.Log($" 자동 빌드 번호 증가: v1.0.{currentBuild}");
    }
}
#endif