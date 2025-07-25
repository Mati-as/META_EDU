using System.Collections;
using System.Collections.Generic;
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
                break;
                case (int)MainSeq.Top:
                break;
                case (int)MainSeq.Botton:
                break;
                case (int)MainSeq.Outwear:
                break;
                case (int)MainSeq.Gloves:
                break;
                case (int)MainSeq.Main_Outro:
                break;
                case (int)MainSeq.OnFinish:
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
    }

}
