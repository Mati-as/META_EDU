

using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
public class UI_SecondSemester_ContentSortedByTheme : UI_PopUp
{
    public enum Btns
    {
      
        Btn_ChuseokAndVehicles,
        Btn_Fall
    }

    public override bool InitEssentialUI()
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
        return base.InitEssentialUI();
    }
}