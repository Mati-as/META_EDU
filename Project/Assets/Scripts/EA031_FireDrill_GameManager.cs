using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EA031_FireDrill_GameManager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Default_LateInit,
        OnIntro,
        OnFireAndAlarm,
        clostMouthAndNoseInstruction,
        CloseMouthAndNose,
        TakeExit,
        OnEscape,
        OnFinish
    }

    private enum Objs
    {
        
        IntroSmoke,
        
        ToxicGas,
        SirenAlert,
        IntroAvatarController,
        TowelToCover,
        OnExitAvataController,
        
        ToxicGas_OnExit,
        OnEscapePaths,
        OnEscapeAvatarController,
        OnEscapeAvatar, // 애니메이션과 별개로 Transform 컨트롤
        
        OnEndAvatarController,
        
        EscapeDefaultPos,
        PathOutPos,
        
        InducingArrowPath_A,
        InducingArrowPath_B,
        InducingArrowPath_C,
        
        EscapeStepsA,
        EscapeStepsB,
        EscapeStepsC,
    }

    private AvatarController _introAvatarController;
    private AvatarController _onExitAvatarController;
    private AvatarController  _onEscapeAvatarController;
    private AvatarController _onEndAvatarController;

    private ParticleSystem _sirenPs;
    private ParticleSystem _smokePs;

    private Vector3 _defaultStepSize;
    public int CurrentMainMainSeq
    {
        get
        {
            return currentMainMainSequence;
        }
        set
        {
            currentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {((MainSeq)CurrentMainMainSeq).ToString()}");
            // commin Init Part.


            ChangeThemeSeqAnim(value);
            switch (value)
            {
                case (int)MainSeq.Default_LateInit:

                    break;

                case (int)MainSeq.OnIntro:
                    DOVirtual.DelayedCall(3f, () =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.OnFireAndAlarm;
                    });
                    SetSmokeStatus((int)Objs.IntroSmoke);
                    _smokePs.Clear();
                    _smokePs.Stop();
                    _smokePs.Play();

                    break;

                case (int)MainSeq.OnFireAndAlarm:
                    GetObject((int)Objs.ToxicGas).SetActive(true);
                    GetObject((int)Objs.SirenAlert).SetActive(true);
                    _uiManager.PopInstructionUIFromScaleZero("검정 연기가 보여~무슨 냄새지?");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA031/Smoke");
                
                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA031/FireAlarm");
                    });
                    DOVirtual.DelayedCall(2f, () =>
                    {  
                        Managers.Sound.Play(SoundManager.Sound.Bgm, "EA031/EA031_BGM_A");
                    });
                  
                    
                  
                    DOVirtual.DelayedCall(3.5f, () =>
                    {
                        float playTime = 0;

                        _uiManager.PopInstructionUIFromScaleZero("친구들~ 불이 났어요! 모두 대피 해야해요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA031/2_Fire");
                        DOVirtual.DelayedCall(4f, () =>
                        {
                            GetObject((int)Objs.ToxicGas_OnExit).SetActive(true);
                            for (int i = 0; i < GetObject((int)Objs.ToxicGas_OnExit).transform.childCount; i++)
                                GetObject((int)Objs.ToxicGas_OnExit).transform.GetChild(i)
                                    .GetComponent<ParticleSystem>().Play();

                            CurrentMainMainSeq = (int)MainSeq.clostMouthAndNoseInstruction;
                        });
                        DOVirtual.Float(0f, 0f, 1000f, _ =>
                        {
                        }).OnUpdate(() =>
                        {
                            playTime += Time.deltaTime;
                            if (playTime > 0.6f)
                            {
                                _sirenPs.Play();
                                playTime = 0f;
                            }
                        });
                    });


                    for (int i = 0; i < 4; i++)
                    {
                        int i1 = i;
                        DOVirtual.DelayedCall(Random.Range(0.2f * i1, 0.4f * i1), () =>
                        {
                            _introAvatarController.PlayAnimation(i1, AvatarController.AnimClip.LookOver);
                        });
                    }


                    break;


                case (int)MainSeq.clostMouthAndNoseInstruction:
                
                    _uiManager.PopInstructionUIFromScaleZero("불이 나면 두손으로 코와 입을 막아요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA031/3_Cover");
                    _introAvatarController.PlayAnimation(3, AvatarController.AnimClip.HideFace);
                    DOVirtual.DelayedCall(3.5f, () =>
                    {
                        _introAvatarController.PauseAnimator(3);
                        GetObject((int)Objs.TowelToCover).transform.localScale = Vector3.zero;
                        GetObject((int)Objs.TowelToCover).SetActive(true);
                        GetObject((int)Objs.TowelToCover).transform.DOScale(_towelDefaultScale, 0.5f)
                            .SetEase(Ease.InOutBack);

                        DOVirtual.DelayedCall(3.5f, () =>
                        {
                            CurrentMainMainSeq = (int)MainSeq.CloseMouthAndNose;
                        });
                    });
                    break;

                case (int)MainSeq.CloseMouthAndNose:
                    SetSmokeStatus(-1);
                    _uiManager.PopInstructionUIFromScaleZero("다른 친구들을 터치하여 도와주세요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA031/4_Help");
                    GetObject((int)Objs.OnExitAvataController).SetActive(true);
                    break;


                case (int)MainSeq.TakeExit:
                    _uiManager.PopInstructionUIFromScaleZero("친구를 따라 비상구로 기어서 이동해주세요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA031/5_TakeExit");
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        InitPaths(0);
                        GetObject((int)Objs.OnEscapeAvatar).SetActive(true);
                        GetObject((int)Objs.OnEscapeAvatarController).SetActive(true);
                        PlayEscapePathAnim(0);
                    });
             
               

                    break;

                case (int)MainSeq.OnEscape:
                  
                    break;

                case (int)MainSeq.OnFinish:
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA031/6_Finish");
                    _uiManager.PopInstructionUIFromScaleZero("두손으로 코와 입을 막고\n비상구로 나가는 것을 기억해요!");
                    Managers.Sound.Play(SoundManager.Sound.Bgm, "Bgm/EA031");
                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Siren_FireTruck");
                    RestartScene(null,10f);
                    break;
            }
        }
    }

    private void InitPaths(int index)
    {
        GetObject((int)Objs.OnEscapePaths).transform.GetChild(0).gameObject.SetActive(false);
        GetObject((int)Objs.OnEscapePaths).transform.GetChild(1).gameObject.SetActive(false);
        GetObject((int)Objs.OnEscapePaths).transform.GetChild(2).gameObject.SetActive(false);
        GetObject((int)Objs.InducingArrowPath_A).SetActive(true);
        GetObject((int)Objs.InducingArrowPath_B).SetActive(true);
        GetObject((int)Objs.InducingArrowPath_C).SetActive(true);
        
        GetObject((int)Objs.OnEscapePaths).transform.GetChild(index).gameObject.SetActive(true);
        
        
    }
    private void PlayEscapePathAnim(int pathIndex,float delay =3f)
    {
        _onEscapeAvatarController.SetWalking(0, true);
        _onEscapeAvatarController.PlayAnimation(0, AvatarController.AnimClip.HideFace);
        GetObject((int)Objs.OnEscapeAvatar).transform.localRotation = _defaultLocalRotationQuatMap[(int)Objs.OnEscapeAvatar];
        
        
        _pathAnimSAeq?.Kill();
        _pathAnimSAeq = DOTween.Sequence();
   

        int PathDuration = 5;
        _pathAnimSAeq.AppendInterval(delay);
        _pathAnimSAeq.AppendCallback( () =>
        {
            GetObject((int)Objs.OnEscapeAvatar).transform.position = GetObject((int)Objs.EscapeDefaultPos).transform.position;
            
            GetObject((int)Objs.OnEscapeAvatar).transform
                .DOPath(_escapePathMap[pathIndex], PathDuration, PathType.CatmullRom)
                .SetEase(Ease.Linear);
        });
        
        
        _pathAnimSAeq.AppendInterval(PathDuration);
        _pathAnimSAeq.AppendCallback(() =>
        {
            var thisRotation = GetObject((int)Objs.OnEscapeAvatar).transform.localRotation.eulerAngles;
            GetObject((int)Objs.OnEscapeAvatar).transform
                .DOLocalRotate(new Vector3(thisRotation.x, thisRotation.y + 180, thisRotation.z), 0.5f);
            _onEscapeAvatarController.PlayAnimation(0, AvatarController.AnimClip.Wave);
            _onEscapeAvatarController.SetWalking(0, false);
            
            _uiManager.PopInstructionUIFromScaleZero("준비 다 됐으면 차분히 날 따라서 나가자!");

            DOVirtual.DelayedCall(4.5f, () =>
            {
                _uiManager.PopInstructionUIFromScaleZero("경로를 순서대로 밟아 비상구로 탈출해요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA031/FollowPath");
            });
            Managers.Sound.Play(SoundManager.Sound.Narration, "EA031/Lets");

            BlinkStep(_currentPathIndex,0);
            
        });

        GameObject currentInducingArrow = null;
        switch (pathIndex)
        {
            case 0 :
                currentInducingArrow =GetObject((int)Objs.InducingArrowPath_A);
                break;
            case 1 :
                currentInducingArrow =GetObject((int)Objs.InducingArrowPath_B);
                break;
            case 2 :
                currentInducingArrow =GetObject((int)Objs.InducingArrowPath_C);
                break;
        }
        
        currentInducingArrow.SetActive(true);
        
        DOVirtual.DelayedCall(15, () =>
        {
            _isClickableForRound = true;
            
             currentInducingArrow.transform.position =GetObject((int)Objs.EscapeDefaultPos).transform.position;
            
            _arrowAnimSeq?.Kill();
            _arrowAnimSeq = DOTween.Sequence();
            
            _arrowAnimSeq.Append(currentInducingArrow.transform
                .DOPath(_escapePathMap[pathIndex], 3, PathType.CatmullRom)
                .SetEase(Ease.Linear)
                .SetLookAt(1));
            _arrowAnimSeq.AppendInterval(3f);
            _arrowAnimSeq.SetLoops(30, LoopType.Restart);
            
            
        });
;
    }

    private Dictionary<int, Vector3[]> _escapePathMap = new();
    
    
    private Dictionary<int, Transform[]> _stepsMap = new ();
    private Dictionary<int, int> _stepOrderMap = new();
    private int _currentStepOrderToClick;
    private const int STEP_MAX_COUNT = 5;
    

    protected override sealed void Init()
    {
        psResourcePath = "Runtime/EA031/Fx_Click";

        BindObject(typeof(Objs));
        base.Init();
        _uiManager = UIManagerObj.GetComponent<Base_UIManager>();
        _smokePs = GetObject((int)Objs.ToxicGas).GetComponent<ParticleSystem>();
        _sirenPs = GetObject((int)Objs.SirenAlert).GetComponent<ParticleSystem>();


        _towelDefaultScale = GetObject((int)Objs.TowelToCover).transform.localScale;
        _onExitAvatarController = GetObject((int)Objs.OnExitAvataController).GetComponent<AvatarController>();
        _onEscapeAvatarController = GetObject((int)Objs.OnEscapeAvatarController).GetComponent<AvatarController>();
        _introAvatarController = GetObject((int)Objs.IntroAvatarController).GetComponent<AvatarController>();
        _onEndAvatarController = GetObject((int)Objs.OnEndAvatarController).GetComponent<AvatarController>();
        
        
        GetObject((int)Objs.TowelToCover).SetActive(false);
        GetObject((int)Objs.OnExitAvataController).SetActive(false);
        GetObject((int)Objs.OnEscapeAvatarController).SetActive(false);
        

        int PATH_TOTAL_COUNT =3;
        int PATH_VERTEX_COUNT =3;
        
        for (int i = 0; i < PATH_TOTAL_COUNT; i++)
        {
            _escapePathMap.Add(i, new Vector3[PATH_VERTEX_COUNT]);
            
            for (int j = 0; j < PATH_VERTEX_COUNT; j++)
            {
                _escapePathMap[i][j] = GetObject((int)Objs.OnEscapePaths).transform.GetChild(i)
                    .GetChild(j).position;
            }
            
            GetObject((int)Objs.OnEscapePaths).transform.GetChild(i).gameObject.SetActive(false);
          
        }



        for (int group = 0; group < 3; group++)
        {
            _stepsMap.Add(group, new Transform[STEP_MAX_COUNT]);
            
            for (int step = 0; step < STEP_MAX_COUNT; step++)
            {
                _stepsMap[group][step] = GetObject(((int)Objs.EscapeStepsA)+group).transform.GetChild(step);
                _stepOrderMap.Add(_stepsMap[group][step].GetInstanceID(), step);
                _defaultStepSize = _stepsMap[group][step].localScale;
            }

        }
  
        
        SetSmokeStatus(-1);
        Logger.Log("Init--------------------------------");
        
        
    }

    private void SetSmokeStatus(int smokeObj = -1)
    {
        if (smokeObj == -1)
        {
            GetObject((int)Objs.IntroSmoke).SetActive(false);
            return;
        }
        else
        {
            GetObject(smokeObj).SetActive(true);
        }


    }
    private Sequence _pathAnimSAeq;
    private Base_UIManager _uiManager;
    private Vector3 _towelDefaultScale;
    private Sequence _arrowAnimSeq;
    
