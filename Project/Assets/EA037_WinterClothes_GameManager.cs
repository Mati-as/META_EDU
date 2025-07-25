using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EA037_WinterClothes_GameManager : Ex_BaseGameManager
{
    
    private enum MainSeq
    {
        Default,
        Main_Into,
        Top,
        Botton,
        Outwear,
        Gloves,
        Main_Outro,
        OnFinish
    }

    private enum Objs
    {
        Avatar_Girl,
        Avatar_Boy,
        Avatars,
        
        ButtonController
    }

    private enum Clothes
    {
        Top,
        Outwear,
        Pants,
        Gloves,
        
    }

    #region 이펙트 관리 

    #endregion

    private Dictionary<int, Sprite[]> _clothesSpritesMap = new();
    private Dictionary<int, Dictionary<int,GameObject>> _clothesOnAvatarMap = new();
    
    
    private ButtonClickEventController _buttonClickEventController;
    private AvatarAnimationController _avatarAnimationController;

    protected override void Init()
    {
        base.Init();
        BindObject(typeof(Objs));
     
        _buttonClickEventController = GetObject((int)Objs.ButtonController).GetComponent<ButtonClickEventController>();
        _avatarAnimationController = GetObject((int)Objs.Avatars).GetComponent<AvatarAnimationController>();
        
        _clothesOnAvatarMap.Add((int)Objs.Avatar_Girl, new Dictionary<int, GameObject>());
        _clothesOnAvatarMap.Add((int)Objs.Avatar_Boy,new Dictionary<int, GameObject>());

        for (int i = 0; i < Enum.GetValues(typeof(Clothes)).Length; i++)
        {
            _clothesOnAvatarMap[(int)Objs.Avatar_Girl].Add(i,GetObject((int)Objs.Avatar_Girl).transform.GetChild(i).gameObject);
            _clothesOnAvatarMap[(int)Objs.Avatar_Boy].Add(i,GetObject((int)Objs.Avatar_Boy).transform.GetChild(i).gameObject);
        }
        
        InitClothes();
        
        
        _buttonClickEventController.OnButtonClicked -= OnBtnClicked;
        _buttonClickEventController.OnButtonClicked += OnBtnClicked;
        
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _buttonClickEventController.OnButtonClicked -= OnBtnClicked;
    }

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
            ChangeThemeSeqAnim(value);
            switch (value)
            {
             
                case (int)MainSeq.Default:
                break;
                
                case (int)MainSeq.Main_Into:
                    baseUIManager.PopInstructionUIFromScaleZero("“친구들에게 따뜻한 옷을 입혀주세요!");
                    _buttonClickEventController.StartBtnClickAnyOrder();
                break;
                
                case (int)MainSeq.Top:
                    baseUIManager.PopInstructionUIFromScaleZero("따뜻한 상의를 터치해주세요!");
                break;
                
                case (int)MainSeq.Botton:
                    baseUIManager.PopInstructionUIFromScaleZero("따뜻한 바지를 터치해주세요!");
                break;
                
                case (int)MainSeq.Outwear:
                    baseUIManager.PopInstructionUIFromScaleZero("따뜻한 외투를 터치해주세요!");
                break;
                
                case (int)MainSeq.Gloves:
                    baseUIManager.PopInstructionUIFromScaleZero("장갑을 터치해주세요!");
                break;
                
                case (int)MainSeq.Main_Outro:
                break;
                case (int)MainSeq.OnFinish:
                    baseUIManager.PopInstructionUIFromScaleZero("겨울은 따뜻하게 옷을입어요!");
                break;
            }
        }
    }
    
#if UNITY_EDITOR
    [SerializeField] private MainSeq SEQ_ON_START_BTN;
#else
    MainSeq SEQ_ON_START_BTN = MainSeq.Main_Into;
#endif

    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        
        CurrentMainMainSeq = (int)SEQ_ON_START_BTN;
        _buttonClickEventController.StartBtnOnTimeClickMode();
    }

    public override void OnRaySynced()
    {
        base.OnRaySynced();
    }


    protected override void OnBtnClickEvent(int btnId)
    {
        base.OnBtnClickEvent(btnId);
  
        switch (currentMainMainSequence)
        {
            case (int)MainSeq.Default:
                break;
            case (int)MainSeq.Main_Into:
                baseUIManager.PopInstructionUIFromScaleZero("창밖에 눈이 내리고 있어요~ 날씨가 너무 추워요~");
                baseUIManager.PopInstructionUIFromScaleZero("“친구들에게 따뜻한 옷을 입혀주세요!");
                break;
            case (int)MainSeq.Top:
                baseUIManager.PopInstructionUIFromScaleZero("따뜻한 상의를 터치해주세요!");
                break;
            case (int)MainSeq.Botton:
                baseUIManager.PopInstructionUIFromScaleZero("따뜻한 바지를 터치해주세요!");
                break;
            case (int)MainSeq.Outwear:
                baseUIManager.PopInstructionUIFromScaleZero("따뜻한 외투를 터치해주세요!");
                break;
            case (int)MainSeq.Gloves:
                baseUIManager.PopInstructionUIFromScaleZero("장갑을 터치해주세요!");
                break;
            case (int)MainSeq.Main_Outro:
                break;
            case (int)MainSeq.OnFinish:
                baseUIManager.PopInstructionUIFromScaleZero("겨울은 따뜻하게 옷을입어요!");
                break;
        }
    }

    #region 아바타 옷입히기 관리

    private void InitClothes()
    {
        foreach (var key in _clothesOnAvatarMap[(int)Objs.Avatar_Girl].Keys.ToArray())
        {
            _clothesOnAvatarMap[(int)Objs.Avatar_Girl][key].SetActive(false);
        }
        
        foreach (var key in _clothesOnAvatarMap[(int)Objs.Avatar_Boy].Keys.ToArray())
        {
            _clothesOnAvatarMap[(int)Objs.Avatar_Boy][key].SetActive(false);
        }
    }

    private void ChangeClothes(int clothIndex)
    {
        
    }

    #endregion

    private void OnBtnClicked(int index)
    {
        
    } 

    #region 이미지 스프라이트 관리

    private void SetAnswerClothes()
    {
        
    }

    private void OnAnswer()
    {
        
    }

    #endregion


    



}
