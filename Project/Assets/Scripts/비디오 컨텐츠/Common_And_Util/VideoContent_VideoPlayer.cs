
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Video;

public class VideoContent_VideoPlayer : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    
    void Awake() 
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
    }
}
