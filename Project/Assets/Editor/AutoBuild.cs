using UnityEditor;
using UnityEngine;
using System;
using System.IO;

public class AutoBuild
{
    // [MenuItem("Build/Build With Timestamp")]
    // public static void BuildWithTimestamp()
    // {
    //     string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    //     string folderName = $"Builds/Build_{timeStamp}";
    //
    //     string exeName = $"MyGame_{timeStamp}.exe";
    //     string fullPath = Path.Combine(folderName, exeName);
    //
    //     BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
    //     buildPlayerOptions.scenes = new[] { "Assets/Scenes/Main.unity" };
    //     buildPlayerOptions.locationPathName = fullPath;
    //     buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
    //     buildPlayerOptions.options = BuildOptions.None;
    //
    //     BuildPipeline.BuildPlayer(buildPlayerOptions);
    // }
}