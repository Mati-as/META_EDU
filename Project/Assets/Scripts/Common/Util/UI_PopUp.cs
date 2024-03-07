namespace MyCustomizedEditor.Common.Util
{
    public class UI_PopUp : UI_Base
    {
        public override bool Init()
        {
            if (base.Init() == false)
                return false;

            Managers.UI.SetCanvas(gameObject, true);
            return true;
        }

        // public virtual void ClosePopupUI()
        // {
        //     Managers.UI.ClosePopupUI(this);
        // }
    }
}