using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EA006_UIManager : UI_Base
{
    public enum TMP
    {
        MessageBox,
        SparrowCount
    }

    public override bool InitEssentialUI()
    {
        BindTMP(typeof(TMP));

        EA006_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
        EA006_GameManager.SeqMessageEvent += OnGetMessageEventFromGm;
        
        EA006_GameManager.SparrowCountEvent -= SparrowCountEvent;
        EA006_GameManager.SparrowCountEvent += SparrowCountEvent;
        GetTMP((int)TMP.MessageBox).text = string.Empty;
        return true;
    }


    private void OnGetMessageEventFromGm(int message)
    {
        
        Logger.Log($"Get Message ---- {message}");
        switch (message)
        {
            case (int)EA006_GameManager.SequenceName.Default:
                GetTMP((int)TMP.MessageBox).text = string.Empty;
                
                break;
            case (int)EA006_GameManager.SequenceName.GrassColorChange:
                GetTMP((int)TMP.MessageBox).text = "벼가 아직 안 익었어요 변신시켜줄까요?";
                Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/RipenIt");
                break;
            
            case (int)EA006_GameManager.SequenceName.FindScarecrow:
                Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/ThereScareCrow");
                GetTMP((int)TMP.MessageBox).text = "가을 곡식을 지켜주는 허수아비 아저씨가 있어요";
                break;
            case (int)EA006_GameManager.SequenceName.SparrowAppear:
                Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/SparrowAppear");
                GetTMP((int)TMP.MessageBox).text = "참새가 나타났어요!";
                DOVirtual.DelayedCall(2.5f, () =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/HelpScareCrow");
                    GetTMP((int)TMP.MessageBox).text = "참새를 잡아서 허수아비 아저씨를 도와줘요";
                });
                break;
            case (int)EA006_GameManager.SequenceName.OnFinish:
                Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/FinishCatching");
                GetTMP((int)TMP.MessageBox).text = "참새를 다 잡았어요!\n 우리가 허수아비 아저씨를 도와줬어요!";
                break;
           
        }
    }
    
    private void SparrowCountEvent(int message)
    {
        
        Logger.Log($"Get Message ---- {message}");

        GetTMP((int)TMP.MessageBox).text = "참새를 잡아서 허수아비 아저씨를 도와줘요\n" +message.ToString() + "마리";
    }

    private void OnDestroy()
    {
        EA006_GameManager.SparrowCountEvent -= OnGetMessageEventFromGm;
        EA006_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
    }
}
