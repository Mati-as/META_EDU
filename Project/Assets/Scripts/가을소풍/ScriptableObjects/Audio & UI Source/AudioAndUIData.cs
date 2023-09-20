using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
[CreateAssetMenu(fileName = "AudioAndUIData", menuName = "AudioUIData/AUInfo", order = 0 )]
public class AudioAndUIData : ScriptableObject
{
    [Header("Animal Sound Audio Settings : 동물 울음 소리")] [Space(10f)] 
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
    
    
    //A: ~ 찾아보세요 B: 찾았어요!
    [Space(10f)]  [Header("UI Audio Settings : A 찾아보세요")] [Space(10f)]
    public AudioClip bullA;
    public AudioClip hippoA;
    public AudioClip foxA;
    public AudioClip giraffeA;
    public AudioClip horseA;
    public AudioClip mouseA;
    public AudioClip owlA;
    public AudioClip parrotA;
    public AudioClip pigA;
    public AudioClip rabbitA;
    public AudioClip racoonA;
    public AudioClip reindeerA;
    public AudioClip sheepA;
    public AudioClip wolfA;
    
    //A: ~ 찾아보세요 B: 찾았어요!
    [Space(15f)]  [Header("UI Audio Settings : B 찾았어요!")][Space(10f)]
   
    public AudioClip bullB;
    public AudioClip hippoB;
    public AudioClip foxB;
    public AudioClip giraffeB;
    public AudioClip horseB;
    public AudioClip mouseB;
    public AudioClip owlB;
    public AudioClip parrotB;
    public AudioClip pigB;
    public AudioClip rabbitB;
    public AudioClip racoonB;
    public AudioClip reindeerB;
    public AudioClip sheepB;
    public AudioClip wolfB;
    
   
   
    [Space(10f)]  [Header("UI Message Settings UI Message 관련")] [Space(10f)]
    public string howToPlayUI;
    public string storyAString;
    public string storyBString;
    public string storyCString;
}
