
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

   
    protected Vector3 _defaultPosition { get; private set; }

    [Header("Video Settings")] [SerializeField] private float playbackSpeed;



    protected override void Init()
    {
        
        BindEvent();
        
     
        
        //DoShake시 트위닝 오류로 원래위치에서 벗어나는 것을 방지하기 위한 defaultPosition 설정.
        //DoShake의 OnComplete에서 동작하도록 구성하였습니다. 02/02/24
        _defaultPosition = new Vector3();
        _defaultPosition = transform.position;
        
        //비디오 재생관련 세팅.
        videoPlayer = GetComponent<VideoPlayer>();
        
        videoPlayer.playbackSpeed = playbackSpeed; 
        
        string mp4Path =
            Path.Combine(Application.streamingAssetsPath,
            $"{SceneManager.GetActiveScene().name.Substring(prefix.Length)}.mp4");
    
        if (File.Exists(mp4Path))
        {
            videoPlayer.url = mp4Path;
        }
        else
        {
            // MP4 파일이 없으면 MOV 파일 재생
            string movPath = 
                Path.Combine(Application.streamingAssetsPath,
                $"{SceneManager.GetActiveScene().name.Substring(prefix.Length)}.mov");
                videoPlayer.url = movPath;
        }
        
        videoPlayer.Play();
        
        _initiailized = true;
        
      
    }


    
}
