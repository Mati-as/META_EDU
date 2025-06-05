using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UI_SemesterSelection : UI_PopUp
{
    
    public enum UI
    {
        SemesterA,
        SemesterB
    }
    // Start is called before the first frame update

    public override bool InitEssentialUI()
    {
        BindObject(typeof(UI));
        
        GetObject((int)UI.SemesterA).BindEvent(() =>
        {
            Managers.UI.ShowPopupUI<UI_FirstSemester_ContentSortedByTheme>();
        });
        
        GetObject((int)UI.SemesterB).BindEvent(() =>
        {
            
        });
        return base.InitEssentialUI();
    }
}
