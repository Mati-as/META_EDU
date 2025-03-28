using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EvaArmisenBaseGameManager : Base_GameManager
{
   // public string gameVersion; //Fish or Tree 
   // private SpriteRenderer _bgSprite;
   private static EvaArmisen_ToolManager s_toolManager;
    public static bool isInit { get; private set; }

    public Queue<GameObject>[] stampPools { get; set; }
    public Queue<Transform> allStamps; // 꽃도장을 모두 반환하기 위한변수
    private Dictionary<int, Queue<GameObject>> _poolIDMap;  //GameobjID - Pool 쌍 
    private GameObject[] _stamps; 

    private ParticleSystem[] _ps;

    
    private readonly float _poolSize = 10;
    private float _elapsed;
    private readonly float _timeLimit = 987654321f;
    private float _remainTime;
    private bool _isRoundReadyToStart;
    private Animator _pictureAnimator;
    private readonly int IDLE_ANIM = Animator.StringToHash("Idle");
    private TextMeshProUGUI _tmp;

    //Glowing Outline MeshRenderer
    private Color _glowDefaultColor;

    public static event Action OnStampingFinished;
    public static event Action printInitEvent;
    public static event Action onRoundRestart;
    
    private RaySynchronizer _raySync;

    protected override void Init()
    {
        base.Init();

        waitForClickableInGameRay = 0.13f;

        DOTween.Init().SetCapacity(12500, 300);
        
        _raySync = GameObject.FindWithTag("RaySynchronizer").GetComponent<RaySynchronizer>();
        
        
        if (s_toolManager == null)
        {
            s_toolManager = GameObject.Find("EvaArmisen_ToolManager").AddComponent<EvaArmisen_ToolManager>();
        }
        s_toolManager.Init();
        

      


        stampPools = new Queue<GameObject>[s_toolManager.FLOWER_STAMP_COUNT];
        allStamps = new Queue<Transform>();
        _poolIDMap = new Dictionary<int, Queue<GameObject>>();
        SetPool();

        _pictureAnimator = GameObject.Find("EvaArmisenAnimation").GetComponent<Animator>();


        _ps = new ParticleSystem[2];
        _ps[0] = GameObject.Find("CFX_EvaRight").GetComponent<ParticleSystem>();
        _ps[1] = GameObject.Find("CFX_EvaLeft").GetComponent<ParticleSystem>();

    }

    protected override void BindEvent()
    {
        base.BindEvent();

        OnStampingFinished -= OnRoundFinished;
        OnStampingFinished += OnRoundFinished;

        EvaArmisen_ToolManager.OnResetClicked -= Reset;
        EvaArmisen_ToolManager.OnResetClicked += Reset;
        
        // onRoundRestart -= OnRoundRestart;
        // onRoundRestart += OnRoundRestart;

        // EvaArmisen_UIManager.onStartUI -= OnStartUI;
        // EvaArmisen_UIManager.onStartUI += OnStartUI;
    }
    

    private float _elapsedToCount;
    private bool _isCountNarrationPlaying;

    private void Reset()
    {
        
        foreach (var stamp in allStamps)
        {
            stamp.transform.DOScale(Vector3.zero, Random.Range(0.6f, 0.8f)).SetDelay(Random.Range(0.6f, 0.8f));
            _poolIDMap[stamp.gameObject.GetInstanceID()].Enqueue(stamp.gameObject);
        }
        
    }

    private void EraseStamp(GameObject stampObj)
    {
        stampObj.transform.DOScale(Vector3.zero, 0.05f).OnComplete(()=>
        {
            stampObj.SetActive(false);
            var stampID =stampObj.GetInstanceID();
            _poolIDMap[stampID].Enqueue(stampObj);
        });
    }

  
 
    private readonly string ON_READY_MESSAGE = "놀이를 다시 준비하고 있어요";
    private void OnRoundFinished()
    {
        DOVirtual.Float(0, 0, 1f, _ => { }).OnComplete(() =>
        {
            

            DOVirtual.Float(0, 0, 4f, _ => { }).OnComplete(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandPainting/OnRoundFinish", 0.8f);
                
                _pictureAnimator.SetBool(IDLE_ANIM,true);
                _ps[0].Play();
                _ps[1].Play();
                //그림 애니메이션 플레이 Duration입니다. 
                DOVirtual.Float(0, 0, 3.55f, _ => { }).OnComplete(() =>
                {
                    _ps[0].Stop();
                    _ps[1].Stop();
                    _pictureAnimator.SetBool(IDLE_ANIM,false); 
                });
           
                DOVirtual.Float(0, 0, 5, _ => { }).OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/BasicContents/HandFlip2/OnReady", 0.8f);
                    _tmp.text = ON_READY_MESSAGE;
                    DOVirtual.Float(0, 0, 3f, _ => { }).OnComplete(() =>
                    {
                        _tmp.text = string.Empty;
                    });
                    
                    DOVirtual.Float(0, 0, 5, _ => { }).OnComplete(() =>
                    {

                        onRoundRestart?.Invoke();
                    });
                });
               
              
            });
        });
    }

    public override void OnRaySynced()
    {
        
        if (!PreCheckOnRaySync()) return;
        
        // 1.1 지우개 모드인 경우 -----------------------
        if (s_toolManager._isEraserMode)
        {
            foreach (var hit in GameManager_Hits)
            {
                if (hit.transform.gameObject.name.Contains("Flower"))
                {
                    EraseStamp(hit.transform.gameObject);
                }
            }
            return;
        }
        
        foreach (var hit in GameManager_Hits)
        {
            // 1.1 버튼클릭인 경우 -----------------------
            Button button = null;

            if (_raySync == null)
            {
                _raySync = GameObject.FindWithTag("RaySynchronizer").GetComponent<RaySynchronizer>();
            }
            if (_raySync?.raycastResults?.Count > 0)
                for (var i = 0; i < _raySync.raycastResults.Count; i++)
                {
                    _raySync.raycastResults[i].gameObject.TryGetComponent(out button);
                
                    if (button != null && (button.gameObject.name.Contains("Btn_Erasor") ||
                                           button.gameObject.name.Contains("Btn_Restart")))
                    {
                        Logger.Log("지우개나 다시시작은 마우스로 클릭이 불가능해요");
                        
                    }
                    else
                    {
                        button?.onClick?.Invoke();
                    }

                  
                }

            if (s_toolManager.isInitialStampSet)
            {
                SetClickableWithDelay();
                GetFromPool(hit.point,s_toolManager.currentStampIndex);
                var randomChar = (char)Random.Range('A', 'F' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, $"Audio/BasicContents/HandFootFlip/Click_{randomChar}",
                    0.3f);

                return;

            }
            
            


        }
    }

    private void SetPool()
    {
        for (int i = 0; i < s_toolManager.FLOWER_STAMP_COUNT; i++)
        {
            var currentStampToCopy = transform.GetChild(i).gameObject;
            stampPools[i] = new Queue<GameObject>();
  
            // Optionally, if you need more instances than available, clone them
            while (stampPools[i].Count < _poolSize * 15)
            {
                var print = Instantiate(currentStampToCopy, transform);
                print.transform.DORotateQuaternion(print.transform.rotation * Quaternion.Euler(0, Random.Range(-180, 180f), 0f), 0.01f);
                print.gameObject.SetActive(false);
                stampPools[i].Enqueue(print);
               
                
                //클릭조작용이 아닌 오브젝트 풀 반환용 및 초기화용입니다----------
                _poolIDMap.Add(print.GetInstanceID(),stampPools[i]);
                allStamps.Enqueue(print.transform);
            }
            currentStampToCopy.SetActive(false);
        }
    }

    private readonly float RETRUN_WAIT_TIME = 100;

    private void GetFromPool(Vector3 spawnPosition, int stampIndex)
    {
        
        if (stampPools[stampIndex].Count <= 0) GrowPool(stampIndex);

        var print = stampPools[stampIndex].Dequeue();
        print.transform.position = spawnPosition;
        print.gameObject.SetActive(true);
        
    }

    protected void GrowPool( int stampIndex)
    {
        var currentStampToCopy = transform.GetChild(stampIndex).gameObject;
        stampPools[stampIndex] = new Queue<GameObject>();
  
        // Optionally, if you need more instances than available, clone them
        while (stampPools[stampIndex].Count < _poolSize)
        {
            var print = Instantiate(currentStampToCopy, transform);
            print.transform.DORotateQuaternion(print.transform.rotation * Quaternion.Euler(0, Random.Range(-180, 180f), 0f), 0.01f);
            print.gameObject.SetActive(false);
            stampPools[stampIndex].Enqueue(print);
        }

    }
    
    protected override void OnDestroy()
    {
         base.OnDestroy();
         OnStampingFinished -= OnRoundFinished;
         EvaArmisen_ToolManager.OnResetClicked -= Reset;
    }

}