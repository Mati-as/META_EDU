
using System;
using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class Base_VideoGameManager : IGameManager
{
    protected VideoPlayer videoPlayer;
    protected bool _initiailized;



    [Header("Video Settings")] public float playbackSpeed;

    void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        videoPlayer = GetComponent<VideoPlayer>();

  
        string mp4Path = Path.Combine(Application.streamingAssetsPath, $"{gameObject.name}.mp4");

    
        if (File.Exists(mp4Path))
        {
       
            videoPlayer.url = mp4Path;
        }
        else
        {
            // MP4 파일이 없으면 MOV 파일 재생
            string movPath = Path.Combine(Application.streamingAssetsPath, $"{gameObject.name}.mov");
            videoPlayer.url = movPath;
        }
        
        videoPlayer.Play();

        _initiailized = true;
    }
    
    
    
    
}
