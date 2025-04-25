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

    public override bool Init()
    {
        BindObject(typeof(UI));

 
        GetObject((int)UI.Btn_ContentSortedByArea).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_ContentSortedByArea>();
        });
        GetObject((int)UI.Btn_ContentSortedByTheme).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_ContentSortedByTheme>();
        });
        GetObject((int)UI.Btn_SensorSettings).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_SensorSettingMain>();
        });
        
        return true;
    }
}
