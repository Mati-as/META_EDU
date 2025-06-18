using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EA021_GameManager : Ex_BaseGameManager
{
    
    private enum MainSeq
    {
        Default_LateInit,
        OnIntro,
        OnFireAndAlarm,
        CloseMuthAndNose,
        TakeExit
        ,OnEscape,
        OnFinish
        
        
    }
   public int CurrentMainMainSeq
    {
        get => CurrentMainMainSequence;
        set
        {
            CurrentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {((MainSeq)CurrentMainMainSeq).ToString()}");
            // commin Init Part.
          

            
            ChangeThemeSeqAnim(value);
            switch (value)
            {
                
                case (int)MainSeq.Default_LateInit:
                    break;
                    
                
                
                case (int)MainSeq.OnIntro:
                    break;

                case (int)MainSeq.OnFireAndAlarm:
                    break;
                
                
                case (int)MainSeq.CloseMuthAndNose:
                    break;
                
                case (int)MainSeq.TakeExit:
                   
                    break;
                
                case (int)MainSeq.OnEscape:
                   
                    break;

                case (int)MainSeq.OnFinish:
                 
                    break;
            }
        }
    }
   protected sealed override void Init()
   {
       psResourcePath = "Runtime/EA010/FX_leaves";
        
       base.Init();
       
    

       Logger.Log("Init--------------------------------");
   }
 
}
