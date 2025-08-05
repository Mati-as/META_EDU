using System;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using TMPro;
using UIManagers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
///     1.아래 기본기능만 사용할땐 씬번호_UIManager 생성후 Base_UIManager만 할당해서 사용가능
///     2.게임별 특별한 UI기능 추가시 상속받아 별도 UIManager 클래스 구성 필요
/// </summary>
public class Base_InGameUIManager : UI_PopUp,IInGameUIManager
{
    public enum UI
    {
        UI_Instruction,

        UI_MediaArtInScene,
        
        //Optional Tools-------------------------------
        UI_ReadyAndStart,
            Ready,
            Start,
            Stop
    }
    
    public enum TMPs
    {
        TMP_Instruction
    }
        
    protected TextMeshProUGUI TMP_Instruction;
    protected GameObject UI_Instruction;


    protected readonly Vector3 DEFAULT_SIZE = Vector3.one;
    protected RectTransform _bgRectTransform;
    protected float _originalHeight;

    protected Sequence _uiSeq;
    private bool isInitialized = false;
    
    
    private UI_ElementsLoader _uiElementsLoader =new();
    
    
    public UI_MediaArtInScene uiMediaArtInScene;
    // protected bool isInitialChecksumPassed = false; // UIManager가 적절한 GameManager와 초기화 되었는지 체크하는 변수


    private Dictionary<int, GameObject> _uiObjects= new();

    protected RectTransform UI_Ready;
    protected RectTransform UI_Start;
    protected RectTransform UI_Stop;

    /// <summary>
    /// [런처 UI 에서 호출]
    /// 1.로드시 자동 초기화 함수
    /// 2.UI_Setting, 등 UIMAnager 로드시 순서를 지정할 필요 없는 나머지 UI는 자동적으로 이벤트함수에서 Init되도록 구현했습니다.
    /// </summary>
    /// <returns></returns>
    public override bool InitOnLoad()
    {
        return true;
    }
    
    /// <summary>
    /// [런처가 아닌 반드시 게임매니저가 있는 씬에서만 호출]
    /// 1.순서보장형 ExplicitInit() 함수
    /// 2. GameManager에서 UIManager 호출, 프리팹 로드 후 초기화되어아 햐는 경우 아래 함수를 호출하여 순서보장형 초기화를 진행합니다. 
    /// </summary>
    public override void ExplicitInitInGame()
    {
      
        BindTMP(typeof(TMPs));
        BindObject(typeof(UI));
        
        uiMediaArtInScene = GetObject((int)UI.UI_MediaArtInScene).GetComponent<UI_MediaArtInScene>();
        UI_Instruction = GetObject((int)UI.UI_Instruction);
        TMP_Instruction = GetTMP((int)TMPs.TMP_Instruction);

       
        
        _bgRectTransform = UI_Instruction.GetComponent<RectTransform>();
        _originalHeight = _bgRectTransform.sizeDelta.y;

        UI_Instruction.transform.localScale = Vector3.zero;
        Logger.CoreClassLog("Base UI Manager Init ------------------------");

        Debug.Assert(TMP_Instruction != null, "TMP_Instruction is null");
        Debug.Assert(UI_Instruction != null, "UI_Instruction is null");
        
        InitOptionalTools();
        
        _objects = new Dictionary<Type, Object[]>();
        
        
    }
    private void InitOptionalTools()
    {
        UIReadyAndStartInit();
    }
    private void UIReadyAndStartInit()
    {
        _uiObjects.Add((int)UI.UI_ReadyAndStart,GetObject((int)UI.UI_ReadyAndStart));
        _uiObjects[(int)UI.UI_ReadyAndStart].SetActive(false);
        
        UI_Ready = _uiObjects[(int)UI.UI_ReadyAndStart].transform.Find(UI.Ready.ToString()).GetComponent<RectTransform>();
        UI_Stop = _uiObjects[(int)UI.UI_ReadyAndStart].transform.Find(UI.Stop.ToString()).GetComponent<RectTransform>();
        UI_Start = _uiObjects[(int)UI.UI_ReadyAndStart].transform.Find(UI.Start.ToString()).GetComponent<RectTransform>();
        
        UI_Ready.localScale = Vector3.zero;
        UI_Start.localScale = Vector3.zero;
        UI_Stop.localScale = Vector3.zero;
    }


