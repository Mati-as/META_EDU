using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

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
        ToxicGas,
        SirenAlert,
        IntroAvatarController,
        TowelToCover,
        OnExitAvataController,
        
        ToxicGas_OnExit,
        OnEscapePaths,
        OnEscapeAvatarController,
        OnEscapeAvatar, // 애니메이션과 별개로 Transform 컨트롤
        
        InducingArrowPath_A,
        InducingArrowPath_B,
        InducingArrowPath_C,
    }

    private AvatarController _introAvatarController;
    private AvatarController _onExitAvatarController;
    private AvatarController  _onEscapeAvatarController;


    private ParticleSystem _sirenPs;
    private ParticleSystem _smokePs;

    public int CurrentMainMainSeq
    {
        get
        {
            return CurrentMainMainSequence;
        }
        set
        {
            CurrentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {((MainSeq)CurrentMainMainSeq).ToString()}");
            // commin Init Part.


            ChangeThemeSeqAnim(value);
            switch (value)
            {
                case (int)MainSeq.Default_LateInit:

                    break;

                case (int)MainSeq.OnIntro:
                    break;

                case (int)MainSeq.OnFireAndAlarm:
                    GetObject((int)Objs.ToxicGas).SetActive(true);
                    GetObject((int)Objs.SirenAlert).SetActive(true);
                    _uiManager.PopFromZeroInstructionUI("검정 연기가 보여~무슨 냄새지?");
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

                        _uiManager.PopFromZeroInstructionUI("친구들~ 불이 났어요! 모두 대피 해야해요!");
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
                    _uiManager.PopFromZeroInstructionUI("불이 나면 두손으로 코와 입을 막아요!");
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
                    _uiManager.PopFromZeroInstructionUI("다른 친구들을 터치하여 도와주세요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA031/4_Help");
                    GetObject((int)Objs.OnExitAvataController).SetActive(true);
                    break;


                case (int)MainSeq.TakeExit:
                    _uiManager.PopFromZeroInstructionUI("친구를 따라 비상구로 기어서 이동해주세요!");
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
                    Managers.Sound.Play(SoundManager.Sound.Bgm, "Bgm/EA031");
                    break;

                case (int)MainSeq.OnFinish:
               
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
        
        _pathAnimSAeq?.Kill();
        _pathAnimSAeq = DOTween.Sequence();
       // GetObject((int)Objs.OnEscapeAvatar).transform.position = _escapePathMap[pathIndex][0];

        int PathDuration = 10;
        _pathAnimSAeq.AppendInterval(delay);
        _pathAnimSAeq.AppendCallback( () =>
        {
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
            
            
        });
        
        _escapePathMap[pathIndex][0] = GetObject((int)Objs.InducingArrowPath_A).transform.position;
        GetObject((int)Objs.InducingArrowPath_A).SetActive(true);
        
        DOVirtual.DelayedCall(10, () =>
        {
            _arrowAnimSeq?.Kill();
            _arrowAnimSeq = DOTween.Sequence();
            
            _arrowAnimSeq.Append(GetObject((int)Objs.InducingArrowPath_A).transform
                .DOPath(_escapePathMap[pathIndex], PathDuration, PathType.CatmullRom)
                .SetEase(Ease.Linear));
            _arrowAnimSeq.AppendInterval(3f);
            _arrowAnimSeq.SetLoops(5, LoopType.Restart);
        });
;
    }

    private Dictionary<int, Vector3[]> _escapePathMap = new();

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

        Logger.Log("Init--------------------------------");
    }

    private Sequence _pathAnimSAeq;
    private Base_UIManager _uiManager;
    private Vector3 _towelDefaultScale;
    private Sequence _arrowAnimSeq;
    
    
    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        CurrentMainMainSeq = (int)MainSeq.OnIntro;
        _smokePs.Clear();
        _smokePs.Stop();
        _smokePs.Play();

        DOVirtual.DelayedCall(3f, () =>
        {
            CurrentMainMainSeq = (int)MainSeq.OnFireAndAlarm;
        });
        _introAvatarController = GetObject((int)Objs.IntroAvatarController).GetComponent<AvatarController>();
    }

    public override void OnRaySynced()
    {
        base.OnRaySynced();

        if (CurrentMainMainSeq == (int)MainSeq.CloseMouthAndNose)
        {
            Logger.ContentTestLog("OnRaySynced - CloseMouthAndNose");
            OnRaySyncOnAvatarClick();
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
                    _uiManager.PopFromZeroInstructionUI("모든 친구를 도와줬어!");

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
    
    private void OnRaySyncOnEscape()
    {
        
        foreach (var hit in GameManager_Hits)
        {
            int id = hit.transform.GetInstanceID();
            _isClickableMap.TryAdd(id, true);
            
            
            
            Logger.ContentTestLog("there's no animator for this avatar: " + hit.transform.name);
        }
    }
}