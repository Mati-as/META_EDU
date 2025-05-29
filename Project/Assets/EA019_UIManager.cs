using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EA019_UIManager : Base_UIManager
{
   private enum Btns
   {
       Btn_Next
   }

   private enum UI
   {
       
   }

   public override bool InitEssentialUI()
   {
       BindButton(typeof(Btns));
       
       GetButton((int)Btns.Btn_Next).gameObject.SetActive(false);
       return base.InitEssentialUI();
   }
}
