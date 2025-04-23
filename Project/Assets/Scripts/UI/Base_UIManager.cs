using System;
using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using Unity.VisualScripting;
using UnityEngine;

public class Base_UIManager : UI_PopUp
{
  
    protected enum UI
    {
        
    }

    protected enum TMPs
    {
        TMP_Instruction
    }
    
    
    
    public override bool Init()
    {
        //base.Init();

        BindText(typeof(TMPs));
        GetText((int)TMPs.TMP_Instruction).text = string.Empty;
        return true;
        // Initialize UI elements here
    }
}
