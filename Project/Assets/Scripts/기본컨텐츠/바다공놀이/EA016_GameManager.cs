using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class EA016_GameManager : Ex_BaseGameManager
{
    private ParticleSystem _clickPs;
    public Queue<ParticleSystem> particlePool;
       
    private RaycastHit[] _hits;
    private RaycastHit _hitForPs; //클릭시 이펙트 효과전용
    
    public float forceAmount;
    public float upOffset;
    
   
    public enum MainSequence
    {
        Default,
        FreePlay,
        ColorMode,
        CountMode,
        OnFinishFreePlay
    }

   
    public int CurrentMainSeq
    {
        get
        {
            return currentMainMainSequence;
        }
        set
        {
            Logger.ContentTestLog($"current Mode : {(MainSequence)CurrentMainSeq}");
            currentMainMainSequence = value;

            switch (currentMainMainSequence)
            {
                case (int)MainSequence.ColorMode:
                    RoundSetInColorBallGame(color:BallInfo.BallColor.Pink);
                    Messenger.Default.Publish(new UI_Payload("돌고래가 원하는 색깔의 공을 넣어주세요!", true, delayAndAutoShutTime: 4f));
                    Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "PutWhatDolphinWants");
                    break;
                
                case (int)MainSequence.CountMode:
                    
                    isBallCountable = false;
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        Messenger.Default.Publish(new UI_Payload("이번엔 숫자만큼 놀아보자!", true, delayAndAutoShutTime: 2f,
                            Checksum: MainSequence.CountMode.ToString()));
                        Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "LetsPlayWithCount");
                    });
                   
                    DOVirtual.DelayedCall(6f, () =>
                    {
                        Messenger.Default.Publish(new UI_Payload("목표 숫자만큼 공을 넣어주세요!", true, delayAndAutoShutTime: 3f));
                        Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "CountAndPut");
                        DOVirtual.DelayedCall(3f, () =>
                        {
                            OnRoundSetInBallCountGame();
                        });
                    });
                    
                    break;
                   
                case (int)MainSequence.OnFinishFreePlay:
                    Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "FreePlay"); 
                    Messenger.Default.Publish(new UI_Payload("잘했어!\n이제부턴 돌고래랑 자유롭게 놀 수 있어!", true, delayAndAutoShutTime: 8f));
                    break;
                    
                    
            }
          
        }
    }

    public Vector3 particleUpOffset;

    protected override void Init()
    {
        SetPool(ref particlePool);
        SensorSensitivity = 0.18f;
        //////BGM_VOLUME = 0.2f;
        psResourcePath = "SortedByScene/BasicContents/EA016/Fx_Click";
        base.Init();
        ManageProjectSettings(150, 0.15f);

        EA016_BallController.OnBallIsInTheHole -= OnBallInTheHole;
        EA016_BallController.OnBallIsInTheHole += OnBallInTheHole;
        currentMainMainSequence = (int)MainSequence.Default;
        _ea016UIManager = UIManagerObj.GetComponent<EA016_UIManager>();
        Debug.Assert(_ea016UIManager != null, "UIManager not found");
    }

    private EA016_UIManager _ea016UIManager;
    protected override void OnDestroy()
    {
        base.OnDestroy();
        EA016_BallController.OnBallIsInTheHole +=OnBallInTheHole;
    }

    protected bool isBallCountable = false;
    private void OnBallInTheHole(int ballColor)
    {
   
        
        if(CurrentMainSeq == (int)MainSequence.FreePlay) return;
        Logger.ContentTestLog($"현재 공 색깔 : {(BallInfo.BallColor)ballColor}");

        if (!isBallCountable)
        {
            Logger.ContentTestLog($"아직 공 카운팅 준비가 안됬거나 해당 모드 아님 {(MainSequence)CurrentMainSeq}");
            return;
        }

        if (CurrentMainSeq == (int)MainSequence.ColorMode)
        {
            if (currentBallColorToPut == (BallInfo.BallColor)ballColor)
            {
                CurrentBallCountAlreadyPut++;
                var randomChar = (char)Random.Range('A', 'C' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/EA016/Hole" + randomChar,0.5f);
            }
        } 
        else if (CurrentMainSeq == (int)MainSequence.CountMode)
        {
            CurrentBallCountAlreadyPut++;
            Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + _currentBallCountAlreadyPut.ToString());

        }
      
    }

    private readonly string NARRATION_PATH = "Audio/EA016/Narration/";
    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        
        CurrentMainSeq = (int)MainSequence.FreePlay;
        DOVirtual.DelayedCall(1.5f, () =>
        {
            Messenger.Default.Publish(new UI_Payload("친구들! 공을 가지고 돌고래와 놀아볼까요?", true, delayAndAutoShutTime: 2.5f));
            Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "LetsPlayWithBalls");
        });
       
        DOVirtual.DelayedCall(5f, () =>
        {
            Messenger.Default.Publish(new UI_Payload("공을 터치해서 구멍에 넣어주세요!", true, delayAndAutoShutTime: 6f));
            Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "TouchAndPutBalls");
        });
    }

    private Dictionary<int, EA016_BallController> _ballMap =new();

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (!isStartButtonClicked) return;
        
        
        
        foreach (var hit in GameManager_Hits)
        {
            var rb = hit.collider.GetComponent<Rigidbody>(); // 부딪힌 물체에 Rigidbody 컴포넌트가 있는지 확인합니다.
            if(hit.transform.gameObject.name.Contains("WaterCollider"))
            {
                PlayParticle(hit.point + particleUpOffset);//offset
            }

            if (CurrentMainSeq == (int)MainSequence.ColorMode)
            {
              
                
                
                if (hit.transform.gameObject.name.Contains("Ball"))
                {
                    int ballID = hit.transform.GetInstanceID();
                    _ballMap.TryAdd(ballID, hit.transform.GetComponent<EA016_BallController>());

                    if (_ballMap[ballID].thisBallColor != currentBallColorToPut)
                    {
                        Logger.ContentTestLog($"색깔이 다름 : {hit.transform.gameObject.name}");
                        _ballMap[ballID].Vanish();
                        PlayParticleEffect(hit.transform.position + particleUpOffset);
                        return;
                    }

                    if (rb != null)
                    {
                        var forceDirection = rb.transform.position - hit.point + Vector3.up * upOffset;

                        char randomChar = (char)Random.Range('A', 'D' + 1);
                        Managers.Sound.Play(SoundManager.Sound.Effect,
                            "Audio/EA016/Click" + randomChar, 0.35f);
                        rb.AddForce(forceDirection.normalized * forceAmount, ForceMode.Impulse);
 
                        DEV_OnValidClick();
                    }
                }
            }
            else
            {
                if (rb != null)
                {
                    var forceDirection = rb.transform.position - hit.point + Vector3.up * upOffset;

                    char randomChar = (char)Random.Range('A', 'D' + 1);
                    Managers.Sound.Play(SoundManager.Sound.Effect,
                        "Audio/EA016/Click" + randomChar, 0.35f);
                    rb.AddForce(forceDirection.normalized * forceAmount, ForceMode.Impulse);

                    DEV_OnValidClick();
                }
            }
            
          
            
        }
        
   
        

     
    }


    
        

    private float _elapsed;
