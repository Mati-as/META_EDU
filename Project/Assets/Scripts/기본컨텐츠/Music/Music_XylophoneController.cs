using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Music_XylophoneController : MonoBehaviour
{
    [Header("Reference")] [SerializeField] private Music_GameManager _gameManager;
    public Transform[] soundProducingXylophones;
    public Transform[] allXylophones;
    public Vector3[] targetPos;

    
    [Space(10f)] 
    [Header("position setting")] public Vector3 defaultOffset;
    // semi-singleton
    private bool _isInit;

    
    [Space(10f)] 
    [Header("sound")] public float pitchInterval;
    public int audioSize;
    public int volumeA;
    private AudioClip _audioClip;
    private AudioSource[] _audioSourcesA;
    private int _totalChildCount;
    private readonly int SOUND_PRODUCING_CHILD_COUNT = 9;

    // Cache Audio Source
    private Dictionary<string, AudioSource> audioSourceMap;
    private Dictionary<string, int> noteSemitones;
    private Dictionary<string, MeshRenderer> _materialMap;
    private Dictionary<MeshRenderer, Color> _defaultColorMap;
      
    private readonly string AUDIO_XYLOPHONE_PATH = "게임별분류/기본컨텐츠/SkyMusic/Audio/Xylophone";
    private readonly int BASE_MAP = Shader.PropertyToID("_BaseColor");
    

    private void Awake()
    {
        BindEvent();

        allXylophones = GetComponentsInChildren<Transform>();
        audioSourceMap = new Dictionary<string, AudioSource>();
        _materialMap = new Dictionary<string, MeshRenderer>();
        _defaultColorMap = new Dictionary<MeshRenderer, Color>();

        _totalChildCount = transform.childCount;
        targetPos = new Vector3[_totalChildCount];


        //클릭시 사운드가 나는 실로폰은 위치정보 + 사운드소스 참조가 필요하기 때문에, 따로 다른 배열로 구성합니다.
        soundProducingXylophones = new Transform[SOUND_PRODUCING_CHILD_COUNT];
        var allTransforms = GetComponentsInChildren<Transform>();
        var childTransforms = allTransforms.Where(t => t != transform).ToArray();
        var numberOfChildrenToTake = Mathf.Min(childTransforms.Length, 8);
        var selectedTransforms = childTransforms.Take(numberOfChildrenToTake).ToArray();
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
        for (var i = 0; i < _totalChildCount; i++) targetPos[i] = allXylophones[i].transform.position;

        var count = 0;


        //초기 위치 설정
        foreach (var x in soundProducingXylophones)
            if (x.gameObject.name != "Xylophones" || x.gameObject.name.Length <= 2)
            {
                var audioSource = new AudioSource();
                InitializeAudioSource(x.transform, audioSource);
                audioSourceMap.TryAdd(x.gameObject.name, audioSource);
                count++;
            }

        count = 0;
        foreach (var x in allXylophones)
            if (x.gameObject.name != "Xylophones" || x.gameObject.name.Length <= 2)
            {
                x.transform.position += defaultOffset;

                count++;
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


    private float GetPitchForNote(string note)
    {
        if (noteSemitones == null)
            noteSemitones = new Dictionary<string, int>
            {
                { "C", 0 }, { "C#", 1 }, { "D", 2 }, { "D#", 3 }, { "E", 4 }, { "F", 5 },
                { "F#", 6 }, { "G", 7 }, { "G#", 8 }, { "A", 9 }, { "A#", 10 }, { "B", 11 },
                { "C2", 12 }
            };

        // 중간 C에서 목표 음까지의 반음 수
        var semitonesFromC4 = noteSemitones[note];

        // pitch 계산
        var pitch = Mathf.Pow(1.059463f, semitonesFromC4);
        return pitch;
    }

    private void InitializeAudioSource(Transform _transform, AudioSource xylophones, float volume = 1f)
    {
        var _audioClip = LoadSound(AUDIO_XYLOPHONE_PATH);

        for (var i = 0; i < audioClipCount; ++i)
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

    private void OnClicked()
    {
        _ray = _gameManager.ray_GameManager;

        //layermask 외부에서 설정 X.
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
            _materialMap.Add(clickedObjName, clickedMeshRenderer);
            _defaultColorMap.Add(clickedMeshRenderer, clickedMeshRenderer.material.color);
        }

        ChangeColor(_materialMap[clickedObjName]);

        FindAndPlayAudio(currentAudioSources);
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


    public float brightenIntensity;

    private void ChangeColor(MeshRenderer meshRenderer, float duration = 0.8f)
    {
        var thisMaterial = new Material(meshRenderer.material);
        var defaultColor = thisMaterial.color;


        //중복 곱 방지
        var brightenedColor = defaultColor * brightenIntensity;


        thisMaterial.DOColor(brightenedColor, BASE_MAP, duration).OnComplete(() =>
        {
            thisMaterial.DOColor(_defaultColorMap[meshRenderer], BASE_MAP, duration);
        });

        meshRenderer.material = thisMaterial;



    }



}