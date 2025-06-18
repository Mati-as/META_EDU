

using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
public class UI_SecondSemester_ContentSortedByTheme : UI_PopUp
{
    public enum Btns
    {
        Btn_ChuseokAndVehicles,
    }

    public override bool InitEssentialUI()
    {
        BindButton(typeof(Btns));
        GetButton((int)Btns.Btn_ChuseokAndVehicles).gameObject.BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_ChuseokAndVehicles_ContentSortedByTheme>();
        });
        return base.InitEssentialUI();
    }
}