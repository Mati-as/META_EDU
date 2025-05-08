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
    }


    public override bool InitEssentialUI()
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
        return base.InitEssentialUI();
    }
}
