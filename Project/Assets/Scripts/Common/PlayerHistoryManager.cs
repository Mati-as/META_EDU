using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// 게임 플레이 종류, 플레이 시간을 기록합니다.
/// </summary>
public class PlayerHistoryManager : MonoBehaviour
{
    
    public DateTime latestSceneStartTime; // 가장최근에 Scene(게임)을 실행한 시간 - 게임종료시간 = 플레이시간 
    public DateTime lastestSceneQuitTime;
    private static TimeSpan _playTime;
    
    public static string currentSceneName;

    private XmlDocument _doc;


    public void Init()
    {
        Utils.ReadXML(ref _doc,"Assets/Resources/Common/Data/UserHistory.xml");
        Debug.Log("History Checker Active");
        
        //Register Dictionary 
        GameInfo.Init();
        
        IGameManager.OnSceneLoad -= OnSceneLoad;
        IGameManager.OnSceneLoad += OnSceneLoad;

        TopMenuUI.OnSceneQuit -= OnSceneOrAppQuit;
        TopMenuUI.OnSceneQuit += OnSceneOrAppQuit;
        
        TopMenuUI.OnAppQuit -= OnSceneOrAppQuit;
        TopMenuUI.OnAppQuit += OnSceneOrAppQuit;
    }

    private void OnDestroy()
    {
        IGameManager.OnSceneLoad -= OnSceneLoad;
        TopMenuUI.OnSceneQuit -= OnSceneOrAppQuit;
    }
    
    
    private void OnSceneLoad(string sceneName, DateTime dateTime)
    {
        latestSceneStartTime = dateTime;
        currentSceneName = sceneName;
        Debug.Log($"Scene On -------currentScene: {sceneName}, startTime : {dateTime}");
    }
    
    private void OnSceneOrAppQuit(string sceneName, DateTime dateTime)
    {
        _playTime = TimeSpan.Zero;
        lastestSceneQuitTime = dateTime;
        currentSceneName = sceneName;
        _playTime = lastestSceneQuitTime - latestSceneStartTime;
        string formattedPlaytime = string.Format("{0:D2}분 {1:D2}초", _playTime.Minutes, _playTime.Seconds);
        Debug.Log($"플레이시간 : {formattedPlaytime}");
        AddPlayInfoNode(ref _doc);
    }
    public void AddPlayInfoNode(ref XmlDocument xmlDoc)
    {
        
        XmlNode root = xmlDoc.DocumentElement;
        XmlElement newUser = xmlDoc.CreateElement("PlayData");

        DateTime today = DateTime.Now;
        Debug.Log("Today's date is: " + today.ToString("yyyy-MM-dd"));
        
        newUser.SetAttribute("date",  today.ToString("g"));
        newUser.SetAttribute("sceneid", currentSceneName +": " +GameInfo.ScenedIdToKoreantitle[currentSceneName]);
        newUser.SetAttribute("playtime", string.Format("{0:D2}분 {1:D2}초", _playTime.Minutes, _playTime.Seconds));
        root?.AppendChild(newUser);
        
        Utils.SaveXML(ref _doc, "Assets/Resources/Common/Data/UserHistory.xml");
    }

    private static void SavePlayInfo()
    {
        
    }
}

public static class GameInfo
{
    public static Dictionary<string, string> ScenedIdToKoreantitle;

    public static void Init()
    {
        ScenedIdToKoreantitle = new Dictionary<string, string>
        {
            {"CA001", "동물친구를 찾아라"},
            {"CA002", "땅속탐험!"},
            {"BC001", "자연 - 가을낙엽"},
            {"BC002", "자연 - 호수"},
            {"AA001", "사회관계 - 태극기 영상"},
            {"AA002", "자연 - 우주 영상"},
            {"AA003", "봄 - 새싹 영상"},
            {"AA004", "여름 - 호수 영상"},
            {"AA005", "가을 - 코스모스 영상"},
            {"AA006", "겨울 - 펭귄 영상"},
            {"AA007", "바다 - 거북이 영상"},
            {"AA008", "바다 - 고래 영상"},
            {"AB001", "별이 빛나는 밤에"},
            {"AB002", "송하맹호도"},
            {"AA009", "꽃게"},
            {"AA010", "부엉이"},
            {"BB001", "신체활동 - 공놀이 1"},
            {"BA001", "미술놀이 - 송하맹호도"},
            {"BA002", "미술놀이 - 별이 빛나는 밤에"},
            {"BA003", "미술놀이 - 몬드리안"},
            {"BD001", "음악놀이 - 무지개"},
            {"BB002", "신체활동 - 사방치기"},
            {"BB003", "신체활동 - 손발 뒤집기 1"},
            {"BB004", "신체활동 - 손발 뒤집기 2"},
            {"BA004", "미술놀이 - 손발 도장찍기"},
            {"BB005", "신체활동 - 남극"},
            {"BA005", "미술놀이 - 요리"},
            {"BC003", "-"},
            {"BD002", "음악놀이 - 바다"},
            {"BB006", "신체활동 - 공놀이 2"},
            {"BD003", "음악놀이 - 비즈 드럼"},
            {"BD004", "음악놀이 - 악기놀이"},
            {"BD005", "음악놀이 - 피아노"},
            {"BD006", "음악놀이 - 드럼"},
            {"BA006", "미술놀이 - 스캐치 앤 스크래치"},
            {"BA007", "미술놀이 - 내가 꾸미는 명화 그림"},
            {"BB007", "신체활동 - 동물 공 놀이"},
            {"BB008", "신체활동 - 낚시"},
            {"BB009", "신체활동 - 팝잇"},
            {"BB010", "신체활동 - 모양길찾기"},
            {"BB011", "신체활동 - 트위스터"},
            {"BB012", "신체활동 - 바다문어"}
        };
    }
}
