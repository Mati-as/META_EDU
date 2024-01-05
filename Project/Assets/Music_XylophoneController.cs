using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class Music_XylophoneController : MonoBehaviour
{
  public Transform[] xylophones;
  public Vector3[] targetPos;

  [Header("position setting")] 
  public Vector3 defaultOffset;

  private bool _isInit;

  [Header("sound")] 
  public int audioSize;
  public int volumeA;
  private AudioClip _audioClip;
  private AudioSource[] _audioSourcesA;
    
  
  private string _path = "게임별분류/기본컨텐츠/Music";

  private void Start()
  {
    if (!_isInit) Init();

    DoIntroMove();
  }
  protected virtual void SetAudio()
  {
    _audioSourcesA = SetAudioSettings(_audioSourcesA, _audioClip, audioSize, volumeA);
  }
  

  private void Init()
  {
    xylophones = new Transform[transform.childCount];
    xylophones = GetComponentsInChildren<Transform>();
    
    targetPos = new Vector3[xylophones.Length];
   
      
    for (int i = 0;i < xylophones.Length ; i++)
    {
      targetPos[i] = xylophones[i].transform.position;
    }
    
    //초기 위치 설정
    foreach (var x in xylophones)
    {
      x.transform.position += defaultOffset;
    }

    _isInit = true;
   
  }

  private float _interval= 0.4f;

  private void DoIntroMove()
  {
    for (int i = 0; i < xylophones.Length ; ++i)
    {
      xylophones[i].transform.DOMove(targetPos[i], 0.5f + _interval* i)
        .SetDelay(2f);
    }
  }

  private AudioClip LoadSound(string path)
  {
    AudioClip _audioClip = Resources.Load<AudioClip>(path);
    return _audioClip;
  }
  
  
  
  
  private AudioSource[] SetAudioSettings(AudioSource[] audioSources, AudioClip audioClip, int size, float volume = 1f,
    float interval = 0.25f)
  {
    
    audioSources = new AudioSource[size];
    AudioClip _audioClip = LoadSound("게임별분류/기본컨텐츠/Music/Xylophone");
      
    for (var i = 0; i < audioSources.Length; i++)
    {
      audioSources[i] = gameObject.AddComponent<AudioSource>();
      audioSources[i].clip = _audioClip;
      audioSources[i].volume = volume;
      audioSources[i].spatialBlend = 0f;
      audioSources[i].outputAudioMixerGroup = null;
      audioSources[i].playOnAwake = false;
      audioSources[i].pitch = 0.75f + 0.08f * i;
    }

    return audioSources;
  }
  
  protected void FindAndPlayAudio(AudioSource[] audioSources, bool isBurst = false, bool recursive = false,
    float volume = 0.8f)
  {
    if (!isBurst)
    {
      var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);

      if (availableAudioSource != null) FadeInSound(availableAudioSource, volume);

#if UNITY_EDITOR
#endif
    }
  }

  protected void FadeOutSound(AudioSource audioSource, float target = 0.1f, float fadeInDuration = 2.3f,
    float duration = 1f)
  {
    audioSource.DOFade(target, duration).SetDelay(fadeInDuration).OnComplete(() =>
    {
#if UNITY_EDITOR
#endif
      audioSource.Stop();
    });
  }

  protected void FadeInSound(AudioSource audioSource, float targetVolume = 1f, float duration = 0.3f)
  {
#if UNITY_EDITOR

#endif
    audioSource.Play();
    audioSource.DOFade(targetVolume, duration).OnComplete(() => { FadeOutSound(audioSource); });
  }
}
