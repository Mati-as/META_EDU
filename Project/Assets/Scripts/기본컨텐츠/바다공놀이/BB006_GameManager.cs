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

public class BB006_GameManager : Ex_BaseGameManager
{
    private ParticleSystem _clickPs;
    public Queue<ParticleSystem> particlePool;
       
    private RaycastHit[] _hits;
    private RaycastHit _hitForPs; //클릭시 이펙트 효과전용
    
    public float forceAmount;
    public float upOffset;
    
   
    public enum MainSequence
    {
        FreePlay,
        ColorMode,
        CountMode,
        OnFinish
    }

    
    public int currentMainSeq
    {
        get => _currentMainSequence;
        set
        {
            Logger.ContentTestLog($"current Mode : {(MainSequence)currentMainSeq}");
            _currentMainSequence = value;

            switch (_currentMainSequence)
            {
                case (int)MainSequence.ColorMode:
                    OnRoundSetInColorBallGame(color:BallInfo.BallColor.Pink);
                    Messenger.Default.Publish(new UI_Payload("돌고래가 원하는 색깔의 공을 넣어주세요!", true, delayAndAutoShutTime: 5f));
                    break;
                
                case (int)MainSequence.CountMode:
                  
                    OnRoundSetInBallCountGame();
                    Messenger.Default.Publish(new UI_Payload("이번엔 숫자만큼 놀아보자!", true, delayAndAutoShutTime: 3f,Checksum:MainSequence.CountMode.ToString()));
                    Messenger.Default.Publish(new UI_Payload("목표 숫자만큼 공을 넣어주세요!!", true, delayAndAutoShutTime: 3f));
                    break;
                
                case (int)MainSequence.OnFinish:
                    Messenger.Default.Publish(new UI_Payload("잘했어! 이제부턴 돌고래랑 자유롭게 놀 수 있어!", true, delayAndAutoShutTime: 3f));
                    break;
                    
                    
            }
          
        }
    }
  
    public Vector3 particleUpOffset;

    protected override void Init()
    {
        SetPool(ref particlePool);
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        WaterPlayground_BallController.OnBallIsInTheHole -= OnBallInTheHole;
        WaterPlayground_BallController.OnBallIsInTheHole += OnBallInTheHole;

        BB006_UIManager = UIManager.GetComponent<BB006_UIManager>();
        Debug.Assert(BB006_UIManager != null, "UIManager not found");
    }

    private BB006_UIManager BB006_UIManager;
    protected override void OnDestroy()
    {
        base.OnDestroy();
        WaterPlayground_BallController.OnBallIsInTheHole +=OnBallInTheHole;
    }

    protected bool isBallCountable = false;
    private void OnBallInTheHole(int ballColor)
    {
   
        if(currentMainSeq == (int)MainSequence.FreePlay) return;
        Logger.ContentTestLog($"현재 공 색깔 : {(BallInfo.BallColor)ballColor}");

        if (isBallCountable) return; 
        
        if (currentBallColorToPut == (BallInfo.BallColor)ballColor)
        {
            CurrentBallCountAlreadyPut++;
        }
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        
        currentMainSeq = (int)MainSequence.FreePlay;
        Messenger.Default.Publish(new UI_Payload("친구들 공을 가지고 돌고래와 놀아볼까요?", true, delayAndAutoShutTime: 2.5f));

        DOVirtual.DelayedCall(5f, () =>
        {
            Messenger.Default.Publish(new UI_Payload("공을 터치해서 구멍에 넣어주세요!", true, delayAndAutoShutTime: 6f));
        });
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (!isStartButtonClicked) return;
        
        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>(); // 부딪힌 물체에 Rigidbody 컴포넌트가 있는지 확인합니다.
            
            if (rb != null)
            {
                Vector3 forceDirection =  rb.transform.position - hit.point + Vector3.up*upOffset;
                
                var randomChar = (char)Random.Range('A', 'D' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, 
                    "Audio/BB006/Click" + randomChar,0.35f);
                rb.AddForce(forceDirection.normalized * forceAmount, ForceMode.Impulse);

                DEV_OnValidClick();
            }
            
        }
        
   
        
        foreach (var hit in _hits)
        {
            if(hit.transform.gameObject.name.Contains("WaterCollider"))
            {
                PlayParticle(hit.point + particleUpOffset);//offset
                return;
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
        if (!isStartButtonClicked && _currentMainSequence != (int)MainSequence.FreePlay) return;

        if (_isFreePlayFinished) return; 
        
        _elapsed += Time.deltaTime;
        if (_elapsed > _timeForNextRound)
        {
            _isFreePlayFinished = true;
            _elapsed = 0;
            currentMainSeq = (int)MainSequence.ColorMode;
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

    private void OnRoundSetInColorBallGame(BallInfo.BallColor color)
    {
        BallCountGoal = Random.Range(7, 15);
        _currentBallCountAlreadyPut = 0;
        currentBallColorToPut = color;
        BB006_UIManager.ShowBallImageAndCountWithRefresh();
        
        isBallCountable = true;
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
    public int CurrentBallCountAlreadyPut
    {
        get
        {
            return _currentBallCountAlreadyPut;
        }
        private set
        {
            _currentBallCountAlreadyPut = value;
            BB006_UIManager.RefreshText();
            if (currentMainSeq == (int)MainSequence.ColorMode)
            {
              
                if (_currentBallCountAlreadyPut >= BallCountGoal)
                {
                    currentMainSeq = (int)MainSequence.OnFinish;
                    Messenger.Default.Publish(new UI_Payload("성공! 다 넣었다!",true, delayAndAutoShutTime: 5f));
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
    
    private void OnRoundSetInBallCountGame()
    {
        BallCountGoal = Random.Range(5, 10);
    }

    #endregion
}
