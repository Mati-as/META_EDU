
using UnityEngine;

namespace MyCustomizedEditor.Common.Util
{
    public class UI_PopUp : UI_Base
    {

        [SerializeField]
        protected bool isBackBtnClickable = false;
        public bool IsBackBtnClickable
        {
            get
            {
                return isBackBtnClickable;
            }
            set
            {
                isBackBtnClickable = value;
            }
        }
        
        
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
