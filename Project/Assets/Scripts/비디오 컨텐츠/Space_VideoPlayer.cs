using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space_VideoPlayer : Base_VideoContentPlayer
{
  
    
    protected override void Init()
    {
        base.Init();
        videoPlayer.playbackSpeed = playbackSpeed; 
       
#if UNITY_EDITOR
        Debug.Log($"{videoPlayer.playbackSpeed} <- speed");
        #endif
    }
}
