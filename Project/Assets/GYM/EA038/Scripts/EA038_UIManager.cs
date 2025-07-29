using DG.Tweening;
using UnityEngine;


public class EA038_UIManager : Base_UIManager
{
    private enum UI
    {
        EA038,
       
    }

    private enum TMPs
    {
        TMP_OnRound
    }
   
    //public static event Action OnNextButtonClicked;
    //private bool _isInvokable = true;
    //private TextMeshProUGUI _tmp;

    private EA038_GameManager gameManager;
    
    public override void ExplicitInitInGame()
    {
        base.ExplicitInitInGame();

        gameManager = FindAnyObjectByType<EA038_GameManager>();
        
        BindObject(typeof(UI));

        foreach (Transform child in GetObject((int)UI.EA038).transform) //버튼 초기화
            child.gameObject.SetActive(false);

        Logger.CoreClassLog("EA038_UIManager Init ---------------");
    }

    public void ShowSelectAgeBtn()
    {
        foreach (Transform child in GetObject((int)UI.EA038).transform)
        {
            child.gameObject.transform.DOScale(Vector3.one, 1f).SetEase(ease: Ease.OutBack)
                .From(Vector3.zero)
                .OnStart(() => child.gameObject.SetActive(true));
        }
    }
    
    
    public void OnSelectAge3()
    {
        gameManager.gamePlayAge = 3;
        var Btn3 = GetObject((int)UI.EA038).transform.GetChild(0).transform;
        Btn3.DOLocalMove(Vector2.zero, 1f).SetEase(Ease.OutBack).OnComplete(() => Btn3.DOShakePosition(0.2f, 40f));
        Btn3.DOScale(Vector3.one * 2f, 1f);

        DOVirtual.DelayedCall(2f, () =>
        {
            foreach (Transform child in GetObject((int)UI.EA038).transform)
            {
                child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(ease: Ease.InOutQuint)
                    .OnComplete(() => child.gameObject.SetActive(false));
            }
        });
        
        DOVirtual.DelayedCall(4f, () => gameManager.ChangeStage(EA038_MainSeq.CardGameStageSequence));
    }
 
    public void OnSelectAge4()
    {
        gameManager.gamePlayAge = 4;
        
        var Btn4 = GetObject((int)UI.EA038).transform.GetChild(1).transform;
        Btn4.DOLocalMove(Vector2.zero, 1f).SetEase(Ease.OutBack).OnComplete(() => Btn4.DOShakePosition(0.2f, 40f));
        Btn4.DOScale(Vector3.one * 2f, 1f);

        DOVirtual.DelayedCall(2f, () =>
        {
            foreach (Transform child in GetObject((int)UI.EA038).transform)
            {
                child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(ease: Ease.InOutQuint)
                    .OnComplete(() => child.gameObject.SetActive(false));
            }
        });
            
        DOVirtual.DelayedCall(4f, () => gameManager.ChangeStage(EA038_MainSeq.CardGameStageSequence));
    }
    
    public void OnSelectAge5()
    {
        gameManager.gamePlayAge = 5;
        
        var Btn5 = GetObject((int)UI.EA038).transform.GetChild(2).transform;
        Btn5.DOLocalMove(Vector2.zero, 1f).SetEase(Ease.OutBack).OnComplete(() => Btn5.DOShakePosition(0.2f, 40f));
        Btn5.DOScale(Vector3.one * 2f, 1f);

        DOVirtual.DelayedCall(2f, () =>
        {
            foreach (Transform child in GetObject((int)UI.EA038).transform)
            {
                child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(ease: Ease.InOutQuint)
                    .OnComplete(() => child.gameObject.SetActive(false));
            }
        });
        
        DOVirtual.DelayedCall(4f, () => gameManager.ChangeStage(EA038_MainSeq.CardGameStageSequence));
    }
    
    
   
}