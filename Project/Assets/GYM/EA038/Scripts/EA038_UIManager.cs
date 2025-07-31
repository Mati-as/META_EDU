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
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                gameManager.gamePlayAge = 3;
                var Btn3 = GetObject((int)UI.EA038).transform.GetChild(0).transform;
                Btn3.DOLocalMove(Vector2.zero, 1f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_17_3살");
                    PopAndChangeUI("3살!", 4f);
                    
                    Btn3.DOShakePosition(0.2f, 40f);
                });
                Btn3.DOScale(Vector3.one * 2f, 1f);
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                foreach (Transform child in GetObject((int)UI.EA038).transform)
                {
                    child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(ease: Ease.InOutQuint)
                        .OnComplete(() => child.gameObject.SetActive(false));
                }
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_3_형님이_되어_3살이_되었어요_");
                PopAndChangeUI("형님이 되어 3살이 되었어요!", 3f);
                
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_6_이제부터_나이와_관련된_놀이를_시작할거에요_");
                PopAndChangeUI("이제부터 나이와 관련된 놀이를\n시작할거에요!", 4f);
                
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                gameManager.ChangeStage(EA038_MainSeq.CardGameStageSequence);
            })
            ;

    }
 
    public void OnSelectAge4()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                gameManager.gamePlayAge = 4;
                var Btn3 = GetObject((int)UI.EA038).transform.GetChild(1).transform;
                Btn3.DOLocalMove(Vector2.zero, 1f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_18_4살");
                    PopAndChangeUI("4살!", 4f);
                    
                    Btn3.DOShakePosition(0.2f, 40f);
                });
                Btn3.DOScale(Vector3.one * 2f, 1f);
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                foreach (Transform child in GetObject((int)UI.EA038).transform)
                {
                    child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(ease: Ease.InOutQuint)
                        .OnComplete(() => child.gameObject.SetActive(false));
                }
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_4_형님이_되어_4살이_되었어요_");
                PopAndChangeUI("형님이 되어 4살이 되었어요!", 3f);
                
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_6_이제부터_나이와_관련된_놀이를_시작할거에요_");
                PopAndChangeUI("이제부터 나이와 관련된 놀이를\n시작할거에요!", 4f);
                
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                gameManager.ChangeStage(EA038_MainSeq.CardGameStageSequence);
            })
            ;
    }
    
    public void OnSelectAge5()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                gameManager.gamePlayAge = 5;
                var Btn3 = GetObject((int)UI.EA038).transform.GetChild(2).transform;
                Btn3.DOLocalMove(Vector2.zero, 1f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_19_5살");
                    PopAndChangeUI("5살!", 4f);
                    
                    Btn3.DOShakePosition(0.2f, 40f);
                });
                Btn3.DOScale(Vector3.one * 2f, 1f);
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                foreach (Transform child in GetObject((int)UI.EA038).transform)
                {
                    child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(ease: Ease.InOutQuint)
                        .OnComplete(() => child.gameObject.SetActive(false));
                }
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_5_형님이_되어_5살이_되었어요_");
                PopAndChangeUI("형님이 되어 5살이 되었어요!", 3f);
                
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_6_이제부터_나이와_관련된_놀이를_시작할거에요_");
                PopAndChangeUI("이제부터 나이와 관련된 놀이를\n시작할거에요!", 4f);
                
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                gameManager.ChangeStage(EA038_MainSeq.CardGameStageSequence);
            })
            ;
    }
    
    
   
}