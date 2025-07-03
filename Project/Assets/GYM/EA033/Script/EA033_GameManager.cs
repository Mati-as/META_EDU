using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using System.Linq;

public class EA033_GameManager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        OnStart,
        OnIntro,
        OnBellStage,
        OnBulbStage,
        OnCandyStage,
        OnStarStage,
        OnFinish
    }

    private MainSeq _stage;

    private enum Cameras 
    {
        Camera1,
        Camera2,
        Camera3,
        Camera4,
        // Camera5
        
    }
    
    private enum Objects
    {
        Candy1,
        Candy2,
        Candy3,
        Candy4,
        Candy5,
        Star1,
        Star2,
        Star3,
        Star4,
        Star5,
        
        
        StarStage,
        
    }
    
    private enum Particle
    {
        Victory1,
        Victory2,
    }
    
    private EA033_UIManager _uiManager;
    private bool gamePlaying;
    private int i;
    private Vector3 effectPos;
    
    [SerializeField] private Transform path1;
    [SerializeField] private Transform path2;
    [SerializeField] private Transform path3;
    
    private Transform starStageParent;
    
    protected override void Init()
    {
        Bind<CinemachineVirtualCamera>(typeof(Cameras));
        BindObject(typeof(Objects));
        Bind<ParticleSystem>(typeof(Particle));
        
        base.Init();
        
        _uiManager = UIManagerObj.GetComponent<EA033_UIManager>();
        
        _stage = MainSeq.OnStart;
        
        psResourcePath = "SideWalk/Asset/Fx_Click";
        SetPool();

        Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 12;      //카메라들 우선순위 초기화
        for (int i = (int)Cameras.Camera2; i <= (int)Cameras.Camera4; i++)
        {
            Get<CinemachineVirtualCamera>(i).Priority = 10;
        }

        Managers.Sound.Play(SoundManager.Sound.Bgm, "EA033/Audio/BGM");

        starStageParent = GetObject((int)Objects.StarStage).transform;
        
        foreach (Transform child in starStageParent.transform)
        {
            child.gameObject.SetActive(false);
        }
        
        // GetObject((int)Objs.Intro_Triangles).gameObject.SetActive(false);
        // GetObject((int)Objs.Intro_Stars).gameObject.SetActive(false);

        // EA033_UIManager.OnNextButtonClicked -=         ;
        // EA033_UIManager.OnNextButtonClicked +=         ;

    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();

        GameStart();
            
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        UI_InScene_StartBtn.onGameStartBtnShut -= GameStart;
    }
    
    public override void OnRaySynced()
    {
        if(!PreCheckOnRaySync()) return;

        if (_stage == MainSeq.OnBellStage && gamePlaying)
        {
            //Bell로 수정해야됨 테스트라 star로 해놓은것
            HandleAllowedObjectClick("Star", 11,1, MainSeq.OnBulbStage);
        }
        // else if (_stage == MainSeq.OnIntro && gamePlaying)
        // {
        //     HandleAllowedObjectClick("Bulb", 2, MainSeq.OnCandyStage);
        // }
        // else if (_stage == MainSeq.OnIntro && gamePlaying)
        // {
        //     HandleAllowedObjectClick("Candy", 3, MainSeq.OnStarStage);
        // }
        // else if (_stage == MainSeq.OnIntro && gamePlaying)
        // {
        //     HandleAllowedObjectClick("Star", 4, MainSeq.OnFinish);
        // }
    }

    private string[] allowedKeywords = { "Star", "Candy", "Bulb", "Bell" };
    
    private void HandleAllowedObjectClick(string keyWord, int stageObjNum , int uiImage , MainSeq next)
    {
        foreach (var hit in GameManager_Hits)
        {
            string clickedName = hit.collider.gameObject.name;

            // 4개 중 하나라도 포함되어 있지 않으면 건너뜀
            if (!allowedKeywords.Any(keyword => clickedName.Contains(keyword)))
                continue;

            effectPos = hit.point;
            effectPos.y += 0.2f;
            PlayParticleEffect(effectPos);

            if (clickedName.Contains(keyWord))
            {
                if (i < 9)
                {
                    int currentIndex = i; 
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        starStageParent.GetChild(currentIndex).gameObject.SetActive(true);
                    });
                }

                i++;
                _uiManager.ActivateImageAndUpdateCount(uiImage, i);

                CorrectClickedSound();

                var startPos = path1.position;
                var middlePos = path2.position;
                var endPos = path3.position;

                Vector3[] pathPoints = 
                {
                    startPos,
                    middlePos,
                    endPos,
                };
            
                hit.collider.gameObject.transform.DOPath(pathPoints, 1.5f, PathType.Linear);
                
                if (i == 5)
                    Managers.Sound.Play(SoundManager.Sound.Narration, $"EA033/Audio/audio_중간알림_{uiImage}");
                
                if (i == 10)
                {
                    i = 0;
                    gamePlaying = false;

                    VictorySoundAndEffect();
                    
                    DOVirtual.DelayedCall(1f, () =>
                    {
                        _uiManager.DeactivateImageAndUpdateCount();
                        
                        Get<CinemachineVirtualCamera>((int)Cameras.Camera4).Priority = 12;
                        Get<CinemachineVirtualCamera>((int)Cameras.Camera3).Priority = 10;  //여기도 수정 필요함 
                    });
                    DOVirtual.DelayedCall(4f, () => NextStage(next));
                }
            }
            else
            {
                
                hit.collider.gameObject.SetActive(false);

                WrongClickedSound();
            }
        }
    }

    
    private void GameStart()
    {
        //if (_stage == MainSeq.OnStart)
        NextStage(MainSeq.OnStart);
        //NextStage(MainSeq.HumanExample);

    }

    private void NextStage(MainSeq next)
    {
        _stage = next;
        switch (next)
        {
            case MainSeq.OnStart: OnStartStage(); break;
            case MainSeq.OnIntro: OnIntroStage(); break;
            case MainSeq.OnBellStage: OnBellStage(); break;
            case MainSeq.OnBulbStage: OnBulbStage(); break;
            case MainSeq.OnCandyStage: OnCandyStage(); break;
            case MainSeq.OnStarStage: OnStarStage(); break;
            case MainSeq.OnFinish: OnFinishStage(); break;
        }
        
        Logger.Log($"{next}스테이지로 변경");
    }

    private void OnStartStage()
    {
        _uiManager.PopFromZeroInstructionUI("친구와 함께 크리스마스 트리를 꾸며요!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_0_친구와_함께_크리스마스_트리를_꾸며요_");

        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnIntro));
    }

    private void OnIntroStage()
    {
        _uiManager.PopFromZeroInstructionUI("산타할아버지가 선물을 놓고 가실 수 있도록\n크리스마스 트리를 꾸며볼까요?");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_1_산타할아버지가_선물을_놓고_가실_수_있도록_크리스마스_트리를~");
        
        Get<CinemachineVirtualCamera>((int)Cameras.Camera1).Priority = 10;
        Get<CinemachineVirtualCamera>((int)Cameras.Camera2).Priority = 12;

        for (int i = (int)Objects.Candy1; i <= (int)Objects.Star5; i++)
        {
            var item = GetObject(i);
            item.transform.DOScale(0, 0.5f).SetEase(Ease.Linear)
                .OnComplete(() => item.SetActive(false));
        }

        DOVirtual.DelayedCall(7f, () => NextStage(MainSeq.OnBellStage));
    }

    private void OnBellStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                Get<CinemachineVirtualCamera>((int)Cameras.Camera2).Priority = 10;
                Get<CinemachineVirtualCamera>((int)Cameras.Camera3).Priority = 12;
                
                _uiManager.PopFromZeroInstructionUI("떨어진 방울장식을 터치해\n트리를 꾸며주세요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_2_떨어진_방울장식을_터치해_트리를_꾸며주세요_");

            })
            .AppendInterval(5.5f)
            .AppendCallback(() =>
            {
                _uiManager.ShutInstructionUI();
                _uiManager.PlayReadyAndStart();
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                gamePlaying = true;
                _uiManager.ActivateImageAndUpdateCount(1, 0);
            })
            ;

    }

    private void OnBulbStage()
    {
        _uiManager.PopFromZeroInstructionUI("이번에는 반짝이는 전구장식을 터치해\n트리를 꾸며주세요!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_0_이번에는_반짝이는_전구장식을_터치해__트리를_꾸며주세요_");

        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnCandyStage));
    }

    private void OnCandyStage()
    {
        _uiManager.PopFromZeroInstructionUI("이번에는 맛있는 사탕장식을 터치해\n트리를 꾸며주세요!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_1_이번에는_맛있는_사탕장식을_터치해__트리를_꾸며주세요_");

        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnStarStage));
    }

    private void OnStarStage()
    {
        _uiManager.PopFromZeroInstructionUI("이번에는 반짝이는 별장식을 터치해\n트리를 꾸며주세요!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_2_이번에는_반짝이는_별장식을_터치해_트리를_꾸며주세요_");
        
        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnFinish));

    }

    private void OnFinishStage()
    {
        _uiManager.PopFromZeroInstructionUI("트리를 열심히 꾸몄구나 고마워!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_11_트리를_열심히_꾸몄구나_고마워_");

        DOVirtual.DelayedCall(4.5f, () =>
        {
            _uiManager.PopFromZeroInstructionUI("친구들 메리 크리스마스~!");
            Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_10_친구들_메리_크리스마스_");
        });
    }

    private void CorrectClickedSound()
    {
        int randomNum = Random.Range(1, 8);
        Managers.Sound.Play(SoundManager.Sound.Effect, $"EA033/Audio/Bell_{randomNum}");
    }

    private void WrongClickedSound()
    {
        char randomLetter = (char)('A' + Random.Range(0, 6));
        Managers.Sound.Play(SoundManager.Sound.Effect, $"EA033/Audio/Click_{randomLetter}");
    }
    
    private void VictorySoundAndEffect()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA033/Audio/audio_Victory");
                    
        Get<ParticleSystem>((int)Particle.Victory1).Play();
        Get<ParticleSystem>((int)Particle.Victory2).Play();
    }
    
}
