using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MyCustomizedEditor.Common.Util;
using Unity.VisualScripting;
using UnityEngine;
using Sequence = DG.Tweening.Sequence;



public class Base_UIManager : UI_PopUp
{
  
    protected enum UI
    {
        InstructionUI
    }

    protected enum TMPs
    {
        TMP_Instruction
    }
    
    private Vector3 _originScale = Vector3.one;
    private RectTransform _bgRectTransform;
    private float _originalHeight;

    private Sequence _uiSeq;
    protected bool isInitialChecksumPassed = false; // UIManager가 적절한 GameManager와 초기화 되었는지 체크하는 변수
    
    
    public override bool Init()
    {
        //base.Init();
        var found = GetComponents<Base_UIManager>();
        foreach (var comp in found)
        {
            if (comp != this)
            {
                Logger.CoreClassLog("⚠ 이미 Base_UIManager가 존재하므로 Init 생략");
                return false;
            }
        }

        BindTMP(typeof(TMPs));
        BindObject(typeof(UI));
        
        GetTMP((int)TMPs.TMP_Instruction).text = string.Empty;
        GetObject((int)UI.InstructionUI).SetActive(false);
        _bgRectTransform = GetObject((int)UI.InstructionUI).GetComponent<RectTransform>();
        _originalHeight = _bgRectTransform.sizeDelta.y;
        _originScale = GetObject((int)UI.InstructionUI).transform.localScale;
        Logger.CoreClassLog("Base UI Manager Init ------------------------");
        return true;
        // Initialize UI elements here
    }

    /// <summary>
    /// 최신 Instruction UI를 사용하고싶을때 InitInstructionUI()를 호출합니다.
    /// 아래함수를 사용하지않는경우 기본적으로 사용하지 않는것으로 간주 합니다. 
    /// </summary>
    protected void InitInstructionUI()
    {
        //GetObject((int)UI.InstructionUI).SetActive(true);
        GetTMP((int)TMPs.TMP_Instruction).text = string.Empty;
    }
    
    /// <summary>
    /// 애니메이션과 함께 텍스트를 바꿔줍니다.
    /// </summary>
    /// <param name="instruction"></param>
    protected void PopFromZeroInstructionUI(string instruction)
    {
        _uiSeq?.Kill();
        _uiSeq = DOTween.Sequence();
        
        Logger.ContentTestLog($"PopInstructionUI :활성화------------ {instruction}");
     
        GetObject((int)UI.InstructionUI).SetActive(true);
       
        GetTMP((int)TMPs.TMP_Instruction).text = instruction;
        
        UpdateBgSize();
        
        GetObject((int)UI.InstructionUI).transform.localScale = Vector3.zero;
        _uiSeq.Append(GetObject((int)UI.InstructionUI).transform.DOScale(_originScale * 1.2f, 0.6f)
            .SetEase(Ease.InOutBounce));
        _uiSeq.Append(GetObject((int)UI.InstructionUI).transform.DOScale(_originScale, 0.15f)
            .SetEase(Ease.InOutBounce));
        
      
    }
    
    protected void PopAndChangeUI(string instruction)
    {
        _uiSeq?.Kill();
        _uiSeq = DOTween.Sequence();
        
        Logger.ContentTestLog($"PopInstructionUI ------------ {instruction}");
     
        GetObject((int)UI.InstructionUI).SetActive(true);
       
        GetTMP((int)TMPs.TMP_Instruction).text = instruction;
        
        UpdateBgSize();
        
      
        _uiSeq.Append(GetObject((int)UI.InstructionUI).transform.DOScale(_originScale * 1.35f, 0.6f)
            .SetEase(Ease.InOutBounce));
        _uiSeq.Append(GetObject((int)UI.InstructionUI).transform.DOScale(_originScale, 0.15f)
            .SetEase(Ease.InOutBounce));
        
      
    }

    
    protected void ShutInstructionUI(string instruction)
    {
        
        _uiSeq?.Kill();
        _uiSeq = DOTween.Sequence();
        
        UpdateBgSize();
        GetObject((int)UI.InstructionUI).SetActive(false);
        GetTMP((int)TMPs.TMP_Instruction).text = string.Empty;
        
        GetObject((int)UI.InstructionUI).SetActive(true);
        GetObject((int)UI.InstructionUI).transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutBounce).OnComplete(() =>
        {
            GetObject((int)UI.InstructionUI).SetActive(false);
        }); 
    }
    


    private void UpdateBgSize()
    {
       // GetTMP((int)TMPs.TMP_Instruction).ForceMeshUpdate();

        // 텍스트 기반 사이즈 측정 (공백, 기호 포함)
        Vector2 textSize =  GetTMP((int)TMPs.TMP_Instruction).GetPreferredValues( GetTMP((int)TMPs.TMP_Instruction).text, 1000f, 0f); // 너비 한도 설정
        float paddingX = 100f; // 좌우 여백
        float paddingY = 60f;  // 상하 여백

        float finalWidth = textSize.x + paddingX;
        float finalHeight =  GetTMP((int)TMPs.TMP_Instruction).text.Contains("\n") ? textSize.y + paddingY : _originalHeight;

        _bgRectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);
    }

}
