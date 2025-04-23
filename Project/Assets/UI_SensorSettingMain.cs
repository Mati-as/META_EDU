using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UI_SensorSettingMain : UI_PopUp
{

    public enum UI
    {
        UI_SensorSetting,
        UI_ScreenSetting
    }


    public override bool Init()
    {
        GetObject((int)UI.UI_SensorSetting).BindEvent(() =>
        {
            
        });
        GetObject((int)UI.UI_ScreenSetting).BindEvent(() =>
        {
          
        });
        return base.Init();
    }
}
