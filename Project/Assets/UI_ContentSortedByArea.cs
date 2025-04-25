using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;


// <summary>
/// 영역별 콘텐츠 찾기 ---------------------------------------------
/// </summary>
public class UI_ContentSortedByArea : UI_PopUp
{  
    public enum UI
    {
        Btn_PA,
        Btn_Art,
        Btn_Communication,
        Btn_Science,
        Btn_Social,
    }

    public override bool Init()
    {
        BindObject(typeof(UI));
        
        GetObject((int)UI.Btn_PA).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_ContentSelection_PA>();
        });
        GetObject((int)UI.Btn_Art).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_ContentSelection_Art>();
        });
        GetObject((int)UI.Btn_Communication).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_ContentSelection_Communication>();
        });
        GetObject((int)UI.Btn_Science).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_ContentSelection_Science>();
        });
        GetObject((int)UI.Btn_Social).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_ContentSelection_Social>();
        });
        return base.Init();
    }
}