#if UNITY_EDITOR
    [SerializeField]
    private MainSeq initialSeq;
 
#else
     private MainSeq initialSeq = MainSeq.OnIntro;
#endif
    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();


        CurrentMainMainSeq = (int)initialSeq;

        for (int i = 0; i < 6; i++)
        {
            _onEndAvatarController.PlayAnimation(i, AvatarController.AnimClip.HideFace);
            
            int indexCache = i;
            DOVirtual.DelayedCall(3.5f + Random.Range(0.1f,0.4f), () =>
            {
                _onEndAvatarController.PauseAnimator(indexCache);
                _onEndAvatarController.SetLegAnim(i, (int)AvatarController.LegAnimClip.Idle);
            });

        }
    

    }

    public override void OnRaySynced()
    {
        base.OnRaySynced();

        if (CurrentMainMainSeq == (int)MainSeq.CloseMouthAndNose)
        {
            Logger.ContentTestLog("OnRaySynced - CloseMouthAndNose");
            OnRaySyncOnAvatarClick();
        }
        if(CurrentMainMainSeq == (int)MainSeq.TakeExit || CurrentMainMainSeq == (int)MainSeq.OnEscape)
        {
            OnRaySyncOnEscape();
        }
    }

    private const int AVATART_COUNT_TO_HELP = 7;
    private int _currentAvatarHelpCount;

    private void OnRaySyncOnAvatarClick()
    {
        foreach (var hit in GameManager_Hits)
        {
            int id = hit.transform.GetInstanceID();
            _isClickableMap.TryAdd(id, true);
            
            if (_onExitAvatarController.IsTransIDValid(id) && _isClickableMap.ContainsKey(id) && _isClickableMap[id])
            {
                _isClickableMap[id] = false; 
                _currentAvatarHelpCount++;
                _onExitAvatarController.PlayAnimationByTransID(id, AvatarController.AnimClip.HideFace);
           
                PlayParticleEffect(hit.point);
                
                int idCache = id;
                DOVirtual.DelayedCall(2.5f, () =>
                {
                    _onExitAvatarController.PauseAnimatorByID(idCache);
                });

                if (_currentAvatarHelpCount >= AVATART_COUNT_TO_HELP)
                {
                    _currentAvatarHelpCount = 0;
                    _uiManager.PopInstructionUIFromScaleZero("모든 친구를 도와줬어!");

                    DOVirtual.DelayedCall(3f, () =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.TakeExit;
                    });
                }
            }
            else
                Logger.ContentTestLog("there's no animator for this avatar: " + hit.transform.name);
        }
    }

    private void OnEscapePathSuccess()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA031/OnSuccess");
        
        InitForNewEscapePath();
        var thisRotation = GetObject((int)Objs.OnEscapeAvatar).transform.localRotation.eulerAngles;
        GetObject((int)Objs.OnEscapeAvatar).transform
            .DOLocalRotate(new Vector3(thisRotation.x, thisRotation.y + 180, thisRotation.z), 0.5f);
        
        
       
        _onEscapeAvatarController.PlayAnimation(0, AvatarController.AnimClip.HideFace);
        DOVirtual.DelayedCall(2f, () =>
        {
            _onEscapeAvatarController.SetWalking(0, true);
            GetObject((int)Objs.OnEscapeAvatar).transform.DOMove(GetObject((int)Objs.PathOutPos).transform.position,1.0f);
        });
      
        

      
        
        
        
        _currentPathIndex++;
        
        if (_currentPathIndex >= 3)
        {
            CurrentMainMainSeq = (int)MainSeq.OnFinish;
        }
        else
        {
            DOVirtual.DelayedCall(6f, () =>
            {
                InitPaths(_currentPathIndex);
               
                PlayEscapePathAnim(_currentPathIndex);
            });
        }
       
    }

    private int _currentPathIndex = 0;
    private bool _isClickableForRound;
    private void InitForNewEscapePath()
    {
      
        _uiManager.PopInstructionUIFromScaleZero("잘했어! 다음친구도 어서 대피하자!");
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA031/GoodJob");
       // _arrowAnimSeq?.Kill();
        _pathAnimSAeq?.Kill();
        _currentStepOrderToClick = 0;
    }

    private int _currentPathRound = 0;

    private void OnRaySyncOnEscape()
    {
        if (!_isClickableForRound) return;
        
        foreach (var hit in GameManager_Hits)
        {
            int id = hit.transform.GetInstanceID();
            var hitTransform = hit.transform;
            _isClickableMap.TryAdd(id, true);

            if (_stepOrderMap.ContainsKey(id))
                
                if (_stepOrderMap[id] == _currentStepOrderToClick && _isClickableMap[id])
                {
                    char randomChar = (char)Random.Range('A', 'B' + 1);
                   
                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA031/OnStep"+randomChar);
                    _isClickableMap[id] = false;
                    hitTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBounce);
                    _currentStepOrderToClick++;
                    //Logger.ContentTestLog($"남은 클릭 개수{STEP_MAX_COUNT-_currentStepOrderToClick}");

                    if (_currentStepOrderToClick < STEP_MAX_COUNT-1)
                    {
                        BlinkStep(_currentPathRound, _currentStepOrderToClick);
                    }
                    
                    if (_currentStepOrderToClick >= STEP_MAX_COUNT)
                    {
                        _currentStepOrderToClick = 0;
                        _isClickableForRound = false;
                        OnEscapePathSuccess();
                        
                    }

                    
                    _stepBlinkSeq?.Kill();
                 
                }

            Logger.ContentTestLog("there's no animator for this avatar: " + hit.transform.name);
        }
    }
    

    private Sequence _stepBlinkSeq;
    
    
    private void BlinkStep(int currentRound , int stepIndex)
    {
        if (_stepBlinkSeq != null && _stepBlinkSeq.IsActive())
            _stepBlinkSeq.Kill();
        
        _stepBlinkSeq = DOTween.Sequence();
        _stepBlinkSeq.Append(_stepsMap[currentRound][stepIndex].DOScale(_defaultStepSize * 0.7f, 0.3f));
        _stepBlinkSeq.Append(_stepsMap[currentRound][stepIndex].DOScale(_defaultStepSize, 0.3f));
        _stepBlinkSeq.SetLoops(150,LoopType.Yoyo);
    }
}