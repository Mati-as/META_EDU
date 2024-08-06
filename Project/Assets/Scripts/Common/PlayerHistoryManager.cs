using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 플레이 종류, 플레이 시간을 기록합니다.
/// </summary>
public class PlayerHistoryManager : MonoBehaviour
{
    
    public static DateTime latestSceneStartTime; // 가장최근에 Scene(게임)을 실행한 시간 - 게임종료시간 = 플레이시간 
    public static DateTime lastestSceneQuitTime; 
    public static string currentSceneName; 
    
    
    private void Awake()
    {
        IGameManager.OnSceneLoad -= OnSceneLoad;
        IGameManager.OnSceneLoad += OnSceneLoad;

        TopMenuUI.OnSceneQuit += OnSceneOrAppQuit;
        TopMenuUI.OnSceneQuit -= OnSceneOrAppQuit;
    }

    private void OnDestroy()
    {
        IGameManager.OnSceneLoad -= OnSceneLoad;
        TopMenuUI.OnSceneQuit -= OnSceneOrAppQuit;
    }
    
    
    private static void OnSceneLoad(string sceneName, DateTime dateTime)
    {
        latestSceneStartTime = dateTime;
        currentSceneName = sceneName;
        Debug.Log($"Scene On -------currentScene: {sceneName}, startTime : {dateTime}");
    }
    
    private static void OnSceneOrAppQuit(string sceneName, DateTime dateTime)
    {
        lastestSceneQuitTime = dateTime;
        currentSceneName = sceneName;
        Debug.Log($"Quit-------- currentScene: {sceneName}, Playtime : {latestSceneStartTime-lastestSceneQuitTime}");
    }
}
