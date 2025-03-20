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
public class WaterMusic_XylophoneController : MonoBehaviour
{

    [FormerlySerializedAs("_gameManager")]
    [Header("Reference")] 
    [SerializeField] private WaterMusicBaseGameManager baseGameManager;

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
    
    private Dictionary<int, Material> materalMap;
    
    private Dictionary<int, Quaternion> _defaultRotationMap;
    //디폴트로 돌아가기 위한 자료사전
    private Dictionary<int, Color> _defaultColorMap;
    //바뀔 컬러 캐싱용
    private Dictionary<int, Color> _colorChangeMap;
    private readonly string AUDIO_XYLOPHONE_PATH = "SortedbyGame/BasicContents/SkyMusic/Audio/Piano/";
    private readonly int BASE_MAP = Shader.PropertyToID("_BaseColor");

    private MeshRenderer[] _xylophoneMeshRenderers;

    private void Awake()
    {
        BindEvent();


        allXylophones = GetComponentsInChildren<Transform>();
   
        materalMap = new Dictionary<int, Material>();
        _defaultColorMap = new Dictionary<int, Color>();
        _isTweening = new Dictionary<int, bool>();
        _defaultRotationMap = new Dictionary<int, Quaternion>();
        _colorChangeMap = new Dictionary<int, Color>();
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
            _colorChangeMap.TryAdd(instanceID,colorsToChange[i]);
        }
  
        
        _xylophoneMeshRenderers = new MeshRenderer[SOUND_PRODUCING_CHILD_COUNT];
        baseGameManager = GameObject.FindWithTag("GameManager").GetComponent<WaterMusicBaseGameManager>();

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
        _clickableMap = new Dictionary<int, bool>();
#if UNITY_EDITOR
        Debug.Log($"soundable객체 수: {TOTAL_SOUNDABLE_COUNT}, targetPos.Length : {targetPos.Length}  _soundProducingXylophones: {_soundProducingXylophones.Length}");
#endif
        for (var i = 0; i < TOTAL_SOUNDABLE_COUNT; i++)
        {
            targetPos[i] = _soundProducingXylophones[i].transform.position;
            var instanceID = _soundProducingXylophones[i].GetInstanceID();

            MeshRenderer meshRenderer = null;
            _soundProducingXylophones[i].TryGetComponent(out meshRenderer);
            if (meshRenderer != null)
            {
                materalMap.TryAdd(instanceID,meshRenderer.material);
                _defaultColorMap.TryAdd(instanceID, materalMap[instanceID].color);
            }
            
            SkinnedMeshRenderer skinnedMeshRenderer = null;
            _soundProducingXylophones[i].TryGetComponent(out skinnedMeshRenderer);
            if (skinnedMeshRenderer != null)
            {
                materalMap.TryAdd(instanceID,skinnedMeshRenderer.material);
                _defaultColorMap.TryAdd(instanceID, materalMap[instanceID].color);
            }
            
           
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

    private Material ChangeColor(Transform transform, float duration = 0.5f)
    {
        var ID = transform.GetInstanceID();
        if(!materalMap.ContainsKey(ID)) return null;
        
        var thisMaterial = new Material(materalMap[ID]);
        
        
        thisMaterial.DOColor(_colorChangeMap[ID] * brightenIntensity, BASE_MAP, duration).OnComplete(() =>
        {
            thisMaterial.DOColor(_defaultColorMap[transform.GetInstanceID()],
                BASE_MAP, duration);
        });
     
#if UNITY_EDITOR
        Debug.Log($"color map ");
#endif


        return thisMaterial;
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
                        "Audio/BasicContents/WaterMusic/" + _soundProducingXylophones[i1].transform.gameObject.name, 0.35f);
                });
            
       
          
        }
           
        
        
    }

    
    private RaycastHit RayHitForXylophone;
    private Ray _ray;
    private bool _isMoveTweening;
    private Dictionary<int, bool> _clickableMap; //클릭횟수
    private WaitForSeconds _resetWait = new WaitForSeconds(0.75f); 

    private void ResetClickabeWithDelay(int instanceID)
    {
        StartCoroutine(ResetClicableWithDelayCo(instanceID));
    }

    IEnumerator ResetClicableWithDelayCo(int instanceID)
    {
        _clickableMap[instanceID] = false;
        yield return _resetWait;
        _clickableMap[instanceID] = true;
    }

    private void OnClicked()
    {
   

        //layermask 외부에서 설정 X.
        var layerMask = LayerMask.GetMask("Default");

        

        if (Physics.Raycast(WaterMusicBaseGameManager.GameManager_Ray, out RayHitForXylophone, Mathf.Infinity, layerMask))
        {
            var id = RayHitForXylophone.transform.GetInstanceID();
            _clickableMap.TryAdd(id, true);
            if (!_clickableMap[id]) return;

            ResetClickabeWithDelay(id);
            
            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/WaterMusic/"+RayHitForXylophone.transform.gameObject.name,0.3f);
   
            
            MeshRenderer meshRenderer = null;
            SkinnedMeshRenderer skinnedMeshRenderer = null;
            var random = Random.Range(0, 100);
            if(random > 20)
            {
               
                RayHitForXylophone.transform.TryGetComponent(out meshRenderer);
                if (meshRenderer != null)
                {
                    meshRenderer.material = ChangeColor(RayHitForXylophone.transform);
                    DoClickMove(RayHitForXylophone.transform);
                }
                
                skinnedMeshRenderer = null;
                RayHitForXylophone.transform.TryGetComponent(out skinnedMeshRenderer);
                if (skinnedMeshRenderer != null)
                {
                    skinnedMeshRenderer.material = ChangeColor(RayHitForXylophone.transform);
                    DoClickMove(RayHitForXylophone.transform);
                }
                
               
               
            }
            else
            {
               
                   RayHitForXylophone.transform.TryGetComponent(out meshRenderer);
                if (meshRenderer != null)
                {
                    meshRenderer.material = ChangeColor(RayHitForXylophone.transform);
                    DoClickMoveDeeper(RayHitForXylophone.transform);
                }
                
                skinnedMeshRenderer = null;
                RayHitForXylophone.transform.TryGetComponent(out skinnedMeshRenderer);
                if (skinnedMeshRenderer != null)
                {
                    skinnedMeshRenderer.material = ChangeColor(RayHitForXylophone.transform);
                    DoClickMoveDeeper(RayHitForXylophone.transform);
                }
                
             
            }


            baseGameManager.DEV_validClick++;
        }
        else
        {
            baseGameManager.DEV_sensorClick++;
        }

     

    }

    private Dictionary<int, bool> _isTweening;
    private void DoClickMove(Transform trans)
    {
    
        var currentID = trans.GetInstanceID();
        if (_isTweening[currentID]) return;
        _isTweening[currentID] = true;
        
#if UNITY_EDITOR
        Debug.Log("Clicked");
#endif
      
   
        trans.DOShakeRotation(1f, 1f).OnComplete(() =>
        {
          
        });
        
        var defaultPos = trans.position;
        trans.DOMove(trans.position + Vector3.down * 0.8f, 1f).SetEase(Ease.InOutBack).OnComplete(() =>
        {
            trans.DOMove(defaultPos, 0.3f).OnComplete(() =>
            {
                DOVirtual.Float(0, 0, 0.6f, _ => { }).OnComplete(() => { _isTweening[currentID] = false; });
            });
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
        


        Managers.Sound.Play(SoundManager.Sound.Effect,
            "Audio/BasicContents/WaterMusic/" + RayHitForXylophone.transform.gameObject.name, 0.3f);
        trans.DORotateQuaternion(_defaultRotationMap[currentID] * Quaternion.Euler(40, 0, 0), 1f);

        var defaultPos = trans.position;
        trans.DOMove(trans.position + Vector3.down * 3.8f, 1f).SetEase(Ease.InOutBack)
            .OnStart(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/WaterMusic/Deeper",
                    0.5f);
            })
            .OnComplete(() =>
            {
               
                trans.DOMove(defaultPos, 0.3f).OnComplete(() =>
                {
                    trans.DORotateQuaternion(_defaultRotationMap[currentID], 1f).OnComplete(() =>
                    {
                        DOVirtual.Float(0, 0, 0.6f, _ => { }).OnComplete(() => { _isTweening[currentID] = false; });
                    });

                });

            });
    }

    protected virtual void BindEvent()
    {
        WaterMusicBaseGameManager.On_GmRay_Synced -= OnClicked;
        WaterMusicBaseGameManager.On_GmRay_Synced += OnClicked;

        UI_Scene_StartBtn.onGameStartBtnShut -= DoIntroMove;
        UI_Scene_StartBtn.onGameStartBtnShut += DoIntroMove;
    }

    private void OnDestroy()
    {  
        UI_Scene_StartBtn.onGameStartBtnShut -= DoIntroMove;
        WaterMusicBaseGameManager.On_GmRay_Synced -= OnClicked;
    }

    public float brightenIntensity;
    private Color brightenedColor;


    private Coroutine _xylophoneCoroutine;


    public float playWaitTime;
    private float _playCurrentTime;
    private WaitForSeconds xylophoneInerval;



}
