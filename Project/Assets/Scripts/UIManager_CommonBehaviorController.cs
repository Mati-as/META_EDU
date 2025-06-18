using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

/// <summary>
///     CoreClass--------
///     UIManager 기본적인 초기화 수행합니다.
///     이중참조 구조임에 주의 *************
///     실행순서
///     UIManager_CommonBehaviorController -> 각 씬에 달려있는 UI의 InitEssentialUI()를 호출
/// </summary>
public class UIManager_CommonBehaviorController : UI_PopUp
{
    public enum UI
    {
        InstructionUI,
        
        UI_Ready,
        UI_Start,
        UI_Stop
    }

    public enum TMPs
    {
        TMP_Instruction
    }

    protected bool isInited;
    protected RectTransform _bgRectTransform;
    protected float _originalHeight;
    protected readonly Vector3 DEFAULT_SIZE = Vector3.one;

    public override bool InitEssentialUI()
    {
        isInited = true;

        BindTMP(typeof(TMPs));
        BindObject(typeof(UI));

        if(GetObject((int)UI.InstructionUI) == null)
        {
            Logger.ContentTestLog("InstructionUI is null or not active ------------해당컨텐츠에서 미사용 아닌경우 확인 필요  (25/0523)");
            return false;
        }

        
        GetObject((int)UI.InstructionUI).transform.localScale = Vector3.zero;
        GetTMP((int)TMPs.TMP_Instruction);

                
        
        GetObject(((int)UI.UI_Ready)).transform.localScale =Vector3.zero;
        GetObject(((int)UI.UI_Start)).transform.localScale =Vector3.zero;
        GetObject(((int)UI.UI_Stop)). transform.localScale =Vector3.zero;

        
        GetTMP((int)TMPs.TMP_Instruction).text = string.Empty;
       // GetObject((int)UI.InstructionUI).SetActive(false);

        _bgRectTransform = GetObject((int)UI.InstructionUI).GetComponent<RectTransform>();
        _originalHeight = _bgRectTransform.sizeDelta.y;

        return base.InitEssentialUI();
    }


    private Sequence _uiSeq;

    public void ShowInitialMessage(string message, float duration = 5f, float delay = 0.85f)
    {
        Logger.ContentTestLog("ShowInitialMessage-----------------------------");
        if (message == string.Empty) return;

        PopFromZeroInstructionUI(message, duration, delay);
    }

    protected void PopFromZeroInstructionUI(string instruction, float duration = 0f, float delay = 0f)
    {
        _uiSeq?.Kill();
        _uiSeq = DOTween.Sequence();
        _uiSeq.AppendInterval(delay);
//        Logger.ContentTestLog($"PopInstructionUI :활성화------------ {instruction}");

        GetObject((int)UI.InstructionUI).SetActive(true);

        GetTMP((int)TMPs.TMP_Instruction).text = instruction;

        UpdateBgSize();

        GetObject((int)UI.InstructionUI).transform.localScale = Vector3.zero;
        _uiSeq.Append(GetObject((int)UI.InstructionUI).transform.DOScale(DEFAULT_SIZE * 1.2f, 0.6f)
            .SetEase(Ease.InOutBounce));
        _uiSeq.Append(GetObject((int)UI.InstructionUI).transform.DOScale(DEFAULT_SIZE, 0.15f)
            .SetEase(Ease.InOutBounce));

        if (duration > 0.5f)
        {
            _uiSeq.AppendInterval(duration);
            _uiSeq.Append(GetObject((int)UI.InstructionUI).transform.DOScale(Vector3.zero, 0.15f)
                .SetEase(Ease.InOutBounce));
        }
        
    }



    private void UpdateBgSize()
    {
        // GetTMP((int)TMPs.TMP_Instruction).ForceMeshUpdate();

        // 텍스트 기반 사이즈 측정 (공백, 기호 포함)
        var textSize = GetTMP((int)TMPs.TMP_Instruction)
            .GetPreferredValues(GetTMP((int)TMPs.TMP_Instruction).text, 1000f, 0f); // 너비 한도 설정
        float paddingX = 100f; // 좌우 여백


        float finalWidth = textSize.x + paddingX;
        float finalHeight = GetTMP((int)TMPs.TMP_Instruction).text.Contains("\n")
            ? _originalHeight * 1.35f
            : _originalHeight;

        _bgRectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
    }
}