#if UNITY_EDITOR
    [SerializeField] [Range(0, 60)] private float _timeForNextRound;
#else
    private float _timeForNextRound = 60;
#endif

    private bool _isFreePlayFinished = false;

    private void Update()
    {
        if (!isStartButtonClicked) return; 
        if (!isStartButtonClicked && CurrentMainSeq != (int)MainSequence.FreePlay) return;

        if (_isFreePlayFinished) return; 
        
        _elapsed += Time.deltaTime;
        if (_elapsed > _timeForNextRound)
        {
            _isFreePlayFinished = true;
            _elapsed = 0;
            CurrentMainSeq = (int)MainSequence.ColorMode;
        }

    }

    protected virtual void SetPool(ref Queue<ParticleSystem> psQueue)
    {
        psQueue = new Queue<ParticleSystem>();
        var index = 0;
        var ps = Resources.Load<GameObject>("SortedByScene/BasicContents/WaterPlayGround/Click").GetComponent<ParticleSystem>();


        psQueue.Enqueue(ps);
        ps.gameObject.SetActive(false);
            

        // Optionally, if you need more instances than available, clone them
        while (psQueue.Count < 20)
        {
            var newPs = Instantiate(ps, transform);
            newPs.gameObject.SetActive(false);
            psQueue.Enqueue(newPs);
        }
    }

    private void PlayParticle(Vector3 point)
    {
        if (particlePool.Count <= 0) return;
        
        var ps = particlePool.Dequeue();
        ps.gameObject.SetActive(true);
        ps.Stop();
        ps.transform.position = point;
        ps.Play();
    
        DOVirtual.Float(0, 0, ps.main.duration, _ =>{})
            .OnComplete(() =>
            {
                ps.gameObject.SetActive(false);
                particlePool.Enqueue(ps);
            });
        
       
    }
    


    #region 색깔공넣기 놀이 파트
    
    
