using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;



public class Base_UIManager : UI_PopUp
{
  
    public enum UI
    {
        InstructionUI
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

    private Sequence _uiSeq;
   // protected bool isInitialChecksumPassed = false; // UIManager가 적절한 GameManager와 초기화 되었는지 체크하는 변수
    
    
    public override bool InitEssentialUI()
    {

        BindTMP(typeof(TMPs));
        BindObject(typeof(UI));
        UI_Instruction = GetObject(((int)UI.InstructionUI));
        TMP_Instruction = GetTMP((int)TMPs.TMP_Instruction);
        _objects = new();
        
        TMP_Instruction.text = string.Empty;
        UI_Instruction.SetActive(false);
        
        _bgRectTransform = UI_Instruction.GetComponent<RectTransform>();
        _originalHeight = _bgRectTransform.sizeDelta.y;
        //_UIInstructionOriginalScale = UI_Instruction.transform.localScale;
        
        Logger.CoreClassLog("Base UI Manager Init ------------------------");
        
        Debug.Assert(TMP_Instruction != null, "TMP_Instruction is null");
        Debug.Assert(UI_Instruction != null, "UI_Instruction is null");
        
        return true;
        // Initialize UI elements here
    }

    /// <summary>
    /// Base에서는 필수 및 공통요소만 바인드합니다.
    /// 추가로 바인딩 하고싶은경우, override 및 enum 추가선언하여 사용
    /// </summary>
    protected virtual void Bind()
    {
        BindTMP(typeof(TMPs));
        BindObject(typeof(UI));
    }

    /// <summary>
    /// 최신 Instruction UI를 사용하고싶을때 InitInstructionUI()를 호출합니다.
    /// 아래함수를 사용하지않는경우 기본적으로 사용하지 않는것으로 간주 합니다. 
    /// </summary>
    protected void InitInstructionUI()
    {
      //  UI_Instruction.SetActive(true);
        TMP_Instruction.text = string.Empty;
    }
    
    /// <summary>
    /// 애니메이션과 함께 텍스트를 바꿔줍니다.
    /// </summary>
    /// <param name="instruction"></param>
    protected void PopFromZeroInstructionUI(string instruction,float duration = 0f,float delay =0f)
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


    }
    
    protected void PopAndChangeUI(string instruction, float delayAndShutTme = 0f)
    {
        _uiSeq?.Kill();
        _uiSeq = DOTween.Sequence();
        
        Logger.ContentTestLog($"PopInstructionUI ------------ {instruction}");
     
        UI_Instruction.SetActive(true);
       
        TMP_Instruction.text = instruction;
        
        UpdateBgSize();
        
      
        _uiSeq.Append(UI_Instruction.transform.DOScale(DEFAULT_SIZE * 1.35f, 0.6f)
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

    
    protected void ShutInstructionUI(string instruction)
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
    


    private void UpdateBgSize()
    {
       // TMP_Instruction.ForceMeshUpdate();

        // 텍스트 기반 사이즈 측정 (공백, 기호 포함)
        Vector2 textSize =  TMP_Instruction.GetPreferredValues( TMP_Instruction.text, 1000f, 0f); // 너비 한도 설정
        float paddingX = 100f; // 좌우 여백
        float paddingY = 60f;  // 상하 여백

        float finalWidth = textSize.x + paddingX;
        float finalHeight =  TMP_Instruction.text.Contains("\n") ? _originalHeight * 2 : _originalHeight;

        _bgRectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
    }

}
