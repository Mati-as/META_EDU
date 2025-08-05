using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class EA036_GameManager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Start,
        BookCaseStage,
        ChairStage,
        TableStage,
        ToysStage,
        End,
        Default
    }
    
    private MainSeq _stage;
    
    private enum Cameras
    {
        Camera1,
        Camera2,
        Camera3,

        //Camera4
        // Camera5
    }

    private enum Objects
    {
        ActivatableObjects,
        BookCaseStageObjects,
        ChairStageObjects,
        TableStageObjects,
        StageObjectAppearPosition,
        ToyPool,
        StartSpawnPosition,
        DisAppearPosition,
        TurnOnObjects,
        EndStageObj,
        ToyBox,
        BookCases,
    }

    private enum Particle
    {
        Victory1,
        Victory2
    }

    private Ea036InGameUIManager _inGameUIManager;
    private EA036_PoolManager _poolManager;

    private Vector3 effectPos;
    
    private AudioClip[] _clickClips;
    
    public GameObject objectAppearPositions;
    public GameObject toysPoolParent;
    public GameObject[] toySpawnPositions;

    private Vector3 endPoint;
    
    private int appearBookCaseToyObjNum;
    private bool canTouchUI = true;

    public Transform toyBox;
    
    [SerializeField] private int targetTouchToyCount = 20;
    
    protected override void Init()
    {
        Bind<CinemachineVirtualCamera>(typeof(Cameras));
        Bind<ParticleSystem>(typeof(Particle));
        BindObject(typeof(Objects));

        base.Init();

        _inGameUIManager = UIManagerObj.GetComponent<Ea036InGameUIManager>();
        _poolManager = FindObjectOfType<EA036_PoolManager>();
        toySpawnPositions = new GameObject[2];
        
        _stage = MainSeq.Default;
        
        Managers.Sound.Play(SoundManager.Sound.Bgm, "EA036/Audio/EA036_BGM");
            
        psResourcePath = "SideWalk/Asset/Fx_Click";
        SetPool();

        // for (int i = (int)Objects.ActivatableObjects; i <= (int)Objects.TurnOnObjects; i++)
        //     foreach (Transform child in GetObject(i).transform)
        //         child.gameObject.SetActive(false);

        _clickClips = new AudioClip[5]; //오디오 char 캐싱 
        for (int i = 0; i < _clickClips.Length; i++)
            _clickClips[i] = Resources.Load<AudioClip>($"EA036/Audio/Click_{(char)('A' + i)}");

        objectAppearPositions = GetObject((int)Objects.StageObjectAppearPosition);
        endPoint = GetObject((int)Objects.DisAppearPosition).transform.position;
        toyBox = GetObject((int)Objects.ToyBox).transform;
        
        for (int i = 0; i < GetObject((int)Objects.StartSpawnPosition).transform.childCount; i++)
        {
            toySpawnPositions[i] = GetObject((int)Objects.StartSpawnPosition).transform.GetChild(i).gameObject;
        }

        toysPoolParent = GetObject((int)Objects.ToyPool);
        foreach (Transform child in toysPoolParent.transform)
        {
            child.gameObject.SetActive(false);
        }


        Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 12; //카메라들 우선순위 초기화
        for (int i = (int)Cameras.Camera2; i <= (int)Cameras.Camera3; i++)
            Get<CinemachineVirtualCamera>(i).Priority = 10;

        foreach (Transform child in GetObject((int)Objects.DisAppearPosition).transform) //교구장 배치 하는 장난감들 비활성화
        {
            child.gameObject.SetActive(false);
        }
        
        foreach (Transform child in GetObject((int)Objects.ActivatableObjects).transform) //각 스테이지 표시 기물들 비활성화
        {
            child.gameObject.SetActive(false);
        }
        
        foreach (Transform child in GetObject((int)Objects.TurnOnObjects).transform) //스테이지 종료 후 나타나는 기물들 비활성화
        {
            child.gameObject.SetActive(false);
        }
        
        foreach (Transform child in GetObject((int)Objects.ToyBox).transform) //장난감 스테이지 장난감 주머니 비활성화
        {
            child.gameObject.SetActive(false);
        }
        
        foreach (Transform child in GetObject((int)Objects.BookCases).transform) // 장난감스테이지 교구장 비활성화
        {
            child.gameObject.SetActive(false);
        }
        
        foreach (Transform child in GetObject((int)Objects.BookCaseStageObjects).transform) // 교구장 스테이지 교구장 비활성화
        {
            child.gameObject.SetActive(false);
        }
        
        foreach (Transform child in GetObject((int)Objects.ChairStageObjects).transform) // 의자 스테이지 의자 비활성화
        {
            child.gameObject.SetActive(false);
        }
        
        foreach (Transform child in GetObject((int)Objects.TableStageObjects).transform) // 책상 스테이지 책상 비활성화
        {
            child.gameObject.SetActive(false);
        }
        
        //Managers.Sound.Play(SoundManager.Sound.Bgm, "EA033/Audio/BGM");

    }


    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();

        StartGame();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        UI_InScene_StartBtn.onGameStartBtnShut -= StartGame;
    }
    
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (_stage == MainSeq.BookCaseStage)
        {
            OnRaySynced_ItemPick(MainSeq.ChairStage);
        }
        else if (_stage == MainSeq.ChairStage)
        {
            OnRaySynced_ItemPick(MainSeq.TableStage);
        }
        else if (_stage == MainSeq.TableStage)
        {
            OnRaySynced_ItemPick(MainSeq.ToysStage);
        }
        else if (_stage == MainSeq.ToysStage)
        {
            foreach (var hit in GameManager_Hits)
            {
                var hitGameObj = hit.collider.gameObject;

                var info = hitGameObj.GetComponent<CellInfo>();
                if (info == null || !info.canClicked) return;

                effectPos = hit.point;
                PlayParticleEffect(effectPos);

                PlayClickSound();

                Tween jump = hitGameObj.transform
                    .DOJump(
                        endPoint,
                        1f,
                        1,
                        2.3f
                    )
                    .SetEase(Ease.OutExpo)
                    .OnStart(() => info.canClicked = false)
                    .OnComplete(() =>
                    {
                        info.canClicked = true;
                        hitGameObj.SetActive(false);
                        if (appearBookCaseToyObjNum < 8)
                        {
                            Transform target = GetObject((int)Objects.DisAppearPosition).transform.GetChild(appearBookCaseToyObjNum);

                            Vector3 childOriginScale = target.localScale;
                            target.localScale = Vector3.zero;
                            target.gameObject.SetActive(true);
                            target.DOScale(childOriginScale, 1f)
                                .SetEase(Ease.OutBounce); 
                            
                            appearBookCaseToyObjNum++;
                        }

                        currentObjectClickedCount++;

                        if (currentObjectClickedCount == targetTouchToyCount)
                        {
                            currentObjectClickedCount = 0;
                            PlayVictorySoundAndEffect();
                            ChangeStage(MainSeq.End);
                        }
                    });

                Sequence seq = DOTween.Sequence();
                seq.Append(jump);
                seq.InsertCallback(
                    atPosition: 0.5f,
                    callback: () =>
                    {
                        _poolManager.ReleaseCell(info.row, info.col);
                    }
                );
            }
        }
        else if (_stage == MainSeq.End && canTouchUI)
        {
            foreach (var hit in GameManager_Hits)
            {
                var hitGameObj = hit.collider.gameObject;
                string objName = hit.collider.gameObject.name;

                switch (objName)
                {
                    case "BookCase":
                        canTouchUI = false;
                        DOVirtual.DelayedCall(0.5f, () => canTouchUI = true);
                        hitGameObj.transform.DOShakePosition(0.3f);
                        _inGameUIManager.TouchUIEndStage(1);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA036/Audio/audio_19_교구장");
                        break;
                    case "Table":
                        canTouchUI = false;
                        DOVirtual.DelayedCall(0.5f, () => canTouchUI = true);
                        hitGameObj.transform.DOShakeScale(0.3f);
                        _inGameUIManager.TouchUIEndStage(2);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA036/Audio/audio_17_책상");
                        break;
                    case "Chair":
                        canTouchUI = false;
                        DOVirtual.DelayedCall(0.5f, () => canTouchUI = true);
                        hitGameObj.transform.DOShakeScale(0.3f);
                        _inGameUIManager.TouchUIEndStage(3);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA036/Audio/audio_18_의자");
                        break;

                }
            }
        }
    }

    [SerializeField] private int currentObjectClickedCount;
    [SerializeField] private int CHECKCOUNT_TO_NextStage = 6;

    private int appearObjNum;
    
    private void OnRaySynced_ItemPick(MainSeq nextStage)
    {
        foreach (var hit in GameManager_Hits)
        {
            var hitGameObj = hit.collider.gameObject;
            
            effectPos = hit.point;
            PlayParticleEffect(effectPos);

            PlayClickSound();

            hitGameObj.transform.DOKill();
            hitGameObj.SetActive(false);

            var appearObjTransform = GetObject((int)Objects.ActivatableObjects).transform.GetChild(appearObjNum);
            appearObjTransform.gameObject.transform
                .DOScale(appearObjTransform.localScale, 0.5f)
                .From(Vector3.zero)
                .OnStart(() =>
                {
                    appearObjNum++;
                    currentObjectClickedCount++;
                    appearObjTransform.gameObject.SetActive(true);
                })
                .OnComplete(() =>
                {
                    if (currentObjectClickedCount >= CHECKCOUNT_TO_NextStage)
                    {
                        currentObjectClickedCount = 0;
                        PlayVictorySoundAndEffect();
                        ChangeStage(nextStage);
                    }
                });

        }
    }

    private void StartGame()
    {
        //if (_stage == MainSeq.Start)
            ChangeStage(MainSeq.Start);
        //ChangeStage(MainSeq.ToysStage);
    }

    private void ChangeStage(MainSeq next)
    {
        if (_stage == next)
            return;
        
        _stage = next;
        switch (next)
        {
            case MainSeq.Start: OnStartStage(); break;
            case MainSeq.TableStage: OnTableStage(); break;
            case MainSeq.ChairStage: OnChairStage(); break;
            case MainSeq.BookCaseStage: OnBookCaseStage(); break;
            case MainSeq.ToysStage: OnToysStage(); break;
            case MainSeq.End: OnEndStage(); break;
        }
        
        Logger.Log($"{next}스테이지로 변경");
    }

    private void OnStartStage()
    {

        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("교실을 꾸며요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_0_교실을_꾸며요");
            })
            .AppendInterval(2.7f)
            .AppendCallback(() =>
            {
                _inGameUIManager.ShutInstructionUI();
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_1_교실이_텅_비어있어요");
            })
            .AppendInterval(3.4f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("친구들 우리 교실을 같이 꾸며볼까요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_2_친구들_우리_교실을_같이_꾸며볼까요_");
            })
            .AppendInterval(4.5f)
            .AppendCallback(() =>
            {
                _inGameUIManager.ShutInstructionUI();
                ChangeStage(MainSeq.BookCaseStage);
            })
            ;

        //인트로 카메라 무빙 기능 추가
        
    }

    //private Transform originalScale;
    private void OnBookCaseStage()
    {
        DOTween.Sequence()
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("교실에는 무엇이 필요할까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_3_교실에는_무엇이_필요할까요_");
            })
            .AppendInterval(3.5f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("교구장부터 교실에 놓아줄까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_4_교구장부터_교실에_놓아줄까요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() => AppearStageObjects(Objects.BookCaseStageObjects))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("교구장을 터치해주세요");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_5_교구장을_터치해주세요");
            })
            ;
    }

    private void OnChairStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("교구장이 생겼어요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_6_교구장이_생겼어요");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("친구들이 앉을 의자가 필요해요");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_7_친구들이_앉을_의자가_필요해요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("의자를 터치해 놓아줄까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_8_의자를_터치해_놓아줄까요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() => AppearStageObjects(Objects.ChairStageObjects))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("의자를 터치해주세요");
                //Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_0_의자을_터치해주세요");
            })
            
        
            ;
    }

    private void OnTableStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("의자가 생겼어요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_9_의자가__생겼어요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("의자 앞에 놓을 책상이 필요해요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_10_의자_앞에_놓을_책상이_필요해요");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("책상을 터치해 놓아주세요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_11_책상을_터치해_놓아주세요");
            })
            .AppendInterval(3f)
            .AppendCallback(() => AppearStageObjects(Objects.TableStageObjects))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("책상을 터치해주세요!");
                //Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_1_책상을_터치해주세요");
            })
            ;
    }

    private void OnToysStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("책상을 생겼어요");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_12_책상이_생겼어요_");

                foreach (Transform child in GetObject((int)Objects.TurnOnObjects).transform)
                {
                    Vector3 origin = child.transform.localScale;
                    child.gameObject.transform.DOScale(origin, 1f).SetEase(Ease.InBounce)
                        .From(Vector3.zero)
                        .OnStart(() => child.gameObject.SetActive(true));
                }
                
                foreach (Transform child in GetObject((int)Objects.BookCases).transform)
                {
                    Vector3 origin = child.transform.localScale;
                    child.gameObject.transform.DOScale(origin, 1f).SetEase(Ease.InBounce)
                        .From(Vector3.zero)
                        .OnStart(() => child.gameObject.SetActive(true));
                }
                
                foreach (Transform child in GetObject((int)Objects.ToyBox).transform)
                {
                    Vector3 origin = child.transform.localScale;
                    child.gameObject.transform.DOScale(origin, 1f).SetEase(Ease.InBounce)
                        .From(Vector3.zero)
                        .OnStart(() => child.gameObject.SetActive(true));
                }
                
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                for (int i = 6; i < GetObject((int)Objects.ActivatableObjects).transform.childCount; i++)
                {
                    Transform child = GetObject((int)Objects.ActivatableObjects).transform.GetChild(i).transform;
                    child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.InBounce)
                        .OnComplete(() => child.gameObject.SetActive(false));
                }
                
                Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 10;
                Get<CinemachineVirtualCamera>((int)Cameras.Camera2).Priority = 12;
                
                _inGameUIManager.PopInstructionUIFromScaleZero("이제 교구장에 장난감들을 넣어볼까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_13_이제_교구장에_장난감들을_넣어볼까요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _inGameUIManager.PopInstructionUIFromScaleZero("마음에 드는 장난감을 터치해 놓아주세요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_14_마음에_드는_장난감을_마음껏_터치해_놓아주세요");
                _poolManager.spawnSeq.Play();
            })
            ;
    }

    private void OnEndStage()
    {
        GameObject endStageObjParent = GetObject((int)Objects.EndStageObj);

        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _poolManager.spawnSeq.Kill();

                foreach (Transform child in GetObject((int)Objects.ToyPool).transform)
                {
                    child.DOScale(Vector3.zero, 1f).SetEase(Ease.InBounce)
                        .OnComplete(() => child.gameObject.SetActive(false));
                }

                for (int i = 4; i < GetObject((int)Objects.DisAppearPosition).transform.childCount; i++)
                {
                    GetObject((int)Objects.DisAppearPosition).transform.GetChild(i).gameObject.SetActive(true);
                }

                Get<CinemachineVirtualCamera>((int)Cameras.Camera2).Priority = 10;
                Get<CinemachineVirtualCamera>((int)Cameras.Camera3).Priority = 12;

                _inGameUIManager.PopInstructionUIFromScaleZero("와 친구들 교실이 완성되었어요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_15_와_친구들_교실에_완성되었어요_");
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                _inGameUIManager.ShutInstructionUI();
                Get<CinemachineVirtualCamera>((int)Cameras.Camera3).Priority = 10;
                Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 12;
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                for (int i = 6; i < GetObject((int)Objects.ActivatableObjects).transform.childCount; i++)
                {
                    Transform child = GetObject((int)Objects.ActivatableObjects).transform.GetChild(i).transform;
                    child.gameObject.transform.DOScale(Vector3.one, 1f).SetEase(Ease.InBounce)
                        .From(Vector3.zero)
                        .OnStart(() => child.gameObject.SetActive(true));
                }
                
                _inGameUIManager.PopInstructionUIFromScaleZero("어떤것들을 놓았는지 알아볼까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_16_어떤것들을_놓았는지_알아볼까요_");
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                endStageObjParent.transform.GetChild(3).gameObject.transform.DOJump(
                        endStageObjParent.transform.GetChild(0).transform.position,
                        1f,
                        1,
                        1f
                    ).SetEase(ease: Ease.Linear)
                    .OnStart(() => endStageObjParent.transform.GetChild(3).gameObject.SetActive(true))
                    .OnComplete(() =>
                    {
                        _inGameUIManager.ActivateUIEndStage(1);
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_19_교구장");
                        endStageObjParent.transform.GetChild(3).gameObject.transform.DOShakePosition(0.3f);
                    });
            })
            .AppendInterval(4.3f)
            .AppendCallback(() =>
            {
                endStageObjParent.transform.GetChild(4).gameObject.transform.DOJump(
                        endStageObjParent.transform.GetChild(1).transform.position,
                        1f,
                        1,
                        1f
                    ).SetEase(ease: Ease.Linear)
                    .OnStart(() => endStageObjParent.transform.GetChild(4).gameObject.SetActive(true))
                    .OnComplete(() =>
                    {
                        _inGameUIManager.ActivateUIEndStage(2);
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_17_책상");
                        endStageObjParent.transform.GetChild(4).gameObject.transform.DOShakeScale(0.3f);
                    });
            })
            .AppendInterval(4.3f)
            .AppendCallback(() =>
            {
                endStageObjParent.transform.GetChild(5).gameObject.transform.DOJump(
                        endStageObjParent.transform.GetChild(2).transform.position,
                        1f,
                        1,
                        1f
                    ).SetEase(ease: Ease.Linear)
                    .OnStart(() => endStageObjParent.transform.GetChild(5).gameObject.SetActive(true))
                    .OnComplete(() =>
                    {
                        _inGameUIManager.ActivateUIEndStage(3);
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_18_의자");
                        endStageObjParent.transform.GetChild(5).gameObject.transform.DOShakeScale(0.3f);
                    });
            })
            .AppendCallback(() =>
            {
                RestartScene(delay: 14);
            })
            ;
    }

    private void PlayVictorySoundAndEffect()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA036/Audio/audio_Victory");

        Get<ParticleSystem>((int)Particle.Victory1).Play();
        Get<ParticleSystem>((int)Particle.Victory2).Play();
    }
    
    private void AppearStageObjects(Objects objs)
    {
        GameObject parent = GetObject((int)objs);
        
        foreach (Transform child in parent.transform)
        {
            Vector3 targetScale = child.localScale;
        
            child.localScale = Vector3.zero;
            child.gameObject.SetActive(true);

            child.DOScale(targetScale, 1f)
                .SetEase(Ease.InBounce)
                .From(Vector3.zero)
                .OnComplete(() =>
                    child.DOScale(targetScale * 1.1f, 0.5f).SetLoops(54321, LoopType.Yoyo));
        }
    }
    
    private void PlayClickSound() 
    {
        int idx = Random.Range(0, _clickClips.Length);
        
        if (_clickClips[idx] == null)
            Logger.Log("사운드 경로에 없음");
        
        Managers.Sound.Play(SoundManager.Sound.Effect, _clickClips[idx]);
        
    }
    
    
}