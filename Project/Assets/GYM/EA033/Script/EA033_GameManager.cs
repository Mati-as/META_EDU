using System.Collections;
using System.Collections.Generic;
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
        _uiManager.PopFromZeroInstructionUI("색깔을 알아볼까요?"); //나레이션

    }

    private void OnIntroStage()
    {
        
    }

    private void OnBellStage()
    {
        
    }

    private void OnBulbStage()
    {
        
    }

    private void OnCandyStage()
    {
        
    }

    private void OnStarStage()
    {
        
    }

    private void OnFinishStage()
    {
        
    }


}
