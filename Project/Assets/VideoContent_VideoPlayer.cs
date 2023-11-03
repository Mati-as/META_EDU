
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Video;

public class VideoContent_VideoPlayer : MonoBehaviour
{
    void Awake() 
    {
        var videoPlayer = GetComponent<VideoPlayer>(); // VideoPlayer 컴포넌트를 찾습니다.
        string path = Path.Combine(Application.streamingAssetsPath, "Turtle.mp4");
        videoPlayer.url = path;
        videoPlayer.Play();
    }
}
