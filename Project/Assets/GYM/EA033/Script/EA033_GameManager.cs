using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

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

    
    public enum Objs //바인드 할 게임 오브젝트
    {
        Balloons,
        Balloon_RedHeart,
        Balloon_OrangeTriangle,
        Balloon_YellowStar,
        Balloon_GreenCircle,
        Balloon_BlueSquare
        
    }

    private EA033_UIManager _uiManager;
    
    protected override void Init()
    {
        BindObject(typeof(Objs));   //Bind<GameObject>(typeof(Objs));
        
        base.Init();
        
        _uiManager = UIManagerObj.GetComponent<EA033_UIManager>();
        
        _stage = MainSeq.OnStart;
        
        psResourcePath = "SideWalk/Asset/Fx_Click"; //클릭시 이펙트 풀 PlayParticleEffect(effectPos); 이걸로 가져다 사용
        SetPool(); //이펙트 풀
        
        // for(int i =(int)Objs.Intro_Hearts; i <= (int)Objs.Intro_Flowers; i++)
        // {
        //     for (int k = 0; k <  GetObject(i).transform.childCount; k++)
        //     {
        //         Transform child = GetObject(i).transform.GetChild(k);
        //         _defaultSizeMap.TryAdd(child.GetInstanceID(), child.localScale);
        //         child.localScale = Vector3.zero;
        //     }
        // }
        // GetObject((int)Objs.Intro_Hearts).gameObject.SetActive(false);
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
        
        if(_stage == (int)MainSeq.OnStart)
        {
            //OnRaySyncOnIntroduce();
        }
        else if (_stage == MainSeq.OnIntro)
        {
            //if (_isRoundFinished) return; 
            
            //OnRaySyncOnBalloonFind();
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
        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnBellStage));
    }

    private void OnBellStage()
    {
        _uiManager.PopFromZeroInstructionUI("떨어진 방울장식을 터치해\n트리를 꾸며주세요!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_2_떨어진_방울장식을_터치해_트리를_꾸며주세요_");
        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnBulbStage));
        // _uiManager.PopFromZeroInstructionUI("방울 잘 찾아서 터치해봐!");
        // Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_3_방울_잘_찾아서_터치해봐_");
    }

    private void OnBulbStage()
    {
        _uiManager.PopFromZeroInstructionUI("이번에는 반짝이는 전구장식을 터치해\n트리를 꾸며주세요!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_0_이번에는_반짝이는_전구장식을_터치해__트리를_꾸며주세요_");

        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnCandyStage));
        // _uiManager.PopFromZeroInstructionUI("전구를 잘 찾아서 터치해봐!");
        // Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_5_전구를_잘_찾아서_터치해봐_");
    }

    private void OnCandyStage()
    {
        _uiManager.PopFromZeroInstructionUI("이번에는 맛있는 사탕장식을 터치해\n트리를 꾸며주세요!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_1_이번에는_맛있는_사탕장식을_터치해__트리를_꾸며주세요_");

        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnStarStage));
        // _uiManager.PopFromZeroInstructionUI("사탕장식을 잘 찾아서 터치해봐!");
        // Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_7_사탕장식을_잘_찾아서_터치해봐_");
    }

    private void OnStarStage()
    {
        _uiManager.PopFromZeroInstructionUI("이번에는 반짝이는 별장식을 터치해\n트리를 꾸며주세요!");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_2_이번에는_반짝이는_별장식을_터치해_트리를_꾸며주세요_");
        
        DOVirtual.DelayedCall(4.5f, () => NextStage(MainSeq.OnFinish));
        // _uiManager.PopFromZeroInstructionUI("별장식을 잘 찾아서 터치해봐!");
        // Managers.Sound.Play(SoundManager.Sound.Narration, "EA033/Audio/audio_9_별장식을_잘_찾아서_터치해봐_");

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


}
