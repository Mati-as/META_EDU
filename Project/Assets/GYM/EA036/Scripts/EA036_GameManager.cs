using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
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
        TurnOnObjects,
        BookCases,
    }

    private enum Particle
    {
        Victory1,
        Victory2
    }

    private EA036_UIManager _uiManager;
    
    [SerializeField]
    private bool gamePlaying;
    [SerializeField] 
    private int index;
    
    private Vector3 effectPos;
    
    private AudioClip[] _clickClips;
    
    // private readonly Dictionary<PoolType, Objects> _groupMap = new Dictionary<PoolType, Objects>
    // {
    //     { PoolType.Bell, Objects.BellStageTreeGroup },
    //     { PoolType.Bulb, Objects.BulbStageTreeGroup },
    //     { PoolType.Candy, Objects.CandyStageTreeGroup },
    //     { PoolType.Star, Objects.StarStageTreeGroup },
    // };
    
    protected override void Init()
    {
        Bind<CinemachineVirtualCamera>(typeof(Cameras));
        Bind<ParticleSystem>(typeof(Particle));
        BindObject(typeof(Objects));

        base.Init();

        _uiManager = UIManagerObj.GetComponent<EA036_UIManager>();
            
        OnChangeStage += CheckWhenNextStage;
        
        _stage = MainSeq.Default;
        
        gamePlaying = false;
            
        psResourcePath = "SideWalk/Asset/Fx_Click";
        SetPool();

        for (int i = (int)Objects.ActivatableObjects; i <= (int)Objects.TurnOnObjects; i++)
            foreach (Transform child in GetObject(i).transform)
                child.gameObject.SetActive(false);

        _clickClips = new AudioClip[5]; //오디오 char 캐싱 
        for (int i = 0; i < _clickClips.Length; i++)
            _clickClips[i] = Resources.Load<AudioClip>($"EA036/Audio/Click_{(char)('A' + i)}");


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

        // if (_stage == MainSeq.TableStage && gamePlaying)
        // {
                OnRaySynced_ItemPick();
        // }
        // else if (_stage == MainSeq.TableStage)
        // {
        //     
        //     
        // }
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

            hitGameObj.SetActive(false);
                
            var appearObjTransform = GetObject((int)Objects.ActivatableObjects).transform.GetChild(index);
            appearObjTransform.gameObject.transform
                .DOScale(appearObjTransform.localScale, 1f)
                .From(Vector3.zero)
                .OnStart(() =>
                {
                    index++;
                    appearObjTransform.gameObject.SetActive(true);
                })
                .OnComplete(() => DOVirtual.DelayedCall(1f, () => OnChangeStage?.Invoke()));

        }
    }

    private void CheckWhenNextStage()
    {
        switch (index)
        {
            case 6:
                NextStage(MainSeq.ChairStage);
                break;
            case 12:
                NextStage(MainSeq.TableStage);
                break;
            case 18:
                NextStage(MainSeq.ToysStage);
                break;
        }
    }

    private void GameStart()
    {
        //if (_stage == MainSeq.Start)
            NextStage(MainSeq.Start);
        //NextStage(MainSeq.OnFinish);
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



        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.BookCaseStage));
    }

    private Transform originalScale;
    private void OnBookCaseStage()
    {
        DOTween.Sequence()
            .AppendCallback(() => AppearStageObjects(Objects.BookCaseStageObjects))
            ;
    }

    private void OnChairStage()
    {
        DOTween.Sequence()
            .AppendCallback(() => AppearStageObjects(Objects.ChairStageObjects))
            ;
    }

    private void OnTableStage()
    {
        DOTween.Sequence()
            .AppendCallback(() => AppearStageObjects(Objects.TableStageObjects))
            ;
        
    }

    private void OnToysStage()
    {
        // DOTween.Sequence()
        //     .AppendCallback(() =>
        //     {
        //         Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 10;
        //         Get<CinemachineVirtualCamera>((int)Cameras.Camera2).Priority = 10;
        //
        //         _uiManager.PopInstructionUIFromScaleZero("이번에는 맛있는 사탕장식을 터치해\n트리를 꾸며주세요!");
        //         Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_1_이번에는_맛있는_사탕장식을_터치해__트리를_꾸며주세요_");
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
        //         _uiManager.ActivateImageAndUpdateCount(3, 0);
        //     })
        //     ;
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

    private void OnFinishStage()
    {
        //DOTween.Sequence()
            // .AppendCallback(() =>
            // {
            //     _uiManager.PopInstructionUIFromScaleZero("트리를 열심히 꾸몄구나 고마워!");
            //     Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_11_트리를_열심히_꾸몄구나_고마워_");
            // })
            // .AppendInterval(4f)
            // .AppendCallback(() =>
            // {
            //     _uiManager.PopInstructionUIFromScaleZero("친구들 메리 크리스마스~!");
            //     Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_10_친구들_메리_크리스마스_");
            // })
            // ;
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
            Logger.Log($"{child.gameObject.name} 찾음");

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
        Managers.Sound.Play(SoundManager.Sound.Effect, _clickClips[idx]);
        
        if (_clickClips[idx] == null)
            Logger.Log("사운드 경로에 없음");
    }
    
}