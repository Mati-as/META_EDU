using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EA038_GameManager : Ex_BaseGameManager
{
     private enum MainSeq
    {
        OnStart,
        OnIntro,
        OnBellStage,
        OnBulbStage,
        OnFinish
    }

    private MainSeq _stage;

    private enum Cameras
    {
        Camera1,
        Camera2,
    }

    private enum Objects
    {
        BellPool,
        BulbPool,
        
        IntroStageTreeGroup,

    }

    private enum Particle
    {
        Victory1,
        Victory2
    }

    private EA038_UIManager _uiManager;
    
    private Vector3 effectPos;

    // private readonly Dictionary<PoolType, Objects> _groupMap = new Dictionary<PoolType, Objects>
    // {
    //     { PoolType.Bell, Objects.BellStageTreeGroup },
    //     { PoolType.Bulb, Objects.BulbStageTreeGroup },
    //     { PoolType.Candy, Objects.CandyStageTreeGroup },
    //     { PoolType.Star, Objects.StarStageTreeGroup },
    // };
    
    protected override void Init()
    {
        //Bind<CinemachineVirtualCamera>(typeof(Cameras));
        BindObject(typeof(Objects));
        Bind<ParticleSystem>(typeof(Particle));

        base.Init();

        _uiManager = UIManagerObj.GetComponent<EA038_UIManager>();
            
        _stage = MainSeq.OnStart;

        psResourcePath = "SideWalk/Asset/Fx_Click";
        SetPool();

        // Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 12; //카메라들 우선순위 초기화
        // for (int i = (int)Cameras.Camera2; i <= (int)Cameras.Camera4; i++)
        //     Get<CinemachineVirtualCamera>(i).Priority = 10;
        //
        // Managers.Sound.Play(SoundManager.Sound.Bgm, "EA033/Audio/BGM");
        //
        // var stageParents = new[]
        // {
        //     GetObject((int)Objects.BellStageTreeGroup).transform,
        //     GetObject((int)Objects.BulbStageTreeGroup).transform,
        //     GetObject((int)Objects.CandyStageTreeGroup).transform,
        //     GetObject((int)Objects.StarStageTreeGroup).transform
        // };
        
        // GetObject((int)Objs.Intro_Triangles).gameObject.SetActive(false);
        // GetObject((int)Objs.Intro_Stars).gameObject.SetActive(false);

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
    
    private void GameStart()
    {
        //if (_stage == MainSeq.OnStart)
        NextStage(MainSeq.OnStart);
        //NextStage(MainSeq.OnFinish);
    }

    private void NextStage(MainSeq next)
    {
        _stage = next;
        switch (next)
        {
            case MainSeq.OnStart: OnStartStage(); break;
            // case MainSeq.OnIntro: OnIntroStage(); break;
            // case MainSeq.OnBellStage: OnBellStage(); break;
            // case MainSeq.OnBulbStage: OnBulbStage(); break;
        }
        
        Logger.Log($"{next}스테이지로 변경");
    }
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        // foreach (var hit in GameManager_Hits)
        // {
        //     var go = hit.collider.gameObject;
        //     go.transform.DOKill();
        //     string clickedName = go.name;
        //
        //     if (!allowedKeywords.Any(keyword => clickedName.Contains(keyword)))
        //         continue;
        //
        //     effectPos = hit.point;
        //     effectPos.y += 0.2f;
        //     PlayParticleEffect(effectPos);
        //
        //     // 클릭된 이름으로 PoolType 결정
        //     var poolType = Enum.GetValues(typeof(PoolType))
        //         .Cast<PoolType>()
        //         .FirstOrDefault(pt => clickedName.Contains(pt.ToString()));
        //
        //     ClickedSound(go.name);
        // }
    }

    private void OnStartStage()
    {
        _uiManager.PopInstructionUIFromScaleZero("친구와 함께 크리스마스 트리를 꾸며요!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_0_친구와_함께_크리스마스_트리를_꾸며요_");

        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnIntro));
    }
    
    private AudioClip[] _clickClips;
    
    private void PlayClickSound() 
    {
        int idx = Random.Range(0, _clickClips.Length);
        
        if (_clickClips[idx] == null)
            Logger.Log("사운드 경로에 없음");
        
        Managers.Sound.Play(SoundManager.Sound.Effect, _clickClips[idx]);
        
    }
    
    private void PlayVictorySoundAndEffect()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA036/Audio/audio_Victory");

        Get<ParticleSystem>((int)Particle.Victory1).Play();
        Get<ParticleSystem>((int)Particle.Victory2).Play();
    }
    
}
