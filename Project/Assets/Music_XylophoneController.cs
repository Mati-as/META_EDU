using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class Music_XylophoneController : MonoBehaviour
{
  [Header("Reference")] [SerializeField] private Music_GameManager _gameManager;
  
  public Transform[] xylophones;
  public Vector3[] targetPos;

  [Header("position setting")] 
  public Vector3 defaultOffset;

  private bool _isInit;

  [Header("sound")] 
  public float pitchInterval;
  public int audioSize;
  public int volumeA;
  private AudioClip _audioClip;
  private AudioSource[] _audioSourcesA;
  
  // 오디오 소스 캐싱
  private Dictionary<string, AudioSource> audioSourceMap;
  
  private string _path = "게임별분류/기본컨텐츠/Music";

  private void Awake()
  {
    BindEvent();
  }

  private void Start()
  {
    if (!_isInit) Init();

  
    DoIntroMove();
  }

  private int CHILD_COUNT;
  protected virtual void SetAudio()
  {
   
  }
  

  private void Init()
  {
    
    audioSourceMap = new Dictionary<string, AudioSource>();

    CHILD_COUNT  = transform.childCount;
    
    xylophones = new Transform[CHILD_COUNT];
    xylophones = GetComponentsInChildren<Transform>();
  
    targetPos = new Vector3[CHILD_COUNT];
      
    for (int i = 0;i < CHILD_COUNT ; i++)
    {
      targetPos[i] = xylophones[i].transform.position;
    }

    int count = 0;
   
    //초기 위치 설정
    foreach (var x in xylophones)
    {
      x.transform.position += defaultOffset;

      AudioSource audioSource = new AudioSource();
      Initialize(x.transform,audioSource,pitchInterval * count);
      audioSourceMap.TryAdd(x.gameObject.name, audioSource);
      count++;
    }

    
    _isInit = true;
   
  }

  private float _interval= 0.4f;

  private void DoIntroMove()
  {
    for (int i = 0; i < CHILD_COUNT ; ++i)
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


  
  private void Initialize(Transform _transform,AudioSource xylophones,float pitch, float volume = 1f,
    float interval = 0.25f)
  {
    AudioSource[] tempAudioSources = new AudioSource[this.xylophones.Length];
  
     AudioClip _audioClip = LoadSound("게임별분류/기본컨텐츠/Music/Xylophone");
      
      xylophones = _transform.gameObject.AddComponent<AudioSource>();
      xylophones.clip = _audioClip;
      xylophones.volume = volume;
      xylophones.spatialBlend = 0f;
      xylophones.outputAudioMixerGroup = null;
      xylophones.playOnAwake = false;
      xylophones.pitch = 0.95f +pitch;


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


  private void OnClicked()
  {
    if (_gameManager.hits.Length <= 0) return;

    foreach (var _ray in _gameManager.hits)
    {
      if (_gameManager == null)
      {
        Debug.Log("_gameManager is null");
        return;
      }

      if (_ray.transform == null)
      {
        Debug.Log("_ray.transform is null");
        return;
      }

      var rayGameObject = _ray.transform.gameObject;
      if (rayGameObject == null)
      {
        Debug.Log("rayGameObject is null");
        return;
      }
        

      if (_ray.transform.gameObject.TryGetComponent(out AudioSource currentAudioSource))
        currentAudioSource.Play();

      currentAudioSource.DOFade(1, 0.15f)
        .OnComplete(() => { FadeOutSound(currentAudioSource); });
      
      return;

    }
      
    
  }
   
    
  
  
  
  protected virtual void OnDestroy()
  {
    Music_GameManager.eventAfterAGetRay -= OnClicked;
  }
    
  protected virtual void BindEvent()
  {
    Music_GameManager.eventAfterAGetRay -= OnClicked;
    Music_GameManager.eventAfterAGetRay += OnClicked;
  }
}

