
using System;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using UnityEngine.SceneManagement;

public class Video_GameManager : IGameManager
{
    protected VideoPlayer videoPlayer;
    protected bool _initiailized;

    private readonly string prefix = "Video_";

   

    [Header("Video Settings")] public float playbackSpeed;



    protected override void Init()
    {
        
        BindEvent();
        
        videoPlayer = GetComponent<VideoPlayer>();

  
        string mp4Path = Path.Combine(Application.streamingAssetsPath, $"{SceneManager.GetActiveScene().name.Substring(prefix.Length)}.mp4");

    
        if (File.Exists(mp4Path))
        {
       
            videoPlayer.url = mp4Path;
        }
        else
        {
            // MP4 파일이 없으면 MOV 파일 재생
            string movPath = Path.Combine(Application.streamingAssetsPath, $"{SceneManager.GetActiveScene().name.Substring(prefix.Length)}.mov");
            videoPlayer.url = movPath;
        }
        
        videoPlayer.Play();

        _initiailized = true;
        
      
    }


    protected override void OnRaySynced()
    {
        
    }
    
}
