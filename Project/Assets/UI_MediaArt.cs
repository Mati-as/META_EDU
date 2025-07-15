using System.Collections;
using System.Collections.Generic;
using MyCustomizedEditor.Common.Util;
using UnityEngine;

public class UI_MediaArt : UI_PopUp
{

    private Animator _animator;
    private bool _isOn = false;
    private static readonly int IsOn = Animator.StringToHash("isOn");

    private enum UI
    {
        Btn
    }

    private enum TMPs
    {
        TMP_MainTitle,
        TMP_SmallTitle,
        TMP_Body
    }
    public override bool InitEssentialUI()
    {
        return base.InitEssentialUI();
    }

    
    /// <summary>
    /// 기본적응로 초기화 되어있으나, 해당 UI를 사용하기 위해서는 InitToUse()를 호출해야 합니다.
    /// </summary>
    private void InitToUse()
    {
        BindObject(typeof(UI));
        BindTMP(typeof(TMPs));
        
        _animator = GetComponent<Animator>();
        GetObject((int)UI.Btn).BindEvent(() =>
        {
            _isOn = !_isOn;
            _animator.SetBool(IsOn, _isOn);
        });
    }
}
