

using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
public class UI_SecondSemester_ContentSortedByTheme : UI_PopUp
{
    public override bool IsBackBtnClickable => true;    
    public enum Btns
    {
      
        Btn_ChuseokAndVehicles,
        Btn_Fall
    }

    public override bool InitOnLoad()
    {
        
        BindButton(typeof(Btns));
        GetButton((int)Btns.Btn_ChuseokAndVehicles).gameObject.BindEvent(() =>
        {
         //   Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_Sep_ContentSelection>();
        });
        
        GetButton((int)Btns.Btn_Fall).gameObject.BindEvent(() =>
        {
           // Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_Oct_ContentSelection>();
        });
        return base.InitOnLoad();
    }
}