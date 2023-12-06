
using System;
using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class Base_VideoContentPlayer : MonoBehaviour
{
    protected VideoPlayer videoPlayer;

    [Header("Video Settings")] public float playbackSpeed;

    void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        videoPlayer = GetComponent<VideoPlayer>(); // VideoPlayer 컴포넌트를 찾습니다.
        string path = Path.Combine(Application.streamingAssetsPath, $"{gameObject.name}.mp4");
        videoPlayer.url = path;
        videoPlayer.Play();
    }
}
