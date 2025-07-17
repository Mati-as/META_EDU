using MyCustomizedEditor.Common.Util;

public class UI_TopNavigator : UI_PopUp
{
    private enum Btns
    {
        Btn_AE,
        Btn_PA,
        Btn_CM,
        Btn_SE,
        Btn_SS
    }
    
    

    public override bool InitOnLoad()
    {
        BindButton(typeof(Btns));


        GetButton((int)Btns.Btn_AE).onClick.AddListener(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_Art_ContentSelection>();
        });

        GetButton((int)Btns.Btn_PA).onClick.AddListener(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_PA_ContentSelection>();
        });

        GetButton((int)Btns.Btn_CM).onClick.AddListener(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_Communication_ContentSelection>();
        });

        GetButton((int)Btns.Btn_SE).onClick.AddListener(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_Science_ContentSelection>();
        });

        GetButton((int)Btns.Btn_SS).onClick.AddListener(() =>
        {
            Managers.UI.ClosePopupUI();
            Managers.UI.ShowPopupUI<UI_Social_ContentSelection>();
        });


        return base.InitOnLoad();
    }
}