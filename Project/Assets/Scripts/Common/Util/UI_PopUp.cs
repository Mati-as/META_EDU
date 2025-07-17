
using UnityEngine;

namespace MyCustomizedEditor.Common.Util
{
    public class UI_PopUp : UI_Base
    {

     
        protected bool isBackBtnClickable = false;
        public virtual bool IsBackBtnClickable
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
        
        
        public override bool InitOnLoad()
        {
            if (base.InitOnLoad() == false)
                return false;
            
            return true;
        }

        public virtual void ClosePopupUI()
        {
            Managers.UI.ClosePopupUI(this);
        }
    }
}
