using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EA035_WinterFood_GameManager : Ex_BaseGameManager
{

   private enum MainSeq
    {
        Default,
        Intro,
        Fish_Intro,
        Fish_Flour,
        Fish_Bean,
        
        
      
    }


    public enum Objs
    {
        SeatSelection,
        Buttons,
        CakeA,
        CakeB,
        CakeC,
        CakeCream_A,
        CakeCream_B,
        CakeCream_C,
        CandySetRoot,
        OriginPosParent,
        TargetPos,
        CandleStartPos,
        Candles
    }

    private SeatSelectionController _seatSelectionController;
    private ButtonClickEventController _buttonClickEventController;

    public int CurrentMainMainSeq
    {
        get
        {
            return CurrentMainMainSequence;
        }
        set
        {
            CurrentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {((MainSeq)CurrentMainMainSeq).ToString()}");

            ChangeThemeSeqAnim(value);
            switch (value)
            {
                case (int)MainSeq.Default:
                    break;

                case (int)MainSeq.Intro:
                 
                
                    break;
                //
                // case (int)MainSeq.OnCream:
                //   
                //   
                //
                //
                //     break;
                // case (int)MainSeq.OnDecorate:
                //
                //  
                //     break;
                //
                // case (int)MainSeq.OnCandle:
                //   
                //  
                //
                //     break;
                //
                // case (int)MainSeq.OnCelebrate:
                //     break;
                //
                // case (int)MainSeq.OnBlowOutCandle:
                //     break;
                //
                //   
                // case (int)MainSeq.OnFinish:
                //     break;
            }
        }
    }
}
