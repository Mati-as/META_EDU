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
        foreach (Transform child in GetObject((int)UI.EA038).transform)
        {
            child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(ease: Ease.InOutQuint)
                .OnComplete(() => child.gameObject.SetActive(false));
        }
        DOVirtual.DelayedCall(1f, () => gameManager.ChangeStage(MainSeq.CardGameStageSequence));
    }
 
    public void OnSelectAge4()
    {
        gameManager.gamePlayAge = 4;
        foreach (Transform child in GetObject((int)UI.EA038).transform)
        {
            child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(ease: Ease.InOutQuint)
                .OnComplete(() => child.gameObject.SetActive(false));
        }
        DOVirtual.DelayedCall(1f, () => gameManager.ChangeStage(MainSeq.CardGameStageSequence));
    }
    
    public void OnSelectAge5()
    {
        gameManager.gamePlayAge = 5;
        foreach (Transform child in GetObject((int)UI.EA038).transform)
        {
            child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(ease: Ease.InOutQuint)
                .OnComplete(() => child.gameObject.SetActive(false));
        }
        DOVirtual.DelayedCall(1f, () => gameManager.ChangeStage(MainSeq.CardGameStageSequence));
    }
    
    
   
}