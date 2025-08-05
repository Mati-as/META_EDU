using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EA039_InGameUIManager : Base_InGameUIManager
{
    private enum UI
    {
        Image_Miscellaneous,
        OnRoundUI
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
    
    public void ActivateImageAndUpdateCount(int ImageToActivate,int count)
    {
         
        GetObject((int)UI.OnRoundUI).SetActive(true);

        GetObject(ImageToActivate).SetActive(true);
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
