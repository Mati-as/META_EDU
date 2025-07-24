using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class EA036_GameManager : Base_GameManager
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
    }

    private enum Particle
    {
        Victory1,
        Victory2
    }

    private EA036_UIManager _uiManager;
    private EA036_PoolManager _poolManager;
    
    [SerializeField]
    private bool gamePlaying;
    [SerializeField] 
    private int index = 0;
    
    private Vector3 effectPos;
    
    private AudioClip[] _clickClips;
    
    public GameObject objectAppearPositions;
    public GameObject toysPoolParent;
    public GameObject[] toySpawnPositions;

    private Vector3 endPoint;
    
    private int appearBookCaseToyObjNum = 0;
    private bool canToundUI = true;

    public Transform ToyBox;
    
    protected override void Init()
    {
        Bind<CinemachineVirtualCamera>(typeof(Cameras));
        Bind<ParticleSystem>(typeof(Particle));
        BindObject(typeof(Objects));

        base.Init();

        _uiManager = UIManagerObj.GetComponent<EA036_UIManager>();
        _poolManager = FindObjectOfType<EA036_PoolManager>();
        toySpawnPositions = new GameObject[2];
        
        
        _stage = MainSeq.Default;
        
        gamePlaying = false;

        Managers.Sound.Play(SoundManager.Sound.Bgm, "EA036/Audio/EA036_BGM");
            
        PsResourcePath = "SideWalk/Asset/Fx_Click";
        SetPool();

        // for (int i = (int)Objects.ActivatableObjects; i <= (int)Objects.TurnOnObjects; i++)
        //     foreach (Transform child in GetObject(i).transform)
        //         child.gameObject.SetActive(false);

        _clickClips = new AudioClip[5]; //오디오 char 캐싱 
        for (int i = 0; i < _clickClips.Length; i++)
            _clickClips[i] = Resources.Load<AudioClip>($"EA036/Audio/Click_{(char)('A' + i)}");

        objectAppearPositions = GetObject((int)Objects.StageObjectAppearPosition);
        endPoint = GetObject((int)Objects.DisAppearPosition).transform.position;
        ToyBox = GetObject((int)Objects.ToyBox).transform;
        
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

        foreach (Transform child in GetObject((int)Objects.DisAppearPosition).transform) //교구장 배치 하는 장난감들 초기화
        {
            child.gameObject.SetActive(false);
        }
        
        foreach (Transform child in GetObject((int)Objects.ActivatableObjects).transform) //각 스테이지 표시 기물들 초기화
        {
            child.gameObject.SetActive(false);
        }
        
        foreach (Transform child in GetObject((int)Objects.TurnOnObjects).transform) //스테이지 종료 후 나타나는 기물들 초기화
        {
            child.gameObject.SetActive(false);
        }
        
        //Managers.Sound.Play(SoundManager.Sound.Bgm, "EA033/Audio/BGM");

    }


    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();

        GameStart();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        UI_InScene_StartBtn.onGameStartBtnShut -= GameStart;
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
                        if (appearBookCaseToyObjNum < 4)
                        {
                            GetObject((int)Objects.DisAppearPosition).transform.GetChild(appearBookCaseToyObjNum)
                                .gameObject
                                .SetActive(true);
                            appearBookCaseToyObjNum++;
                        }

                        index++;
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
        else if (_stage == MainSeq.End && canToundUI)
        {
            foreach (var hit in GameManager_Hits)
            {
                var hitGameObj = hit.collider.gameObject;
                string objName = hit.collider.gameObject.name;

                switch (objName)
                {
                    case "BookCase":
                        canToundUI = false;
                        DOVirtual.DelayedCall(0.5f, () => canToundUI = true);
                        hitGameObj.transform.DOShakePosition(0.3f);
                        _uiManager.TouchUIEndStage(1);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA036/Audio/audio_19_교구장");
                        break;
                    case "Table":
                        canToundUI = false;
                        DOVirtual.DelayedCall(0.5f, () => canToundUI = true);
                        hitGameObj.transform.DOShakeScale(0.3f);
                        _uiManager.TouchUIEndStage(2);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA036/Audio/audio_17_책상");
                        break;
                    case "Chair":
                        canToundUI = false;
                        DOVirtual.DelayedCall(0.5f, () => canToundUI = true);
                        hitGameObj.transform.DOShakeScale(0.3f);
                        _uiManager.TouchUIEndStage(3);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA036/Audio/audio_18_의자");
                        break;

                }
            }
        }
    }

    [SerializeField] private int currentObjectClickedCount = 0;
    [SerializeField] private int CHECKCOUNT_TO_NextStage = 6;

    private void OnRaySynced_ItemPick(MainSeq nextStage)
    {
        foreach (var hit in GameManager_Hits)
        {
            var hitGameObj = hit.collider.gameObject;

            effectPos = hit.point;
            PlayParticleEffect(effectPos);

            PlayClickSound();

            hitGameObj.SetActive(false);

            var appearObjTransform = GetObject((int)Objects.ActivatableObjects).transform.GetChild(index);
            appearObjTransform.gameObject.transform
                .DOScale(appearObjTransform.localScale, 1f)
                .From(Vector3.zero)
                .OnStart(() =>
                {
                    currentObjectClickedCount++;
                    appearObjTransform.gameObject.SetActive(true);
                });

            if (currentObjectClickedCount >= CHECKCOUNT_TO_NextStage)
            {
                currentObjectClickedCount = 0;
                PlayVictorySoundAndEffect();
                ChangeStage(nextStage);
            }
        }
    }

    private void GameStart()
    {
        //if (_stage == MainSeq.Start)
            //NextStage(MainSeq.Start);
        ChangeStage(MainSeq.ToysStage);
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
                _uiManager.PopInstructionUIFromScaleZero("교실을 꾸며요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_0_교실을_꾸며요");
            })
            .AppendInterval(2.7f)
            .AppendCallback(() =>
            {
                _uiManager.ShutInstructionUI();
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_1_교실이_텅_비어있어요");
            })
            .AppendInterval(3.4f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("친구들 우리 교실을 같이 꾸며볼까요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_2_친구들_우리_교실을_같이_꾸며볼까요_");
            })
            .AppendInterval(4.5f)
            .AppendCallback(() =>
            {
                _uiManager.ShutInstructionUI();
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
                _uiManager.PopInstructionUIFromScaleZero("교실에는 무엇이 필요할까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_3_교실에는_무엇이_필요할까요_");
            })
            .AppendInterval(3.5f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("교구장부터 교실에 놓아줄까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_4_교구장부터_교실에_놓아줄까요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() => AppearStageObjects(Objects.BookCaseStageObjects))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("교구장을 터치해주세요");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_5_교구장을_터치해주세요");
            })
            ;
    }

    private void OnChairStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("교구장이 생겼어요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_6_교구장이_생겼어요");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("친구들이 앉을 의자가 필요해요");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_7_친구들이_앉을_의자가_필요해요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("의자를 터치해 놓아줄까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_8_의자를_터치해_놓아줄까요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() => AppearStageObjects(Objects.ChairStageObjects))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("의자를 터치해주세요");
                //Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_0_의자을_터치해주세요");
            })
            
        
            ;
    }

    private void OnTableStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("의자가 생겼어요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_9_의자가__생겼어요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("의자 앞에 놓을 책상이 필요해요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_10_의자_앞에_놓을_책상이_필요해요");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("책상을 터치해 놓아주세요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_11_책상을_터치해_놓아주세요");
            })
            .AppendInterval(3f)
            .AppendCallback(() => AppearStageObjects(Objects.TableStageObjects))
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("책상을 터치해주세요!");
                //Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_1_책상을_터치해주세요");
            })
            ;
    }

    private void OnToysStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("책상을 생겼어요");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_12_책상이_생겼어요_");

                foreach (Transform child in GetObject((int)Objects.TurnOnObjects).transform)
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
                Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 10;
                Get<CinemachineVirtualCamera>((int)Cameras.Camera2).Priority = 12;
                
                _uiManager.PopInstructionUIFromScaleZero("이제 교구장에 장난감들을 넣어볼까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_13_이제_교구장에_장난감들을_넣어볼까요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("마음에 드는 장난감을 터치해 놓아주세요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_14_마음에_드는_장난감을_마음껏_터치해_놓아주세요");
                _poolManager.spawnSeq.Play();
            })
            ;
    }

    private void OnEndStage()
    {
        GameObject EndStageObjParent = GetObject((int)Objects.EndStageObj);
        
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _poolManager.spawnSeq.Kill();
                
                for (int i = 4; i < GetObject((int)Objects.DisAppearPosition).transform.childCount; i++)
                {
                    GetObject((int)Objects.DisAppearPosition).transform.GetChild(i).gameObject.SetActive(true);
                }

                Get<CinemachineVirtualCamera>((int)Cameras.Camera2).Priority = 10;
                Get<CinemachineVirtualCamera>((int)Cameras.Camera3).Priority = 12;

                _uiManager.PopInstructionUIFromScaleZero("와 친구들 교실이 완성되었어요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_15_와_친구들_교실에_완성되었어요_");
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                _uiManager.ShutInstructionUI();
                Get<CinemachineVirtualCamera>((int)Cameras.Camera3).Priority = 10;
                Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 12;
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("어떤것들을 놓았는지 알아볼까요?");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_16_어떤것들을_놓았는지_알아볼까요_");
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                EndStageObjParent.transform.GetChild(3).gameObject.transform.DOJump(
                        EndStageObjParent.transform.GetChild(0).transform.position,
                        1f,
                        1,
                        1f
                    ).SetEase(ease: Ease.Linear)
                    .OnStart(() => EndStageObjParent.transform.GetChild(3).gameObject.SetActive(true))
                    .OnComplete(() =>
                    {
                        _uiManager.ActivateUIEndStage(1);
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_19_교구장");
                        EndStageObjParent.transform.GetChild(3).gameObject.transform.DOShakePosition(0.3f);
                    });
            })
            .AppendInterval(4.3f)
            .AppendCallback(() =>
            {
                EndStageObjParent.transform.GetChild(4).gameObject.transform.DOJump(
                        EndStageObjParent.transform.GetChild(1).transform.position,
                        1f,
                        1,
                        1f
                    ).SetEase(ease: Ease.Linear)
                    .OnStart(() => EndStageObjParent.transform.GetChild(4).gameObject.SetActive(true))
                    .OnComplete(() =>
                    {
                        _uiManager.ActivateUIEndStage(2);
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_17_책상");
                        EndStageObjParent.transform.GetChild(4).gameObject.transform.DOShakeScale(0.3f);
                    });
            })
            .AppendInterval(4.3f)
            .AppendCallback(() =>
            {
                EndStageObjParent.transform.GetChild(5).gameObject.transform.DOJump(
                        EndStageObjParent.transform.GetChild(2).transform.position,
                        1f,
                        1,
                        1f
                    ).SetEase(ease: Ease.Linear)
                    .OnStart(() => EndStageObjParent.transform.GetChild(5).gameObject.SetActive(true))
                    .OnComplete(() =>
                    {
                        _uiManager.ActivateUIEndStage(3);
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA036/Audio/audio_18_의자");
                        EndStageObjParent.transform.GetChild(5).gameObject.transform.DOShakeScale(0.3f);
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
                .From(Vector3.zero);
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