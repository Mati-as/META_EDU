using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;


public class EA019_UIManager : Base_UIManager
{
   private enum Btns
   {
       Btn_Next
   }


   private enum UI
   {
       OnRoundUI,
       
       Image_RedHeart,
       Image_OrangeTriangle,
       Image_YellowStar,
       Image_GreenCircle,
       Image_BlueSquare,
       Image_PinkFlower,
       
   }

    private enum TMPs
    {
        TMP_OnRound
    }
   
   public static event Action onNextButtonClicked;
   private bool _isInvokable = true;
   private TextMeshProUGUI _tmp;
   
   public override bool InitEssentialUI()
   {
       base.InitEssentialUI();
       
       BindButton(typeof(Btns));
       BindObject(typeof(UI));
         BindTMP(typeof(TMPs));
       
       GetButton((int)Btns.Btn_Next).image.DOFade(0, 0.00001f);
       _tmp = GetButton((int)Btns.Btn_Next).GetComponentInChildren<TextMeshProUGUI>();
       GetButton((int)Btns.Btn_Next).gameObject.SetActive(false);
       GetButton((int)Btns.Btn_Next).gameObject.BindEvent(() =>
       {
           if (!_isInvokable) return;
           _isInvokable = false;
           Managers.Sound.Play(SoundManager.Sound.Effect, "EA019/OnNextBtn");
           DOVirtual.DelayedCall(3f, () =>
           {
               _isInvokable = true;
           });
           onNextButtonClicked?.Invoke();
       });
       
       GetObject((int)UI.OnRoundUI).SetActive(false);
       
       return true;
   }
    
    
   public void ActivateNextButton(float delay)
   {
       DOVirtual.DelayedCall(delay, () =>
       {
           GetButton((int)Btns.Btn_Next).gameObject.SetActive(true);
           GetButton((int)Btns.Btn_Next).image.DOFade(1, 0.5f);
           _tmp.DOFade(1, 0.5f);
       });
      
   }
   public void DeactivateNextButton(float delay=0)
   {
       DOVirtual.DelayedCall(delay, () =>
       {
           GetButton((int)Btns.Btn_Next).image.DOFade(0, 0.5f)
               .OnComplete(() =>
               {
                   GetButton((int)Btns.Btn_Next).gameObject.SetActive(false);
                   _tmp.DOFade(0, 0.5f);
               });
       });
   }


   public void PopBallonCountUI(string instruction, float duration = 5f, float delay = 0f)
   {
       _uiSeq?.Kill();
       _uiSeq = DOTween.Sequence();
       _uiSeq.AppendInterval(delay);
//        Logger.ContentTestLog($"PopInstructionUI :활성화------------ {instruction}");

       GetObject((int)UI.OnRoundUI).SetActive(true);

       TMP_Instruction.text = instruction;

       GetObject((int)UI.OnRoundUI).transform.localScale = Vector3.zero;
       _uiSeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(DEFAULT_SIZE * 1.2f, 0.6f)
           .SetEase(Ease.InOutBounce));
       _uiSeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(DEFAULT_SIZE, 0.15f)
           .SetEase(Ease.InOutBounce));

       if (duration > 0.5f)
       {
           _uiSeq.AppendInterval(duration);
           _uiSeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(Vector3.zero, 0.15f)
               .SetEase(Ease.InOutBounce));
       }
   }


   public void ActivateImageAndUpdateCount(int ImageToActivate,int count)
   {
         
       GetObject((int)UI.OnRoundUI).SetActive(true);
       
       GetObject((int)UI.Image_RedHeart).SetActive(false);
       GetObject((int)UI.Image_OrangeTriangle).SetActive(false);
       GetObject((int)UI.Image_YellowStar).SetActive(false);
       GetObject((int)UI.Image_GreenCircle).SetActive(false);
       GetObject((int)UI.Image_BlueSquare).SetActive(false);
       GetObject((int)UI.Image_PinkFlower).SetActive(false);
       
       GetObject(ImageToActivate).SetActive(true);
       GetTMP((int)TMPs.TMP_OnRound).text = $"x {count}";
       
       
       _ea019UISeq?.Kill();
       _ea019UISeq = DOTween.Sequence();
       GetObject((int)UI.OnRoundUI).transform.localScale = Vector3.zero;
       _ea019UISeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(Vector3.one, 0.15f)
           .SetEase(Ease.InOutBounce));
   }

   private Sequence _ea019UISeq;
   public void DeactivateRoundScoreBoard()
   {
       _ea019UISeq?.Kill();
       _ea019UISeq = DOTween.Sequence();
       
       _ea019UISeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(Vector3.zero, 0.15f)
           .SetEase(Ease.InOutBounce));
   }
}
