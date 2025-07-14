using System;
using DG.Tweening;
using TMPro;
using UnityEngine;


public class EA033_UIManager : Base_UIManager
{
   private enum Btns
   {
       Btn_Next
   }
   
   public enum UI
   {
       OnRoundUI,
       
       Image_Bell,
       Image_Bulb,
       Image_Candy,
       Image_Star
       
   }

    private enum TMPs
    {
        TMP_OnRound
    }
   
   public static event Action OnNextButtonClicked;
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
           Managers.Sound.Play(SoundManager.Sound.Effect, "EA033/Audio/OnNextBtn");
           DOVirtual.DelayedCall(3f, () =>
           {
               _isInvokable = true;
           });
           OnNextButtonClicked?.Invoke();
       });
       
       GetObject((int)UI.OnRoundUI).SetActive(false);

       GetButton((int)Btns.Btn_Next).gameObject.GetComponent<CursorImageController>().DefaultScale = Vector3.one;
       
       return true;
   }
   
   
   public void ActivateImageAndUpdateCount(int ImageToActivate,int count)
   {
       GetObject((int)UI.OnRoundUI).transform.localScale = Vector3.zero;
        
       GetObject((int)UI.OnRoundUI).SetActive(true);
       GetObject((int)UI.Image_Bell).SetActive(false);
       GetObject((int)UI.Image_Bulb).SetActive(false);
       GetObject((int)UI.Image_Candy).SetActive(false);
       GetObject((int)UI.Image_Star).SetActive(false);
        
       GetObject(ImageToActivate).SetActive(true);
       GetTMP((int)TMPs.TMP_OnRound).text = $"x {20 - count}";
        
       var seq = DOTween.Sequence();
       seq?.Kill();
       seq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(Vector3.one, 0.15f)
           .SetEase(Ease.InOutBounce));
   }

   public void DeactivateImageAndUpdateCount()
   {
       var seq = DOTween.Sequence();
       seq?.Kill();
       seq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(Vector3.zero, 0.15f).From(Vector3.one)
           .SetEase(Ease.InOutBounce).OnComplete(() =>
           {
               GetObject((int)UI.OnRoundUI).SetActive(false);
               GetObject((int)UI.Image_Bell).SetActive(false);
               GetObject((int)UI.Image_Bulb).SetActive(false);
               GetObject((int)UI.Image_Candy).SetActive(false);
               GetObject((int)UI.Image_Star).SetActive(false);
           }));
        
   }
   
}
