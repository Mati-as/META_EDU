using DG.Tweening;
using UnityEngine;


public class EA038_UIManager : Base_UIManager
{
    // private enum Btns
    // {
    //     Btn_Next
    // }
   
    private enum UI
    {
        OnRoundUI,
        EndStageUI_BookCase,
        EndStageUI_Table,
        EndStageUI_Chair
       
    }

    private enum TMPs
    {
        TMP_OnRound
    }
   
    //public static event Action OnNextButtonClicked;
    //private bool _isInvokable = true;
    //private TextMeshProUGUI _tmp;

    public override void ExplicitInit()
    {
        base.ExplicitInit();

        BindObject(typeof(UI));
       
        GetObject((int)UI.OnRoundUI).SetActive(true);
        GetObject((int)UI.EndStageUI_BookCase).SetActive(false);
        GetObject((int)UI.EndStageUI_Table).SetActive(false);
        GetObject((int)UI.EndStageUI_Chair).SetActive(false);

        Logger.CoreClassLog("EA038_UIManager Init ---------------");
    }

 

    public void ActivateUIEndStage(int num)
    {
        var seq = DOTween.Sequence();
        seq?.Kill();
        seq.Append(GetObject((int)UI.OnRoundUI + num).transform.DOScale(Vector3.one, 0.15f).From(Vector3.zero)
            .OnStart(() => GetObject((int)UI.OnRoundUI + num).SetActive(true))
            .SetEase(Ease.InOutBounce));
    }
   
    public void TouchUIEndStage(int num)
    {
        var seq = DOTween.Sequence();
        seq?.Kill();
        seq.Append(GetObject((int)UI.OnRoundUI + num).transform.DOShakeScale(0.3f)
            .SetEase(Ease.InOutBounce));
    }
   
}