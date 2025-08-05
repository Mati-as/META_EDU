using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SS010_UIManager : Base_UIManager
{
    private enum UI
    {
        

    }

    private enum Btn
    {
        
    }

    private enum Tmp
    {
        
    }


    public override void ExplicitInitInGame()
    {
        base.ExplicitInitInGame();

        BindObject(typeof(UI));
        BindButton(typeof(Btn));
        BindTMP(typeof(Tmp));

        
        
        
        Logger.CoreClassLog("EA038_UIManager Init ---------------");
    }
    
    
    
    
    
    
}
