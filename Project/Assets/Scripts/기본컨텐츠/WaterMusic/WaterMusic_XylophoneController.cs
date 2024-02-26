using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;
public class WaterMusic_XylophoneController : MonoBehaviour
{

    [Header("Reference")] 
    [SerializeField] private WaterMusic_GameManager _gameManager;

    private Transform _soundProducingXylophoneParent;
    private Transform[] _soundProducingXylophones;
    public Transform[] allXylophones;
    public Vector3[] targetPos;

    [Header("Color")]
    public Color[] colorsToChange;

    [Space(10f)] [Header("position setting")]
    public Vector3 defaultOffset;

    // semi-singleton
    private bool _isInit;

    
  
    private int TOTAL_SOUNDABLE_COUNT;
    private readonly int SOUND_PRODUCING_CHILD_COUNT = 8;

    // Cache Audio Source
    //클릭시, gameObjecet.name(string)값에 대한 AudioSource를 플레이하기 위해 자료사전을 사용합니다. 
   
    private Dictionary<int, string> noteSemitones;
    private Dictionary<string, MeshRenderer> _materialMap;

    private Dictionary<int, Quaternion> _defaultRotationMap;
    //디폴트로 돌아가기 위한 자료사전
    private Dictionary<MeshRenderer, Color> _defaultColorMap;
    //바뀔 컬러 캐싱용 

    private readonly string AUDIO_XYLOPHONE_PATH = "게임별분류/기본컨텐츠/SkyMusic/Audio/Piano/";
    private readonly int BASE_MAP = Shader.PropertyToID("_BaseColor");

    private MeshRenderer[] _xylophoneMeshRenderers;

    private void Awake()
    {
        BindEvent();


        allXylophones = GetComponentsInChildren<Transform>();
   
        _materialMap = new Dictionary<string, MeshRenderer>();
        _defaultColorMap = new Dictionary<MeshRenderer, Color>();
        _isTweening = new Dictionary<int, bool>();
        _defaultRotationMap = new Dictionary<int, Quaternion>();
        
        //클릭시 사운드가 나는 실로폰은 위치정보 + 사운드소스 참조가 필요하기 때문에, 따로 다른 배열로 구성합니다.
        _soundProducingXylophoneParent = GameObject.Find("SoundableObjects").transform;
      
        TOTAL_SOUNDABLE_COUNT = _soundProducingXylophoneParent.childCount;
        
        targetPos = new Vector3[TOTAL_SOUNDABLE_COUNT];
        _soundProducingXylophones = new Transform[TOTAL_SOUNDABLE_COUNT];
       
        for (int i = 0; i < TOTAL_SOUNDABLE_COUNT; i++)
        {
            _soundProducingXylophones[i] = _soundProducingXylophoneParent.GetChild(i);
            var instanceID = _soundProducingXylophones[i].GetInstanceID();
            _isTweening.TryAdd(instanceID, false);
            _defaultRotationMap.TryAdd(instanceID, _soundProducingXylophones[i].rotation);
        }
        
        
        
        _xylophoneMeshRenderers = new MeshRenderer[SOUND_PRODUCING_CHILD_COUNT];
        _gameManager = GameObject.FindWithTag("GameManager").GetComponent<WaterMusic_GameManager>();

    }


    private void Start()
    {
        if (!_isInit) Init();
        
        
    }

    private bool _isIncreasingWay;

    private void Update()
    {
        _playCurrentTime += Time.deltaTime;
        
    }
    

    private void Init()
    {
#if UNITY_EDITOR
        Debug.Log($"soundable객체 수: {TOTAL_SOUNDABLE_COUNT}, targetPos.Length : {targetPos.Length}  _soundProducingXylophones: {_soundProducingXylophones.Length}");
#endif
        for (var i = 0; i < TOTAL_SOUNDABLE_COUNT; i++)
        {
            targetPos[i] = _soundProducingXylophones[i].transform.position;
        }
        var count = 0;

        //초기 위치 설정
        foreach (var x in allXylophones)
            if (x.gameObject.name != "Xylophones" || x.gameObject.name.Length <= 2)
            {
                x.transform.position += defaultOffset;
            }
       
        count = 0;


        _isInit = true;
    }

