using DG.Tweening;
using UnityEngine;

public class Ea038InGameUIManager : Base_InGameUIManager
{
    private enum UI
    {
        EA038,

    }

    private enum Btns
    {
        Btn_3,
        Btn_4,
        Btn_5,
    }

    private enum Tmps
    {
        Tmp_3,
        Tmp_4,
        Tmp_5,
    }

    private Vector3 centerPos;
    private EA038_GameManager gameManager;

    public override void ExplicitInitInGame()
    {
        base.ExplicitInitInGame();

        gameManager = FindAnyObjectByType<EA038_GameManager>();

        BindObject(typeof(UI));
        BindButton(typeof(Btns));
        BindTMP(typeof(Tmps));

        foreach (Transform child in GetObject((int)UI.EA038).transform) //버튼 초기화
            child.gameObject.SetActive(false);
        
        centerPos = new Vector3(0, -114, 0);

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
    
    public void ShowEndSelectAgeBtn()
    {
        foreach (Transform child in GetObject((int)UI.EA038).transform)
        {
            child.gameObject.transform.DOScale(Vector3.one * 1.6f, 1f).SetEase(ease: Ease.OutBack)
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
                
                for (int i = 0; i < 3; i++)
                {
                    GetButton(i).interactable = false;
                }
                
                var Btn3 = GetObject((int)UI.EA038).transform.GetChild(0).transform;

                GetButton((int)Btns.Btn_4).image.DOFade(0f, 1f).OnComplete(() =>
                    GetObject((int)UI.EA038).transform.GetChild(1).gameObject.SetActive(false));
                ;
                GetTMP((int)Btns.Btn_4).DOFade(0f, 1f);
                GetButton((int)Btns.Btn_5).image.DOFade(0f, 1f).OnComplete(() =>
                    GetObject((int)UI.EA038).transform.GetChild(2).gameObject.SetActive(false));
                ;
                GetTMP((int)Btns.Btn_5).DOFade(0f, 1f);

                Btn3.DOLocalMove(centerPos, 1f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_17_3살");
                    PopInstructionUIFromScaleZero("3살!", 4f);

                    Btn3.DOShakePosition(0.2f, 40f);
                });
                Btn3.DOScale(Vector3.one * 1.6f, 1f);
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
                PopInstructionUIFromScaleZero("형님이 되어 3살이 되었어요!", 3f);

            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_6_이제부터_나이와_관련된_놀이를_시작할거에요_");
                PopInstructionUIFromScaleZero("이제부터 나이와 관련된 놀이를\n시작할거에요!", 4f);

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

                for (int i = 0; i < 3; i++)
                {
                    GetButton(i).interactable = false;
                }
                
                GetButton((int)Btns.Btn_3).image.DOFade(0f, 1f).OnComplete(() =>
                    GetObject((int)UI.EA038).transform.GetChild(0).gameObject.SetActive(false));
                ;
                GetTMP((int)Btns.Btn_3).DOFade(0f, 1f);
                GetButton((int)Btns.Btn_5).image.DOFade(0f, 1f).OnComplete(() =>
                    GetObject((int)UI.EA038).transform.GetChild(2).gameObject.SetActive(false));
                ;
                GetTMP((int)Btns.Btn_5).DOFade(0f, 1f);

                var Btn3 = GetObject((int)UI.EA038).transform.GetChild(1).transform;
                Btn3.DOLocalMove(centerPos, 1f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_18_4살");
                    PopInstructionUIFromScaleZero("4살!", 4f);

                    Btn3.DOShakePosition(0.2f, 40f);
                });
                Btn3.DOScale(Vector3.one * 1.6f, 1f);
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
                PopInstructionUIFromScaleZero("형님이 되어 4살이 되었어요!", 3f);

            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_6_이제부터_나이와_관련된_놀이를_시작할거에요_");
                PopInstructionUIFromScaleZero("이제부터 나이와 관련된 놀이를\n시작할거에요!", 4f);

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
                
                for (int i = 0; i < 3; i++)
                {
                    GetButton(i).interactable = false;
                }

                GetButton((int)Btns.Btn_3).image.DOFade(0f, 1f).OnComplete(() =>
                    GetObject((int)UI.EA038).transform.GetChild(0).gameObject.SetActive(false));
                GetTMP((int)Btns.Btn_3).DOFade(0f, 1f);
                GetButton((int)Btns.Btn_4).image.DOFade(0f, 1f).OnComplete(() =>
                    GetObject((int)UI.EA038).transform.GetChild(1).gameObject.SetActive(false));
                ;
                GetTMP((int)Btns.Btn_4).DOFade(0f, 1f);

                var Btn3 = GetObject((int)UI.EA038).transform.GetChild(2).transform;
                Btn3.DOLocalMove(centerPos, 1f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_19_5살");
                    PopInstructionUIFromScaleZero("5살!", 4f);

                    Btn3.DOShakePosition(0.2f, 40f);
                });
                Btn3.DOScale(Vector3.one * 1.6f, 1f);
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
                PopInstructionUIFromScaleZero("형님이 되어 5살이 되었어요!", 3f);

            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_6_이제부터_나이와_관련된_놀이를_시작할거에요_");
                PopInstructionUIFromScaleZero("이제부터 나이와 관련된 놀이를\n시작할거에요!", 4f);

            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                gameManager.ChangeStage(EA038_MainSeq.CardGameStageSequence);
            })
            ;
        
    }

}