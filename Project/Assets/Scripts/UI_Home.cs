using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UI_Home : UI_PopUp
{    public enum UI
    {
        Btn_SensorSettings,
        Btn_ContentSortedByArea,
        Btn_ContentSortedByTheme,
        Btn_ContentGuide,
        
        Btn_ThisMonth,
        Btn_MediaArt,
        Btn_TestBuild
    }

    public override bool InitOnLoad()
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
            Managers.UI.ShowPopupUI<UI_SeasonSelection>();
        });
        
        GetObject((int)UI.Btn_MediaArt).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_MediaArt>();
        });
        
        
        GetObject((int)UI.Btn_ContentGuide).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_ContentGuide>();
        });
        
        
        
        
        GetObject((int)UI.Btn_SensorSettings).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_SensorSettingMain>();
        });
        GetObject((int)UI.Btn_TestBuild).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_TestBuild>();
        });
        
        GetObject((int)UI.Btn_ThisMonth).BindEvent(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_ThisMonth>();
        });
        

        return true;
    }
}