    private readonly float _interval = 0.03f;




    public int audioClipCount;


    private void DoIntroMove()
    {
        for (var i = 0; i < TOTAL_SOUNDABLE_COUNT; ++i)
        {
            var i1 = i;
            _soundProducingXylophones[i].transform.DOMove(targetPos[i], 0.6f + _interval * i)
                .SetEase(Ease.OutBack)
                .SetDelay(1f+ _interval * i)
                .OnStart(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect,
                        "Audio/기본컨텐츠/WaterMusic/" + _soundProducingXylophones[i1].transform.gameObject.name, 0.35f);
                });
            
       
          
        }
           
        
        
    }

    
    private RaycastHit RayHitForXylophone;
    private Ray _ray;

    private void OnClicked()
    {
   

        //layermask 외부에서 설정 X.
        var layerMask = LayerMask.GetMask("Default");

        

        if (Physics.Raycast(WaterMusic_GameManager.GameManager_Ray, out RayHitForXylophone, Mathf.Infinity, layerMask))
        {

            var random = Random.Range(0, 100);
            if(random > 20)
            {
                
                
                DoClickMove(RayHitForXylophone.transform);
            }
            else
            {
                
                DoClickMoveDeeper(RayHitForXylophone.transform);
            }
            
        }

     

    }

    private Dictionary<int, bool> _isTweening;
    private void DoClickMove(Transform trans)
    {
    
        var currentID = trans.GetInstanceID();
        if (_isTweening[currentID]) return;
        _isTweening[trans.GetInstanceID()] = true;
#if UNITY_EDITOR
        Debug.Log("Clicked");
#endif
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/WaterMusic/"+RayHitForXylophone.transform.gameObject.name,0.3f);
   
        trans.DOShakeRotation(1f, 1f);
        
        var defaultPos = trans.position;
        trans.DOMove(trans.position+ Vector3.down * 0.8f, 1f).SetEase(Ease.InOutBack)
            .OnStart(() =>
            {
                
            })
            .OnComplete(() =>
            {
                _isTweening[trans.GetInstanceID()] = false;
                trans.DOMove(defaultPos, 0.3f);
            });
    }

    private void DoClickMoveDeeper(Transform trans)
    {
#if UNITY_EDITOR
        Debug.Log("Clicked");
#endif
        var currentID = trans.GetInstanceID();
        if (_isTweening[currentID]) return;
        _isTweening[currentID] = true;
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/WaterMusic/"+RayHitForXylophone.transform.gameObject.name,0.3f);
        trans.DORotateQuaternion(_defaultRotationMap[currentID]*Quaternion.Euler(40, 0, 0),  1f);
        
        var defaultPos = trans.position;
        trans.DOMove(trans.position+ Vector3.down * 3.8f, 1f).SetEase(Ease.InOutBack)
            .OnStart(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/WaterMusic/Deeper",
                    0.5f);
            })
            .OnComplete(() =>
            {
                trans.DORotateQuaternion(_defaultRotationMap[currentID], 1f);
                _isTweening[currentID] = false;
                trans.DOMove(defaultPos, 0.3f);
            });
    }


    protected virtual void BindEvent()
    {
        WaterMusic_GameManager.On_GmRay_Synced -= OnClicked;
        WaterMusic_GameManager.On_GmRay_Synced += OnClicked;

        UI_Scene_Button.onBtnShut -= DoIntroMove;
        UI_Scene_Button.onBtnShut += DoIntroMove;
    }


    public float brightenIntensity;
    private Color brightenedColor;


    private Coroutine _xylophoneCoroutine;


    public float playWaitTime;
    private float _playCurrentTime;
    private WaitForSeconds xylophoneInerval;



}
