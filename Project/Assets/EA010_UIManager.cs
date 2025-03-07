using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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

        EA010_AutumnalFruits_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
        EA010_AutumnalFruits_GameManager.SeqMessageEvent += OnGetMessageEventFromGm;
        return true;
    }


    private void OnGetMessageEventFromGm(string message)
    {
        switch (message)
        {
            case nameof(EA010_AutumnalFruits_GameManager.MessageSequence.Intro):
                GetText((int)TMP.MessageBox).text = "가을에는 주렁주렁열매가 매달려요\n어떤 열매가 있을까요?";
                break;

            case nameof(EA010_AutumnalFruits_GameManager.SeqName.Default):
                GetText((int)TMP.MessageBox).text = string.Empty;
                break;
                     
            // case nameof(EA010_AutumnalFruits_GameManager.Fruits.Chestnut) + "Q":
            //     DOVirtual.DelayedCall(1f, () => { GetText((int)TMP.MessageBox).text = "이 열매는 갈색이고 딱딱해요~\n어떤 열매 일까요?"; });
            //     break;
            //
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Chestnut):
                DOVirtual.DelayedCall(1f, () => { GetText((int)TMP.MessageBox).text = "밤"; });
                break;
   
            
            
            // case nameof(EA010_AutumnalFruits_GameManager.Fruits.Acorn) + "Q":
            //     DOVirtual.DelayedCall(1f, () => { GetText((int)TMP.MessageBox).text = "이 열매는 모자를 쓰고 있고\n 다람쥐가 좋아해요~"; });
            //     break;

            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Acorn):
                DOVirtual.DelayedCall(1f, () => { GetText((int)TMP.MessageBox).text = "도토리"; });
                break;

            
            
            // case nameof(EA010_AutumnalFruits_GameManager.Fruits.Apple) + "Q":
            //     DOVirtual.DelayedCall(1f, () => { GetText((int)TMP.MessageBox).text = "이 열매는 빨간색이고 아삭아삭해요~ \n 어떤열매일까요?"; });
            //     break;

            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Apple):
                DOVirtual.DelayedCall(1f, () => { GetText((int)TMP.MessageBox).text = "사과"; });
                break;

            
            // case nameof(EA010_AutumnalFruits_GameManager.Fruits.Gingko) + "Q":
            //     DOVirtual.DelayedCall(1f, () => { GetText((int)TMP.MessageBox).text = "이 열매는 노란색이고 동글동글해요~ \n 어떤열매일까요?"; });
                break;
            
            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Gingko):
                DOVirtual.DelayedCall(1f, () => { GetText((int)TMP.MessageBox).text = "은행"; });
          
                break;

            case nameof(EA010_AutumnalFruits_GameManager.Fruits.Persimmon):

                DOVirtual.DelayedCall(1f, () => { GetText((int)TMP.MessageBox).text = "감"; });
                
                break;
        }
    }

    private void OnDestroy()
    {
        EA010_AutumnalFruits_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
    }
}
