using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Music_XylophoneController : MonoBehaviour
{
    [FormerlySerializedAs("_gameManager")] [Header("Reference")] [SerializeField] private MusicBaseGameManager baseGameManager;
    public Transform[] soundProducingXylophones;
    public Transform[] allXylophones;
    public Vector3[] targetPos;

    [Header("Color")]
    public Color[] colorsToChange;

    [Space(10f)] [Header("position setting")]
    public Vector3 defaultOffset;

    // semi-singleton
    private bool _isInit;


    [Space(10f)] [Header("sound")] public float pitchInterval;
    public int audioSize;
    public int volumeA;
    private AudioClip _audioClip;

    //순서대로 피아노 음원 재생을 위해 배열로 캐싱합니다.
    private AudioSource[] _xylophoneAudioSources;
    private int _totalChildCount;
    private readonly int SOUND_PRODUCING_CHILD_COUNT = 8;

    // Cache Audio Source
    //클릭시, gameObjecet.name(string)값에 대한 AudioSource를 플레이하기 위해 자료사전을 사용합니다. 
    private Dictionary<string, AudioSource> audioSourceMap;
    private Dictionary<int, string> noteSemitones;
    private Dictionary<string, MeshRenderer> _materialMap;
    //디폴트로 돌아가기 위한 자료사전
    private Dictionary<MeshRenderer, Color> _defaultColorMap;
    //바뀔 컬러 캐싱용 
    private Dictionary<Transform, Color> _colorChangeMap;

    private readonly string AUDIO_XYLOPHONE_PATH = "게임별분류/기본컨텐츠/SkyMusic/Audio/Piano/";
    private readonly int BASE_MAP = Shader.PropertyToID("_BaseColor");

    private MeshRenderer[] _xylophoneMeshRenderers;

    private void Awake()
    {
        BindEvent();


        allXylophones = GetComponentsInChildren<Transform>();
        audioSourceMap = new Dictionary<string, AudioSource>();
        _materialMap = new Dictionary<string, MeshRenderer>();
        _defaultColorMap = new Dictionary<MeshRenderer, Color>();
        _colorChangeMap = new Dictionary<Transform, Color>();

    

        _totalChildCount = transform.childCount;
        targetPos = new Vector3[_totalChildCount];


        //클릭시 사운드가 나는 실로폰은 위치정보 + 사운드소스 참조가 필요하기 때문에, 따로 다른 배열로 구성합니다.
        soundProducingXylophones = new Transform[SOUND_PRODUCING_CHILD_COUNT];
        _xylophoneAudioSources = new AudioSource[SOUND_PRODUCING_CHILD_COUNT];
        _xylophoneMeshRenderers = new MeshRenderer[SOUND_PRODUCING_CHILD_COUNT];

        var allTransforms = GetComponentsInChildren<Transform>();
        var childTransforms = allTransforms.Where(t => t != transform).ToArray();
        var numberOfChildrenToTake = Mathf.Min(childTransforms.Length, 8);
        var selectedTransforms = childTransforms.Take(numberOfChildrenToTake).ToArray();

        soundProducingXylophones = selectedTransforms;
        for (int i = 0; i < soundProducingXylophones.Length; i++)
        {
            _colorChangeMap.Add(soundProducingXylophones[i],colorsToChange[i]);
#if UNITY_EDITOR
            // Debug.Log($"color map caching.. {soundProducingXylophones[i].gameObject.name},{colorsToChange[i]}");
#endif
        }

    }


    private void Start()
    {
        if (!_isInit) Init();

             
   

        DoIntroMove();
    }

    private bool _isIncreasingWay;

    private void Update()
    {
        _playCurrentTime += Time.deltaTime;

        if (_playCurrentTime > playWaitTime) StartPlayAutomaticallyCoroutine(_isIncreasingWay);
    }


    private AudioSource[] SetAudio(AudioSource[] audioSources, string path, float volume = 0.5f)
    {
        //int - 파일경로 딕셔너리 입니다.
        noteSemitones = new Dictionary<int, string>
        {
            { 0, "C" },{ 1, "D" },{ 2, "E" },{ 3, "F" },{ 4, "G" },{ 5, "A" },{ 6, "B" },{ 7, "C2" }
        };

        audioSources = new AudioSource[audioSources.Length];
        for (var i = 0; i < audioSources.Length; ++i)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            var _audioClip = LoadSound(path + noteSemitones[i]);
            audioSources[i].clip = _audioClip;
            audioSources[i].volume = volume;
            audioSources[i].spatialBlend = 0f;
            audioSources[i].outputAudioMixerGroup = null;
            audioSources[i].playOnAwake = false;
           
        }

        return audioSources;
    }

    private void Init()
    {
        for (var i = 0; i < _totalChildCount; i++) targetPos[i] = allXylophones[i].transform.position;
        var count = 0;

        //초기 위치 설정
        foreach (var x in soundProducingXylophones)
            if (x.gameObject.name != "Xylophones" || x.gameObject.name.Length <= 2)
            {
                var audioSource = new AudioSource();
                InitializeAudioSource(x.transform, audioSource);
                audioSourceMap.TryAdd(x.gameObject.name, audioSource);
            }
        
        
        foreach (var x in allXylophones)
            if (x.gameObject.name != "Xylophones" || x.gameObject.name.Length <= 2)
            {
                x.transform.position += defaultOffset;
            }

        _xylophoneAudioSources = SetAudio(_xylophoneAudioSources, AUDIO_XYLOPHONE_PATH);
        
        
        count = 0;
        foreach (var xyl in soundProducingXylophones)
        {
            if (xyl.gameObject.name != "Xylophones" || xyl.gameObject.name.Length <= 2)
            {
                var thisMeshRenderer = xyl.gameObject.GetComponent<MeshRenderer>();
                if (thisMeshRenderer != null)
                {
                    _xylophoneMeshRenderers[count] = thisMeshRenderer;
                }
                else
                {
                    // MeshRenderer가 없는 경우에 대한 처리
                    Debug.LogError("MeshRenderer가 없는 GameObject 발견: " + xyl.gameObject.name);
                }

                count++;
            }
        }

        _isInit = true;
    }

    private readonly float _interval = 0.4f;

    private void DoIntroMove()
    {
        for (var i = 0; i < _totalChildCount; ++i)
            allXylophones[i].transform.DOMove(targetPos[i], 0.5f + _interval * i)
                .SetDelay(2f);
    }

    private AudioClip LoadSound(string path)
    {
        var _audioClip = Resources.Load<AudioClip>(path);
        return _audioClip;
    }


    public int audioClipCount;



    private void InitializeAudioSource(Transform _transform, AudioSource xylophones, float volume = 1f)
    {
       
        
        var _audioClip = LoadSound(AUDIO_XYLOPHONE_PATH + _transform.gameObject.name);
        
        
#if UNITY_EDITOR
       if(_audioClip == null)
        Debug.Log("이니셜 오디오소스 널 에러");
#endif
        for (var i = 0; i < audioClipCount; ++i)
        {
            GameObject gameObj;
            xylophones = (gameObj = _transform.gameObject).AddComponent<AudioSource>();
            xylophones.clip = _audioClip;
            xylophones.volume = volume;
            xylophones.spatialBlend = 0f;
            xylophones.outputAudioMixerGroup = null;
            xylophones.playOnAwake = false;
            xylophones.pitch = 1;
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

    protected void FadeInSound(AudioSource audioSource, float target = 0.3f, float fadeInDuration = 0.8f
        , float delay = 1f)
    {
        audioSource.Play();
        audioSource.DOFade(target, fadeInDuration).OnComplete(() =>
        {
#if UNITY_EDITOR
#endif
        }).OnComplete(() => { audioSource.Stop(); });
    }

    private RaycastHit RayForXylophone;
    private Ray _ray;

    private bool _isPianoAutomaticallyPlaying;
    public bool isBubbleAboveXyl { get; set; }
    private void OnClicked()
    {

        if (isBubbleAboveXyl)
        {
            // Debug.Log("bubble is on the xyl");
            return;
        }
        if (_isPianoAutomaticallyPlaying) return;

        //layermask 외부에서 설정 X.
        var layerMask = LayerMask.GetMask("Default");

      //  if (Music_GameManager.GameManager_Hits.Length <= 0) return;
        if (Physics.Raycast(MusicBaseGameManager.GameManager_Ray, out RayForXylophone, Mathf.Infinity, layerMask))

            if (baseGameManager == null)
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
#if UNITY_EDITOR
            Debug.Log("rayGameObject is null");
#endif

            return;
        }

        var clickedObjName = RayForXylophone.transform.gameObject.name;

        // string.length로 판단하는 로직은 유지보수난이도 상승.. 로직 변경필요. 1-10-24
        if (clickedObjName.Length > 3)
        {
            Debug.Log("this object is not xylophone, return..");
            return;
        }


        AudioSource[] currentAudioSources;
        currentAudioSources = RayForXylophone.transform.gameObject.GetComponents<AudioSource>();


        //TryAdd 변경 금지;에러 발생 1-10-24
        if (!_materialMap.ContainsKey(clickedObjName))
        {
            MeshRenderer clickedMeshRenderer;
            RayForXylophone.transform.gameObject.TryGetComponent(out clickedMeshRenderer);
            _materialMap.TryAdd(clickedObjName, clickedMeshRenderer);
            _defaultColorMap.TryAdd(clickedMeshRenderer, clickedMeshRenderer.material.color);
        }

        ChangeColor(_materialMap[clickedObjName],_colorChangeMap[RayForXylophone.transform]);

        FindAndPlayAudio(currentAudioSources);
    }


    protected virtual void OnDestroy()
    {
        MusicBaseGameManager.On_GmRay_Synced -= OnClicked;
    }

    protected virtual void BindEvent()
    {
        MusicBaseGameManager.On_GmRay_Synced -= OnClicked;
        MusicBaseGameManager.On_GmRay_Synced += OnClicked;
    }


    public float brightenIntensity;
    private Color brightenedColor;

    private void ChangeColor(MeshRenderer meshRenderer,Color brightendColor, float duration = 0.7f)
    {
        var thisMaterial = new Material(meshRenderer.material);
        
        _defaultColorMap.TryAdd(meshRenderer,thisMaterial.color);
        
        var defaultColor = thisMaterial.color;
        
        brightenedColor = new Color();

        brightenedColor = brightendColor;
        
        thisMaterial.DOColor(brightenedColor, BASE_MAP, duration).OnComplete(() =>
        {
            thisMaterial.DOColor(_defaultColorMap[meshRenderer], BASE_MAP, duration);
        });
     


        meshRenderer.material = thisMaterial;
    }

    private Coroutine _xylophoneCoroutine;

    private void StartPlayAutomaticallyCoroutine(bool isIncrease)
    {
        var i = Random.Range(0, 2);
        switch (i)
        {
            case 0:
                isIncrease = true;
                break;
            case 1:
                isIncrease = false;
                break;
        }

        //_xylophoneCoroutine = 
        StartCoroutine(PlayAutomatically(isIncrease));
    }


    public float playWaitTime;
    private float _playCurrentTime;
    private WaitForSeconds xylophoneInerval;

    private IEnumerator PlayAutomatically(bool isIncrease)
    {
        _isPianoAutomaticallyPlaying = true;
        _playCurrentTime = 0;
        
        //처음 시작에만 짧은 Interval로 동작하고 이후에는 아래처럼 랜덤으로 동작
        playWaitTime = Random.Range(10, 20);

#if UNITY_EDITOR
Debug.Log($"렌더러 Length{_xylophoneMeshRenderers.Length}");
Debug.Log($"오디오 Length{_xylophoneAudioSources.Length}");
#endif
        for (var i = 0; i < _xylophoneAudioSources.Length; i++)
        {
            if (isIncrease)
            {
                FadeInSound(_xylophoneAudioSources[i]);
                ChangeColor(_xylophoneMeshRenderers[i], colorsToChange[i]);
            }

            else
            {
                FadeInSound(_xylophoneAudioSources[_xylophoneAudioSources.Length - i - 1]);
                ChangeColor(_xylophoneMeshRenderers[_xylophoneAudioSources.Length - i - 1], colorsToChange[_xylophoneAudioSources.Length - i - 1]);
            }


            if (xylophoneInerval == null) xylophoneInerval = new WaitForSeconds(0.093f);

            yield return xylophoneInerval;
        }

        DOVirtual.Float(0, 0, 1.0f, _ => { }).OnComplete(() =>
        {
            _isPianoAutomaticallyPlaying = false;
        });

    }
}