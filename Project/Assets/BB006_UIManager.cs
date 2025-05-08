using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class BB006_UIManager : Base_UIManager
{
    private BB006_GameManager _gm;


    protected new enum UI
    {
        Pink,
        Yellow,
        Blue,
        BallImageAndCount
    }

    protected new enum TMPs
    {
        Count,
    }

    private void Awake()
    {
        // 메시지 구독
        Messenger.Default.Subscribe<UI_Payload>(OnGetMessageEventFromGm);

        if (_gm == null) _gm = GameObject.FindWithTag("GameManager").GetComponent<BB006_GameManager>();
        Debug.Assert(_gm != null, "GameManager not found");
    }

    private void OnDestroy()
    {
        // 구독 해제
        Messenger.Default.Unsubscribe<UI_Payload>(OnGetMessageEventFromGm);
    }


    public override bool InitEssentialUI()
    {
        base.InitEssentialUI();
       // InitInstructionUI();
        Bind();
        GetObject((int)UI.BallImageAndCount).SetActive(false);
        return true;
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
            if (payload.IsPopFromZero) PopFromZeroInstructionUI($"{payload.Narration}", payload.DelayAndAutoShutTime);
            else PopAndChangeUI(payload.Narration, payload.DelayAndAutoShutTime);
        }
    }

    private readonly Vector3 _originScale = Vector3.one;

    public void ShowBallImageAndCountWithRefresh()
    {
        GetObject((int)UI.BallImageAndCount).transform.localScale = Vector3.zero;

        GetTMP((int)TMPs.Count).text = _gm.ballCountLeftToPut.ToString();


        GetObject((int)UI.Pink).SetActive(false);
        GetObject((int)UI.Yellow).SetActive(false);
        GetObject((int)UI.Blue).SetActive(false);

        switch (_gm.currentBallColorToPut)
        {
            case BallInfo.BallColor.Pink:
                GetObject((int)UI.Pink).SetActive(true);
                break;
            case BallInfo.BallColor.Yellow:
                GetObject((int)UI.Yellow).SetActive(true);
                break;
            case BallInfo.BallColor.Blue:
                GetObject((int)UI.Blue).SetActive(true);
                break;
        }

        GetObject((int)UI.BallImageAndCount).SetActive(true);
        GetObject((int)UI.BallImageAndCount).transform.DOScale(_originScale, 0.15f).SetEase(Ease.InOutBounce);
    }

    public void RefreshText()
    {
        GetTMP((int)TMPs.Count).text = _gm.ballCountLeftToPut.ToString();

        GetObject((int)UI.BallImageAndCount).transform.localScale = Vector3.zero;
        GetObject((int)UI.BallImageAndCount).SetActive(true);
        GetObject((int)UI.BallImageAndCount).transform.DOScale(_originScale, 0.15f).SetEase(Ease.InOutBounce);
    }
    
    
}