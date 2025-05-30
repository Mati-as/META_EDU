using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class EA019_UIManager : Base_UIManager
{
   private enum Btns
   {
       Btn_Next
   }


   
   public static event Action onNextButtonClicked;
   private bool _isInvokable = true;

   public override bool InitEssentialUI()
   {
       base.InitEssentialUI();
       
       BindButton(typeof(Btns));
       
       
       GetButton((int)Btns.Btn_Next).image.DOFade(0, 0.00001f);
       GetButton((int)Btns.Btn_Next).gameObject.SetActive(false);
       GetButton((int)Btns.Btn_Next).gameObject.BindEvent(() =>
       {
           if (!_isInvokable) return;
           _isInvokable = false;
           DeactivateNextButton(0f);
           DOVirtual.DelayedCall(3f, () =>
           {
               _isInvokable = true;
           });
           onNextButtonClicked?.Invoke();
       });
       
       return true;
   }
    
    
   public void ActivateNextButton(float delay)
   {
       DOVirtual.DelayedCall(delay, () =>
       {
           GetButton((int)Btns.Btn_Next).gameObject.SetActive(true);
           GetButton((int)Btns.Btn_Next).image.DOFade(1, 0.5f);
       });
      
   }
   public void DeactivateNextButton(float delay)
   {
       DOVirtual.DelayedCall(delay, () =>
       {
           GetButton((int)Btns.Btn_Next).image.DOFade(0, 0.5f)
               .OnComplete(() =>
               {
                   GetButton((int)Btns.Btn_Next).gameObject.SetActive(false);
               });
       });


   }
   
}
