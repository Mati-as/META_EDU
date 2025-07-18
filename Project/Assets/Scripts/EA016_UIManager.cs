using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class EA016_UIManager : Base_UIManager
{
    private EA016_GameManager _gm;

    private readonly string NARRATION_PATH = "Audio/BB006/Narration/";

    protected new enum UI
    {
        Pink,
        Yellow,
        Blue,
        AllBalls,
        BallImageAndCount
    }

    protected new enum TMPs
    {
        Count,
    }

    protected override void Awake()
    {
        base.Awake();
        // 메시지 구독
        Messenger.Default.Subscribe<UI_Payload>(OnGetMessageEventFromGm);

        if (_gm == null) _gm = GameObject.FindWithTag("GameManager").GetComponent<EA016_GameManager>();
        Debug.Assert(_gm != null, "GameManager not found");
    }

    private void OnDestroy()
    {
        // 구독 해제
        Messenger.Default.Unsubscribe<UI_Payload>(OnGetMessageEventFromGm);
    }


    public override void ExplicitInit()
    {
        base.ExplicitInit();
        Bind();
        GetObject((int)UI.BallImageAndCount).SetActive(false);
    }

    
 
    protected override void Bind()
    {
        BindTMP(typeof(TMPs));
        BindObject(typeof(UI));
    }


    private void OnGetMessageEventFromGm(UI_Payload payload)
    {
        Logger.ContentTestLog($"Get Message ---- {payload.Narration}");
        if (payload.IsCustom)
        {
            if (payload.IsPopFromZero) PopInstructionUIFromScaleZero($"{payload.Narration}", payload.DelayAndAutoShutTime);
            else PopAndChangeUI(payload.Narration, payload.DelayAndAutoShutTime);
        }
    }

    private readonly Vector3 _originScale = Vector3.one;

    public void ShowBallImageAndCountWithRefresh()
    {
        GetObject((int)UI.BallImageAndCount).transform.localScale = Vector3.zero;

      


        GetObject((int)UI.Pink).SetActive(false);
        GetObject((int)UI.Yellow).SetActive(false);
        GetObject((int)UI.Blue).SetActive(false);
        GetObject((int)UI.AllBalls).SetActive(false);



        if (_gm.CurrentMainSeq == (int)EA016_GameManager.MainSequence.ColorMode)
        {
            GetTMP((int)TMPs.Count).text = _gm.ballCountLeftToPut.ToString();
            switch (_gm.currentBallColorToPut)
            {
                case BallInfo.BallColor.Pink:
                    GetObject((int)UI.Pink).SetActive(true);
                    Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "PutPinkBall");
                    break;
                case BallInfo.BallColor.Yellow:
                  
                    Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "PutYellowBall");
                    GetObject((int)UI.Yellow).SetActive(true);
                    break;
                case BallInfo.BallColor.Blue:
                    Managers.Sound.Play(SoundManager.Sound.Narration, NARRATION_PATH + "PutBlueBall");
                    GetObject((int)UI.Blue).SetActive(true);
                    break;
            }
        }
        else if (_gm.CurrentMainSeq == (int)EA016_GameManager.MainSequence.CountMode)
        {
            GetTMP((int)TMPs.Count).text = _gm.BallCountGoal.ToString();
            GetObject((int)UI.AllBalls).SetActive(true);
        }

       

        GetObject((int)UI.BallImageAndCount).SetActive(true);
        GetObject((int)UI.BallImageAndCount).transform.DOScale(_originScale, 0.15f).SetEase(Ease.InOutBounce);
    }

    public void RefreshText()
    {
        
        
        int cache = _gm.ballCountLeftToPut;
        if (_gm.CurrentMainSeq == (int)EA016_GameManager.MainSequence.ColorMode)
        {
            GetTMP((int)TMPs.Count).text = Mathf.Clamp(cache,0,50).ToString();
            GetObject((int)UI.BallImageAndCount).transform.localScale = Vector3.zero;
            GetObject((int)UI.BallImageAndCount).SetActive(true);
            GetObject((int)UI.BallImageAndCount).transform.DOScale(_originScale, 0.15f).SetEase(Ease.InOutBounce);
        }

    }
    public void ShutBallCountUI()
    {
      
        GetObject((int)UI.BallImageAndCount).transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InOutBounce)
            .OnComplete(()=>
            {
             //   GetObject((int)UI.BallImageAndCount).SetActive(false);
            });
    }
    
}