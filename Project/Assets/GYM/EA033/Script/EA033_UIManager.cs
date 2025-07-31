using DG.Tweening;
using TMPro;
using UnityEngine;


public class EA033_UIManager : Base_UIManager
{
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
   
   private TextMeshProUGUI _tmp;
   


   public override void ExplicitInitInGame()
   {
       base.ExplicitInitInGame();
       //2. InGamePart UI Init()
       BindObject(typeof(UI));
       BindTMP(typeof(TMPs));
       GetObject((int)UI.OnRoundUI).SetActive(false);
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
       GetTMP((int)TMPs.TMP_OnRound).text = $"x {24 - count}";
        
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