    public void LoadUIElements(GameObject parent)
    {
        // Load UI elements using the UI_ElementsLoader
        _uiElementsLoader.LoadUIElements(parent);
        
    }
    /// <summary>
    ///     Base에서는 필수 및 공통요소만 바인드합니다.
    ///     추가로 바인딩 하고싶은경우, override 및 enum 추가선언하여 사용
    /// </summary>
    protected virtual void Bind()
    {
        BindTMP(typeof(TMPs));
        BindObject(typeof(UI));
    }

    /// <summary>
    ///     최신 Instruction UI를 사용하고싶을때 InitInstructionUI()를 호출합니다.
    ///     아래함수를 사용하지않는경우 기본적으로 사용하지 않는것으로 간주 합니다.
    /// </summary>
    public void InitInstructionUI()
    {
        //  UI_Instruction.SetActive(true);
        if(TMP_Instruction!=null)TMP_Instruction.text = string.Empty;
    }

    /// <summary>
    ///     애니메이션과 함께 텍스트를 바꿔줍니다.
    /// </summary>
    /// <param name="instruction"></param>
    public void PopInstructionUIFromScaleZero(string instruction, float duration = 10f, float delay = 0f,string narrationPath =null)
    {
        _uiSeq?.Kill();
        _uiSeq = DOTween.Sequence();
        _uiSeq.AppendInterval(delay);
//        Logger.ContentTestLog($"PopInstructionUI :활성화------------ {instruction}");

        UI_Instruction.SetActive(true);

        TMP_Instruction.text = instruction;

        UpdateBgSize();

        UI_Instruction.transform.localScale = Vector3.zero;
        _uiSeq.Append(UI_Instruction.transform.DOScale(DEFAULT_SIZE * 1.2f, 0.6f)
            .SetEase(Ease.InOutBounce));
        _uiSeq.Append(UI_Instruction.transform.DOScale(DEFAULT_SIZE, 0.15f)
            .SetEase(Ease.InOutBounce));

        if (duration > 0.5f)
        {
            _uiSeq.AppendInterval(duration);
            _uiSeq.Append(UI_Instruction.transform.DOScale(Vector3.zero, 0.15f)
                .SetEase(Ease.InOutBounce));
        }

        if(narrationPath != null)
        {
            Managers.Sound.Play(SoundManager.Sound.Narration, narrationPath);
        }
    }

    public void PopAndChangeUI(string instruction, float delayAndShutTme = 0f)
    {
        _uiSeq?.Kill();
        _uiSeq = DOTween.Sequence();

//        Logger.ContentTestLog($"PopInstructionUI ------------ {instruction}");

        UI_Instruction.SetActive(true);

        TMP_Instruction.text = instruction;

        UpdateBgSize();


        _uiSeq.Append(UI_Instruction.transform.DOScale(DEFAULT_SIZE * 1.35f, 0.15f)
            .SetEase(Ease.InOutBounce));
        _uiSeq.Append(UI_Instruction.transform.DOScale(DEFAULT_SIZE, 0.15f)
            .SetEase(Ease.InOutBounce));

        if (delayAndShutTme > 0.5f)
        {
            _uiSeq.AppendInterval(delayAndShutTme);
            _uiSeq.Append(UI_Instruction.transform.DOScale(Vector3.zero, 0.15f)
                .SetEase(Ease.InOutBounce));
        }
    }


