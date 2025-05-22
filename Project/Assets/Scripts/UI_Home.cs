using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UI_Home : UI_PopUp
{    public enum UI
    {
        Btn_SensorSettings,
        Btn_ContentSortedByArea,
        Btn_ContentSortedByTheme
    }

    public override bool InitEssentialUI()
    {
        BindObject(typeof(UI));

 
        GetObject((int)UI.Btn_ContentSortedByArea).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_ContentSortedByArea>();
        });
        
        //현재 미사용, 추후구현 예정 250501 민석
        GetObject((int)UI.Btn_ContentSortedByTheme).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_SemesterSelection>();
        });
        GetObject((int)UI.Btn_SensorSettings).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_SensorSettingMain>();
        });
        
        return true;
    }
}
