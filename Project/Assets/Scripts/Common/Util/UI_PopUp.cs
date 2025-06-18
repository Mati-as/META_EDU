
namespace MyCustomizedEditor.Common.Util
{
    public class UI_PopUp : UI_Base
    {
        public override bool InitEssentialUI()
        {
            if (base.InitEssentialUI() == false)
                return false;
            
            return true;
        }

        public virtual void ClosePopupUI()
        {
            Managers.UI.ClosePopupUI(this);
        }
    }
}
