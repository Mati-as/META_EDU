using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
[CreateAssetMenu(fileName = "AudioAndUIData", menuName = "AudioUIData/AUInfo", order = 0 )]
public class AudioAndUIData : ScriptableObject
{
    [Header("Animal Sound Audio Settings")] [Space(10f)] 
    public AudioClip bull;
    public AudioClip hippo;
    public AudioClip  fox;
    public AudioClip giraffe;
    public AudioClip horse;
    public AudioClip mouse;
    public AudioClip owl;
    public AudioClip parrot;
    public AudioClip pig;
    public AudioClip rabbit;
    public AudioClip racoon;
    public AudioClip reindeer;
    public AudioClip sheep;
    public AudioClip wolf;
    
    [Space(10f)]  [Header("UI Audio Settings")] [Space(10f)]
    public AudioClip bullUI;
    public AudioClip hippoUI;
    public AudioClip  foxUI;
    public AudioClip giraffeUI;
    public AudioClip horseUI;
    public AudioClip mouseUI;
    public AudioClip owlUI;
    public AudioClip parrotUI;
    public AudioClip pigUI;
    public AudioClip rabbitUI;
    public AudioClip racoonUI;
    public AudioClip reindeerUI;
    public AudioClip sheepUI;
    public AudioClip wolfUI;
    
    

    [Space(10f)]  [Header("UI Message Settings")] [Space(10f)]
    public AudioClip howToPlayAudioClip;
    public AudioClip storyA;
    public AudioClip storyB;
    public AudioClip storyC;

    
    public string howToPlayUI;
    public string storyAString;
    public string storyBString;
    public string storyCString;
}
