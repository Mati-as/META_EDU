using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.UI;

public class UI_ContentGuideImage : UI_PopUp
{
    public static int MonthOfImageToShow;
    public override bool IsBackBtnClickable => true;   
    private enum UI
    {
        ContentInfoImage
    }
    
    Image _contentInfoImage;
    public override bool InitOnLoad()
    {
        base.InitOnLoad();
        BindObject(typeof(UI));
        _contentInfoImage = GetObject((int)UI.ContentInfoImage).GetComponent<Image>();
        _contentInfoImage.sprite = Managers.Resource.Load<Sprite>($"UI/ContentGuide/ContentInfoImage_{MonthOfImageToShow}");
        return true;
    }
}
