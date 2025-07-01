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

       //_defaultColor = TMP_Instruction.colorGradient;
       GetButton((int)Btns.Btn_Next).gameObject.GetComponent<CursorImageController>().DefaultScale = Vector3.one;
       
       return true;
   }

//
//    private VertexGradient _defaultColor;
//    public void ChangeTextColor(Color color)
//    {
//        TMP_Instruction.colorGradient = new VertexGradient(
//            color, color, color, color
//        );
//    }
//     
//    public void ResetTextColor()
//    {
//        TMP_Instruction.colorGradient = _defaultColor;
//    }
//    public void ActivateNextButton(float delay)
//    {
//        _nextBtnSeq?.Kill();
//        _nextBtnSeq = DOTween.Sequence();
//
//      
//        _nextBtnSeq.AppendInterval(delay);
//        _nextBtnSeq.AppendCallback(() =>
//        {
//            GetButton((int)Btns.Btn_Next).transform.localScale = Vector3.zero;
//            GetButton((int)Btns.Btn_Next).gameObject.SetActive(true);
//        });
//        _nextBtnSeq.Append(GetButton((int)Btns.Btn_Next).image.DOFade(1, 0.6f));
//        _nextBtnSeq.Join(GetButton((int)Btns.Btn_Next).transform.DOScale(Vector3.one, 0.5f)
//            .SetEase(Ease.OutBack));
//        
//    
//       
//    }
//
//    private Sequence _nextBtnSeq;
//    public void DeactivateNextButton(float delay=0)
//    {
//        _nextBtnSeq?.Kill();
//        _nextBtnSeq = DOTween.Sequence();
//
//        
//        _nextBtnSeq.AppendInterval(delay);
//     
//     
//        _nextBtnSeq.Append(GetButton((int)Btns.Btn_Next).transform.DOScale(Vector3.one *1.35f, 0.2f)
//            .SetEase(Ease.OutBack));
//
//        _nextBtnSeq.Append(GetButton((int)Btns.Btn_Next).transform.DOScale(Vector3.zero, 0.5f)
//            .SetEase(Ease.OutBack));
//        _nextBtnSeq.Join(GetButton((int)Btns.Btn_Next).image.DOFade(0, 0.5f));
//        _nextBtnSeq.AppendCallback(() =>
//        {
//            GetButton((int)Btns.Btn_Next).gameObject.SetActive(false);
//        });
//    }
//
//
//    public void PopBallonCountUI(string instruction, float duration = 5f, float delay = 0f)
//    {
//        _uiSeq?.Kill();
//        _uiSeq = DOTween.Sequence();
//        _uiSeq.AppendInterval(delay);
// //        Logger.ContentTestLog($"PopInstructionUI :활성화------------ {instruction}");
//
//        GetObject((int)UI.OnRoundUI).SetActive(true);
//
//        TMP_Instruction.text = instruction;
//
//        GetObject((int)UI.OnRoundUI).transform.localScale = Vector3.zero;
//        _uiSeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(DEFAULT_SIZE * 1.2f, 0.6f)
//            .SetEase(Ease.InOutBounce));
//        _uiSeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(DEFAULT_SIZE, 0.15f)
//            .SetEase(Ease.InOutBounce));
//
//        if (duration > 0.5f)
//        {
//            _uiSeq.AppendInterval(duration);
//            _uiSeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(Vector3.zero, 0.15f)
//                .SetEase(Ease.InOutBounce));
//        }
//    }
//
//
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
        
        
        Sequence _ea019UISeq = DOTween.Sequence();
        _ea019UISeq?.Kill();
        GetObject((int)UI.OnRoundUI).transform.localScale = Vector3.zero;
        _ea019UISeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(Vector3.one, 0.15f)
            .SetEase(Ease.InOutBounce));
    }

}
