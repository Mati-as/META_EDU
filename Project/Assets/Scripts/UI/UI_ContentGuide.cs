using MyCustomizedEditor.Common.Util;

public class UI_ContentGuide : UI_PopUp
{
    public override bool IsBackBtnClickable
    {
        get
        {
            return true;
        }
    }

    private enum Btns
    {
        Btn_CtInfo1 = 0,
        Btn_CtInfo2,
        Btn_CtInfo3,
        Btn_CtInfo4,
        Btn_CtInfo5,
        Btn_CtInfo6,
        Btn_CtInfo7,
        Btn_CtInfo8,
        Btn_CtInfo9,
        Btn_CtInfo10,
        Btn_CtInfo11,
        Btn_CtInfo12,
        Max
    }

    public override bool InitOnLoad()
    {
        base.InitOnLoad();

        UI_ContentGuideImage.MonthOfImageToShow = 0;
        BindObject(typeof(Btns));

        for (int i = (int)Btns.Btn_CtInfo1; i < (int)Btns.Max; i++)
            GetObject(i).BindEvent(() =>
            {
                UI_ContentGuideImage.MonthOfImageToShow = i +1;
                Managers.UI.ShowPopupUI<UI_ContentGuideImage>();
            });


        return true;
    }
}