using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EA039_InGameUIManager : Base_InGameUIManager
{
    public enum UI
    {
        Image_Miscellaneous,
        Image_Shoes,
        
        Image_Red,
        Image_Orange,
        Image_Yellow,
        Image_Green,
        Image_Blue,
        Image_Purple,
        
        OnRoundUI,
        Max
        
    }
    private Sequence _ea039UISeq;

    private enum TMPs
    {
        TMP_OnRound
    }
    public override void ExplicitInitInGame()
    {
        base.ExplicitInitInGame();
        
        BindObject(typeof(UI));
        BindTMP(typeof(TMPs));

        
        GetObject((int)UI.OnRoundUI).SetActive(false);
    }
    
    public void ActivateImageAndUpdateCount(int imageToActivate,int count)
    {
        for (int i = 0; i < (int)UI.Max; i++) GetObject(i).SetActive(false);

       GetObject(imageToActivate).SetActive(true);

        
        GetObject((int)UI.OnRoundUI).SetActive(true);

        GetObject(imageToActivate).SetActive(true);
        GetTMP((int)TMPs.TMP_OnRound).text = $"x {count}";


        _ea039UISeq?.Kill();
        _ea039UISeq = DOTween.Sequence();
        GetObject((int)UI.OnRoundUI).transform.localScale = Vector3.zero;
        _ea039UISeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(Vector3.one, 0.15f)
            .SetEase(Ease.InOutBounce));
    }

    public void DeactivateRoundScoreBoard()
    {
        _ea039UISeq?.Kill();
        _ea039UISeq = DOTween.Sequence();

        _ea039UISeq.Append(GetObject((int)UI.OnRoundUI).transform.DOScale(Vector3.zero, 0.15f)
            .SetEase(Ease.InOutBounce));
    }
}
