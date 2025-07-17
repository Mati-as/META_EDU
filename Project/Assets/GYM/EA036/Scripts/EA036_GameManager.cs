using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

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
        //Camera3,

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
        TurnOnObjects,
        BookCases,
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
    
    // private readonly Dictionary<PoolType, Objects> _groupMap = new Dictionary<PoolType, Objects>
    // {
    //     { PoolType.Bell, Objects.BellStageTreeGroup },
    //     { PoolType.Bulb, Objects.BulbStageTreeGroup },
    //     { PoolType.Candy, Objects.CandyStageTreeGroup },
    //     { PoolType.Star, Objects.StarStageTreeGroup },
    // };
    
    public GameObject ObjectAppearPositions;
    
    protected override void Init()
    {
        Bind<CinemachineVirtualCamera>(typeof(Cameras));
        Bind<ParticleSystem>(typeof(Particle));
        BindObject(typeof(Objects));

        base.Init();

        _uiManager = UIManagerObj.GetComponent<EA036_UIManager>();
        _poolManager = FindObjectOfType<EA036_PoolManager>();    
        
        OnChangeStage += CheckWhenNextStage;
        
        _stage = MainSeq.Default;
        
        gamePlaying = false;
            
        psResourcePath = "SideWalk/Asset/Fx_Click";
        SetPool();

        // for (int i = (int)Objects.ActivatableObjects; i <= (int)Objects.TurnOnObjects; i++)
        //     foreach (Transform child in GetObject(i).transform)
        //         child.gameObject.SetActive(false);

        _clickClips = new AudioClip[5]; //오디오 char 캐싱 
        for (int i = 0; i < _clickClips.Length; i++)
            _clickClips[i] = Resources.Load<AudioClip>($"EA036/Audio/Click_{(char)('A' + i)}");

        ObjectAppearPositions = GetObject((int)Objects.StageObjectAppearPosition);


        // Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 12; //카메라들 우선순위 초기화
        // for (int i = (int)Cameras.Camera2; i <= (int)Cameras.Camera4; i++)
        //     Get<CinemachineVirtualCamera>(i).Priority = 10;

        //Managers.Sound.Play(SoundManager.Sound.Bgm, "EA033/Audio/BGM");

        // var stageParents = new[]
        // {
        //     GetObject((int)Objects.BellStageTreeGroup).transform,
        //     GetObject((int)Objects.BulbStageTreeGroup).transform,
        //     GetObject((int)Objects.CandyStageTreeGroup).transform,
        //     GetObject((int)Objects.StarStageTreeGroup).transform
        // };
        //
        // foreach (var parent in stageParents)
        //     foreach (Transform child in parent)
        //         child.gameObject.SetActive(false);


        // EA033_UIManager.OnNextButtonClicked -=         ;
        // EA033_UIManager.OnNextButtonClicked +=         ;
    }


    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();

        GameStart();
    }

    protected override void OnDestroy()
    {
        OnChangeStage -= CheckWhenNextStage;
        
        base.OnDestroy();
        
        UI_InScene_StartBtn.onGameStartBtnShut -= GameStart;
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (_stage == MainSeq.TableStage && gamePlaying)
        {
            OnRaySynced_ItemPick();
        }
        else if (_stage == MainSeq.TableStage)
        {


        }
        else if (_stage == MainSeq.ToysStage)
        {
            foreach (var hit in GameManager_Hits)
            {
                var hitGameObj = hit.collider.gameObject;
                
                var info = hitGameObj.GetComponent<CellInfo>();
                if (info == null) return;

                info.poolManager.ReleaseCell(info.row, info.col);

                //PoolManager.Instance.ReturnToPool(gameObject); //pool로 돌아가는 로직
            }
        }
    }

    private event Action OnChangeStage;

    private void OnRaySynced_ItemPick()
    {
        foreach (var hit in GameManager_Hits)
        {
            var hitGameObj = hit.collider.gameObject;

            effectPos = hit.point;
            PlayParticleEffect(effectPos);

            ClickedSound();

            //hitGameObj.GetComponent<Collider>().enabled = false;
            hitGameObj.SetActive(false);
                
            var appearObjTransform = GetObject((int)Objects.ActivatableObjects).transform.GetChild(index);
            appearObjTransform.gameObject.transform
                .DOScale(appearObjTransform.localScale, 1f)
                .From(Vector3.zero)
                .OnStart(() =>
                {
                    index++;
                    appearObjTransform.gameObject.SetActive(true);
                    OnChangeStage?.Invoke();
                });

        }
    }


    private void CheckWhenNextStage()
    {

        switch (index)
        {
            case 6:
                DOVirtual.DelayedCall(1f, () =>
                {
                    NextStage(MainSeq.ChairStage);
                    VictorySoundAndEffect();
                });
                break;
            case 12:
                DOVirtual.DelayedCall(1f, () =>
                {
                    NextStage(MainSeq.TableStage);
                    VictorySoundAndEffect();
                });
                break;
            case 18:
                DOVirtual.DelayedCall(1f, () =>
                {
                    NextStage(MainSeq.ToysStage);
                    VictorySoundAndEffect();
                });
                break;
        }
    }

    private void GameStart()
    {
        //if (_stage == MainSeq.Start)
            //NextStage(MainSeq.Start);
        NextStage(MainSeq.ToysStage);
    }

    private void NextStage(MainSeq next)
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
                NextStage(MainSeq.BookCaseStage);
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
        // DOTween.Sequence()
        //     .AppendCallback(() =>
        //     {
        //         Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 10;
        //         Get<CinemachineVirtualCamera>((int)Cameras.Camera2).Priority = 10;
        //
        //         _uiManager.PopInstructionUIFromScaleZero("이번에는 반짝이는 별장식을 터치해\n트리를 꾸며주세요!");
        //         Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_2_이번에는_반짝이는_별장식을_터치해_트리를_꾸며주세요_");
        //     })
        //     .AppendInterval(5.5f)
        //     .AppendCallback(() =>
        //     {
        //         _uiManager.ShutInstructionUI();
        //         _uiManager.PlayReadyAndStart();
        //     })
        //     .AppendInterval(5f)
        //     .AppendCallback(() =>
        //     {
        //         _uiManager.ActivateImageAndUpdateCount(4, 0);
        //     })
        //     ;
    }

    private void VictorySoundAndEffect()
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
    
    private void ClickedSound() 
    {
        int idx = Random.Range(0, _clickClips.Length);
        
        if (_clickClips[idx] == null)
            Logger.Log("사운드 경로에 없음");
        
        Managers.Sound.Play(SoundManager.Sound.Effect, _clickClips[idx]);
        
    }
    
    
}