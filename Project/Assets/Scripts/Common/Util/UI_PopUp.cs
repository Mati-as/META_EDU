using UnityEngine.SceneManagement;

namespace MyCustomizedEditor.Common.Util
{
    public class UI_PopUp : UI_Base
    {
        public override bool InitEssentialUI()
        {
            if (base.InitEssentialUI() == false)
                return false;

            // if (SceneManager.GetActiveScene().name.Contains("METAEDU"))
            // {
            //     Logger.CoreClassLog("Popup UIs on Launcher Canvas Set");
            //     Managers.UI.SetCanvas(gameObject, true);
            // }
            return true;
        }

        public virtual void ClosePopupUI()
        {
            Managers.UI.ClosePopupUI(this);
        }
    }
}