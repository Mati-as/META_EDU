using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UI_SensorSettingMain : UI_PopUp
{
    public enum UI
    {
        Btn_ScreenSetting,
        Btn_SensorSetting,
        Btn_MenuSetting,
    }


    public override bool InitOnLoad()
    {
        BindObject(typeof(UI));
        
  
        GetObject((int)UI.Btn_ScreenSetting).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_ScreenSetting>();
        });
        
        GetObject((int)UI.Btn_SensorSetting).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_SensorSetting>();
        });

        GetObject((int)UI.Btn_MenuSetting).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_MenuSetting>();
        });
        return base.InitOnLoad();
    }
}
