using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;


// <summary>
/// 영역별 콘텐츠 찾기 ---------------------------------------------
/// </summary>
public class UI_ContentSortedByArea : UI_PopUp
{  
    
    public override bool IsBackBtnClickable => true;    
    public enum UI
    {
        Btn_PA,
        Btn_Art,
        Btn_Communication,
        Btn_Science,
        Btn_Social,
    }

    public override bool InitOnLoad()
    {
        BindObject(typeof(UI));
        
        GetObject((int)UI.Btn_PA).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_PA_ContentSelection>();
        });
        GetObject((int)UI.Btn_Art).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_Art_ContentSelection>();
        });
        GetObject((int)UI.Btn_Communication).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_Communication_ContentSelection>();
        });
        GetObject((int)UI.Btn_Science).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_Science_ContentSelection>();
        });
        GetObject((int)UI.Btn_Social).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_Social_ContentSelection>();
        });
        return base.InitOnLoad();
    }
}