#if UNITY_EDITOR
    [Range(0,20)]
    public int BALLCOUNT_MIN_COLORMODE;
    [Range(0,20)]
    public int BALLCOUNT_MAX_COLORMODE;
#else

    public int BALLCOUNT_MIN_COLORMODE = 7;
    public int BALLCOUNT_MAX_COLORMODE = 15;
#endif


    private void RoundSetInColorBallGame(BallInfo.BallColor color)
    {
        currentBallColorToPut = color;
        BallCountGoal = Random.Range(BALLCOUNT_MIN_COLORMODE, BALLCOUNT_MAX_COLORMODE);
        _currentBallCountAlreadyPut = 0;
      
        
        DOVirtual.DelayedCall(4f, () =>
        {
            //Logger.ContentTestLog($"현재 목표 공 색상: {currentBallColorToPut}");
            _ea016UIManager.ShowBallImageAndCountWithRefresh();
            isBallCountable = true;
        });
     
        
     
    }
    public BallInfo.BallColor currentBallColorToPut
    {
        get;
        private set;
    }
    
    public int BallCountGoal
    {
        get;
        private set;
    }

    private int _currentBallCountAlreadyPut = -123;
    private int currentCountModeCount = 1;
    private readonly int COUNT_MODE_MAX = 3;

    public int CurrentBallCountAlreadyPut
    {
        get
        {
            return _currentBallCountAlreadyPut;
        }
        private set
        {
            _currentBallCountAlreadyPut = value;
            _ea016UIManager.RefreshText();
            if (CurrentMainSeq == (int)MainSequence.ColorMode)
            {
                if (_currentBallCountAlreadyPut >= BallCountGoal)
                {
                    isBallCountable = false;
                    Messenger.Default.Publish(new UI_Payload("성공! 다 넣었다!", true, delayAndAutoShutTime: 5f));
                    DOVirtual.DelayedCall(0.5f,()=>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "GoodJob");
                    });
                    

                    _ea016UIManager.ShutBallCountUI();
                    DOVirtual.DelayedCall(3f, () =>
                    {
                        if (currentBallColorToPut == BallInfo.BallColor.Pink)
                            RoundSetInColorBallGame(BallInfo.BallColor.Yellow);
                        else if (currentBallColorToPut == BallInfo.BallColor.Yellow)
                            RoundSetInColorBallGame(BallInfo.BallColor.Blue);
                        else if (currentBallColorToPut == BallInfo.BallColor.Blue)
                        {
                            isBallCountable = false;
                            CurrentMainSeq = (int)MainSequence.CountMode;
                        }
                    });
                }
            }
            else if (CurrentMainSeq == (int)MainSequence.CountMode)
            {
                
                if (_currentBallCountAlreadyPut >= BallCountGoal)
                {
                    isBallCountable = false;
                    Messenger.Default.Publish(new UI_Payload("성공! 다 넣었다!", true, delayAndAutoShutTime: 4f));
                    DOVirtual.DelayedCall(0.5f,()=>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "GoodJob");
                    });
                    
                    DOVirtual.DelayedCall(3f, () =>
                    {
                        if (currentCountModeCount < COUNT_MODE_MAX)
                        {
                          
                            currentCountModeCount++;
                            OnRoundSetInBallCountGame();
                        }
                        else
                        {
                       
                            _ea016UIManager.ShutBallCountUI();
                            CurrentMainSeq = (int)MainSequence.OnFinishFreePlay;
                        }
                    });
                }
       
            }
        }
    }
    
    public int ballCountLeftToPut
    {
        get
        {
            return BallCountGoal - CurrentBallCountAlreadyPut;
        }
    }

    #endregion


    #region 갯수 놀이 파트

#if UNITY_EDITOR
    [Range(0,20)]
    public int BALLCOUNT_MIN_COUNTMODE;
    [Range(0,20)]
    public int BALLCOUNT_MAX_COUNTMODE;
#else

    public int BALLCOUNT_MIN_COUNTMODE = 7;
    public int BALLCOUNT_MAX_COUNTMODE = 10;
#endif
    
 

    private void OnRoundSetInBallCountGame()
    {
        
        BallCountGoal = Random.Range(BALLCOUNT_MIN_COUNTMODE, BALLCOUNT_MAX_COUNTMODE);
        _currentBallCountAlreadyPut = 0;
        DOVirtual.DelayedCall(4f, () =>
        {
            _ea016UIManager.ShowBallImageAndCountWithRefresh();
            isBallCountable = true;
        });
    }

    #endregion
}
