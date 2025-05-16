using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
///     CoreClass--------
///     UIManager 기본적인 초기화 수행합니다.
/// </summary>
public class UIManager_CommonBehaviorController : Base_UIManager
{
    /// <summary>
    ///     수동 UI초기화임에 주의
    /// </summary>

    protected bool isInited;
    protected override void Awake()
    {

    }

    protected void Start()
    {
        
    }

    public void ManualInit()
    {
        if (isInited) return;

        BindTMP(typeof(TMPs));
        BindObject(typeof(UI));

        UI_Instruction = GetObject((int)UI.InstructionUI);
        TMP_Instruction = GetTMP((int)TMPs.TMP_Instruction);

        TMP_Instruction.text = string.Empty;
        UI_Instruction.SetActive(false);

        _bgRectTransform = UI_Instruction.GetComponent<RectTransform>();
        _originalHeight = _bgRectTransform.sizeDelta.y;
        //DEFAULT_SIZE = UI_Instruction.transform.localScale;

        isInited = true;
    }
    public override bool InitEssentialUI()
    {
        
        isInited = true;
        return true;
        // Initialize UI elements here
    }

    public void ShowInitialMessage(string message, float duration = 5f, float delay = 1.5f)
    {
        if (!isInited)
        {
            ManualInit();
        }
        Logger.ContentTestLog("ShowInitialMessage-----------------------------");
        if (message == string.Empty) return;

        PopFromZeroInstructionUI(message, duration, delay);
    }
}