    public void ShutInstructionUI(string instruction = "")
    {
        _uiSeq?.Kill();
        _uiSeq = DOTween.Sequence();

        UpdateBgSize();
        UI_Instruction.SetActive(false);
        TMP_Instruction.text = string.Empty;

        UI_Instruction.SetActive(true);
        UI_Instruction.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            UI_Instruction.SetActive(false);
        });
    }


    protected void UpdateBgSize()
    {
        // TMP_Instruction.ForceMeshUpdate();

        // 텍스트 기반 사이즈 측정 (공백, 기호 포함)
        var textSize = TMP_Instruction.GetPreferredValues(TMP_Instruction.text, 1000f, 0f); // 너비 한도 설정
        float paddingX = 100f; // 좌우 여백


        float finalWidth = textSize.x + paddingX;
        float finalHeight = TMP_Instruction.text.Contains("\n") ? _originalHeight * 1.5f : _originalHeight;

        _bgRectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
    }


    private Sequence _timerRelatedAnimSeq;

    public void PlayReadyAndStart(Action OnStart = null, float intervalBtwStartAndReady = 1f)
    {
        _uiObjects[(int)UI.UI_ReadyAndStart].SetActive(true);
        
        _timerRelatedAnimSeq?.Kill();
        _timerRelatedAnimSeq = DOTween.Sequence();

        UI_Ready.gameObject.SetActive(true);
        UI_Start.gameObject.SetActive(true);


        _timerRelatedAnimSeq.AppendCallback(() =>
        {
            UI_Ready.localScale = Vector3.one;
        });
        _timerRelatedAnimSeq.Append(UI_Ready.DOShakeScale(1.2f, 0.5f).OnStart(() =>
        {
            // PopFromZeroInstructionUI("준비!");
            Managers.Sound.Play(SoundManager.Sound.Narration, "EA020/Ready");
        }));
        _timerRelatedAnimSeq.AppendInterval(0.65f);
        _timerRelatedAnimSeq.Append(UI_Ready.DOScale(Vector3.zero, 0.3f));
        _timerRelatedAnimSeq.AppendInterval(intervalBtwStartAndReady);
        _timerRelatedAnimSeq.AppendCallback(() =>
        {
            UI_Start.localScale = Vector3.one;
        });
        _timerRelatedAnimSeq.Append(UI_Start.DOShakeScale(0.8f, 0.4f).OnStart(() =>
        {
            // PopFromZeroInstructionUI("시작!");
            Managers.Sound.Play(SoundManager.Sound.Narration, "EA020/Start");
        }));
        _timerRelatedAnimSeq.AppendInterval(0.1f);
        _timerRelatedAnimSeq.AppendCallback(() =>
        {
            OnStart?.Invoke();
        });
        _timerRelatedAnimSeq.Append(UI_Start.DOScale(Vector3.zero, 0.1f));
        _timerRelatedAnimSeq.AppendCallback(() =>
        {
            UI_Ready.gameObject.SetActive(false);
            UI_Start.gameObject.SetActive(false);
        });

        _timerRelatedAnimSeq.OnKill(() =>
        {
            UI_Ready.localScale = Vector3.zero;
            UI_Start.localScale = Vector3.zero;
            UI_Ready.gameObject.SetActive(false);
            UI_Start.gameObject.SetActive(false);
        });
    }

    public void PlayStopAnimation(float interval = 1.5f)
    {
        UI_Stop.gameObject.SetActive(true);

        _timerRelatedAnimSeq?.Kill();
        _timerRelatedAnimSeq = DOTween.Sequence();

        _timerRelatedAnimSeq.Append(UI_Stop.DOScale(Vector3.one * 1.1f, 1f).OnStart(() =>
        {
            Managers.Sound.Play(SoundManager.Sound.Narration, "EA020/Stop");
        }));
        _timerRelatedAnimSeq.AppendInterval(interval);
        _timerRelatedAnimSeq.Append(UI_Stop.DOScale(Vector3.zero, 1f));
        _timerRelatedAnimSeq.AppendCallback(() =>
        {
            UI_Stop.gameObject.SetActive(false);
        });

        _timerRelatedAnimSeq.OnKill(() =>
        {
            UI_Stop.localScale = Vector3.zero;
            UI_Stop.gameObject.SetActive(false);
        });
    }

    public void PlayTimerRelatedAnimationByEnum(UI ui)
    {
        if (ui == UI.Start) PlayTimerRelatedAnimation(UI_Start);
        if (ui == UI.Ready) PlayTimerRelatedAnimation(UI_Ready);
        if (ui == UI.Stop) PlayTimerRelatedAnimation(UI_Stop);
    }

    private void PlayTimerRelatedAnimation(RectTransform rect, float interval = 1.25f)
    {
        rect.gameObject.SetActive(true);

        _timerRelatedAnimSeq?.Kill();
        _timerRelatedAnimSeq = DOTween.Sequence();

        _timerRelatedAnimSeq.Append(rect.DOScale(Vector3.one * 1.5f, 1f));
        _timerRelatedAnimSeq.AppendInterval(interval);
        _timerRelatedAnimSeq.Append(rect.DOScale(Vector3.zero, 1f));
        _timerRelatedAnimSeq.AppendCallback(() =>
        {
            rect.gameObject.SetActive(false);
        });

        _timerRelatedAnimSeq.OnKill(() =>
        {
            rect.localScale = Vector3.zero;
            rect.gameObject.SetActive(false);
        });
    }
}