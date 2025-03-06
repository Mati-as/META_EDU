using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class EA010_UIManager : UI_Base
{

    public enum TMP
    {
        MessageBox
    }

    public override bool Init()
    {
        BindText(typeof(TMP));

        EA010_AutumnalFruits_GameManager.SequnceMessage -= OnGetMessageFromGM;
        EA010_AutumnalFruits_GameManager.SequnceMessage += OnGetMessageFromGM;
        return true;
    }


    private void OnGetMessageFromGM(string message)
    {

        switch (message)
        {
            case nameof(EA010_AutumnalFruits_GameManager.MessageSequence.Intro) :
                GetText((int)TMP.MessageBox).text = "가을에는 주렁주렁열매가 매달려요\n어떤 열매가 있을까요?";
                break;
                
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Chestnut) :
                GetText((int)TMP.MessageBox).text = "밤";
                break;
            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Acorn) :
                GetText((int)TMP.MessageBox).text = "도토리";
                break;
            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Apple) :
                GetText((int)TMP.MessageBox).text = "사과";
                break;
            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Gingko) :
                GetText((int)TMP.MessageBox).text = "은행";
                break;
            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Persimmon) :
                GetText((int)TMP.MessageBox).text = "감";
                break;
        }
    }

    private void OnDestroy()
    {
        EA010_AutumnalFruits_GameManager.SequnceMessage -= OnGetMessageFromGM;
    }
}
