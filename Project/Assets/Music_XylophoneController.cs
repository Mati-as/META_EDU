using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Music_XylophoneController : MonoBehaviour
{
  [Header("Reference")] [SerializeField] private Music_GameManager _gameManager;
  
   public Transform[] soundProducingXylophones;
   public Transform[] allXylophones;
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
  private int TOTAL_CHILD_COUNT;
  private int SOUND_PRODUCING_CHILD_COUNT = 9;
  // 오디오 소스 캐싱
  private Dictionary<string, AudioSource> audioSourceMap;
  private Dictionary<string, int> noteSemitones;
  private string _path = "게임별분류/기본컨텐츠/Music";

  private void Awake()
  {
    BindEvent();
    // 음과 중간 C (C4) 사이의 반음 수

    allXylophones = GetComponentsInChildren<Transform>();
    audioSourceMap = new Dictionary<string, AudioSource>();

    TOTAL_CHILD_COUNT  = transform.childCount;
    targetPos = new Vector3[TOTAL_CHILD_COUNT];
    
    
    soundProducingXylophones = new Transform[SOUND_PRODUCING_CHILD_COUNT];

    Transform[] allTransforms = GetComponentsInChildren<Transform>();


    var childTransforms = allTransforms.Where(t => t != transform).ToArray();


    int numberOfChildrenToTake = Mathf.Min(childTransforms.Length, 8);
    Transform[] selectedTransforms = childTransforms.Take(numberOfChildrenToTake).ToArray();


    soundProducingXylophones = selectedTransforms;
  
  }


  private void Start()
  {
    if (!_isInit) Init();

  
    DoIntroMove();
  }


  protected virtual void SetAudio()
  {
   
  }
  

  private void Init()
  {


      
    for (int i = 0;i < TOTAL_CHILD_COUNT ; i++)
    {
      targetPos[i] = allXylophones[i].transform.position;
    }

    int count = 0;
   
    
    //초기 위치 설정
    foreach (var x in soundProducingXylophones)
    {
      if (x.gameObject.name != "Xylophones" || x.gameObject.name.Length <= 2)
      {
    
        AudioSource audioSource = new AudioSource();
        Initialize(x.transform,audioSource);
        audioSourceMap.TryAdd(x.gameObject.name, audioSource);
        count++;
      }
    
    }

    count = 0;
    foreach (var x in allXylophones)
    {
      if (x.gameObject.name != "Xylophones" || x.gameObject.name.Length <= 2)
      {
        x.transform.position += defaultOffset;
       
        count++;
      }
    
    }
    
    _isInit = true;
   
  }

  private float _interval= 0.4f;

  private void DoIntroMove()
  {
    for (int i = 0; i < TOTAL_CHILD_COUNT ; ++i)
    {
      allXylophones[i].transform.DOMove(targetPos[i], 0.5f + _interval* i)
        .SetDelay(2f);
    }
  }

  private AudioClip LoadSound(string path)
  {
    AudioClip _audioClip = Resources.Load<AudioClip>(path);
    return _audioClip;
  }


  public int audioClipCount;
  private void Initialize(Transform _transform,AudioSource xylophones, float volume = 1f)
  {
      
     AudioClip _audioClip = LoadSound("게임별분류/기본컨텐츠/Music/Xylophone");

     for (int i = 0; i < audioClipCount; ++i)
     {
       GameObject gameObj;
       xylophones = (gameObj = _transform.gameObject).AddComponent<AudioSource>();
       xylophones.clip = _audioClip;
       xylophones.volume = volume;
       xylophones.spatialBlend = 0f;
       xylophones.outputAudioMixerGroup = null;
       xylophones.playOnAwake = false;
       xylophones.pitch = GetPitchForNote(gameObj.name);

     }


  }

  protected void FindAndPlayAudio(AudioSource[] audioSources,
    float volume = 0.8f)
  {
    
      var availableAudioSource = Array.Find(audioSources, audioSource => !audioSource.isPlaying);

      if (availableAudioSource != null) FadeInSound(availableAudioSource, volume);

#if UNITY_EDITOR
    Debug.Log("재생");
#endif

  }
  float GetPitchForNote(string note)
  {
    if (noteSemitones == null)
    {
      noteSemitones = new Dictionary<string, int>
      {
        {"C", 0}, {"C#", 1}, {"D", 2}, {"D#", 3}, {"E", 4}, {"F", 5},
        {"F#", 6}, {"G", 7}, {"G#", 8}, {"A", 9}, {"A#", 10}, {"B", 11},
        {"C2", 12}
      };
    }

    // 중간 C에서 목표 음까지의 반음 수
    int semitonesFromC4 = noteSemitones[note];

    // pitch 계산
    float pitch = Mathf.Pow(1.059463f, semitonesFromC4);
    return pitch;
  }


  protected void FadeInSound(AudioSource audioSource, float target = 0.3f, float fadeInDuration = 0.8f
    ,float delay =1f)
  {
    audioSource.Play();
    audioSource.DOFade(target, fadeInDuration).OnComplete(() =>
    {
#if UNITY_EDITOR
#endif
      _isProducingSound = true;
      
    }).OnComplete(() =>
    {
      audioSource.Stop();
    });
  }

  private RaycastHit RayForXylophone;
  private Ray _ray;

  private void OnClicked()
  {
    _ray = _gameManager.ray_GameManager;

    var layerMask = LayerMask.GetMask("Default");

    if (_gameManager.hits.Length <= 0) return;
    if (Physics.Raycast(_ray, out RayForXylophone, Mathf.Infinity, layerMask))

      if (_gameManager == null)
      {
        Debug.Log("_gameManager is null");
        return;
      }

    if (RayForXylophone.transform == null)
    {
      Debug.Log("_ray.transform is null");
      return;
    }

    var rayGameObject = RayForXylophone.transform.gameObject;
    if (rayGameObject == null)
    {
      Debug.Log("rayGameObject is null");
      return;
    }


    AudioSource[] currentAudioSources;
    currentAudioSources = RayForXylophone.transform.gameObject.GetComponents<AudioSource>();
      
    FindAndPlayAudio(currentAudioSources);
  }

  private void OnDrawGizmos()
  {
    var layerMask = LayerMask.GetMask("Screen");
    if (Physics.Raycast(_ray, out RayForXylophone, Mathf.Infinity, layerMask))
    {
      // 레이를 그립니다.
      Gizmos.color = Color.blue;
      Gizmos.DrawLine(_ray.origin, RayForXylophone.point);

      // 히트 지점에 작은 구체를 그립니다.
      Gizmos.color = Color.magenta;
      Gizmos.DrawSphere(RayForXylophone.point,0.15f);
    }
  }
  
  
  private bool _isProducingSound;
    
  
  
  
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

