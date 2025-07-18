using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UI_SeasonSelection : UI_PopUp
{
    public override bool IsBackBtnClickable => true;   
    public enum UI
    {
        Spring,
        Summer,
        Fall,
        Winter,
    }
    // Start is called before the first frame update

    public override bool InitOnLoad()
    {
        Managers.UI.Master.currentSeason = null;
        
        BindObject(typeof(UI));
        GetObject((int)UI.Spring).BindEvent(() =>
        {
            Managers.UI.Master.currentSeason = UI.Spring.ToString();
            Managers.UI.ShowPopupUI<UI_Mar>(path:"UI_Month");
        });
        
        GetObject((int)UI.Summer).BindEvent(() =>
        {
            Managers.UI.Master.currentSeason = UI.Summer.ToString();
            Managers.UI.ShowPopupUI<UI_June>(path:"UI_Month");
        });
        GetObject((int)UI.Fall).BindEvent(() =>
        {
            Managers.UI.Master.currentSeason = UI.Fall.ToString();
            Managers.UI.ShowPopupUI<UI_Sep>(path:"UI_Month");
        });
        
        GetObject((int)UI.Winter).BindEvent(() =>
        {
            Managers.UI.Master.currentSeason = UI.Winter.ToString();
            Managers.UI.ShowPopupUI<UI_Dec>(path:"UI_Month");
        });
        return base.InitOnLoad();
    }
}
