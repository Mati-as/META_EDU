using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class EA006_UIManager : Base_UIManager
{
    public enum TMP
    {
      //  MessageBox,
        SparrowCount
    }

    public override bool InitEssentialUI()
    {
        base.InitEssentialUI();
        BindTMP(typeof(TMP));

        EA006_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
        EA006_GameManager.SeqMessageEvent += OnGetMessageEventFromGm;
        
        EA006_GameManager.SparrowCountEvent -= SparrowCountEvent;
        EA006_GameManager.SparrowCountEvent += SparrowCountEvent;
        //GetTMP((int)TMP.MessageBox).text = string.Empty;
        return true;
    }


    private void OnGetMessageEventFromGm(int message)
    {
        
        Logger.Log($"Get Message ---- {message}");
        switch (message)
        {
            case (int)EA006_GameManager.SequenceName.Default:
              //  PopFromZeroInstructionUI( string.Empty);
                
                break;
            case (int)EA006_GameManager.SequenceName.GrassColorChange:
                PopFromZeroInstructionUI("초록색 벼를 터치해 노란색으로 바꿔주세요!");
                Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/RipenIt");

                float dealyAmount = Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length + 0.45f;
                DOVirtual.DelayedCall(dealyAmount, () =>
                {
                   //초록색 벼를 터치해~ 재생
                    Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/Narration/ChangeRiceToYellow");
                });
                
                break;
            
            case (int)EA006_GameManager.SequenceName.FindScarecrow:
                Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/ThereScareCrow");
                
                PopFromZeroInstructionUI( "장난치는 허수아비 아저씨를 터치해주세요!");
                float dealyAmountB = Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length + 0.45f;
                DOVirtual.DelayedCall(dealyAmountB, () =>
                {
                    //장난치는 허수아비 아저씨를 터치해주세요!
                    Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/Narration/GetRidOfScarecrow");
                });
                break;
            case (int)EA006_GameManager.SequenceName.SparrowAppear:
                Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/SparrowAppear");
                PopFromZeroInstructionUI( "참새가 나타났어요!");
                DOVirtual.DelayedCall(2.5f, () =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/HelpScareCrow");
                    
                    PopFromZeroInstructionUI( "참새를 터치해 쫓아주세요");
                    float dealyAmountC = Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length + 0.45f;
                    DOVirtual.DelayedCall(dealyAmountC, () =>
                    {
                        //참새를 터치해 쫓아주세요 재생
                        Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/Narration/GetRidOfSparrows");
                    });
                });
                break;
            case (int)EA006_GameManager.SequenceName.OnFinish:
                
                DOVirtual.DelayedCall(3f, () =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA006/FinishCatching");
                    PopFromZeroInstructionUI( "참새를 다 잡았어요!\n 우리가 허수아비 아저씨를 도와줬어요!");
                });
                
                break;
           
        }
    }
    
    private void SparrowCountEvent(int message)
    {
        
        Logger.Log($"Get Message ---- {message}");
        PopFromZeroInstructionUI( "참새를 잡아서 허수아비 아저씨를 도와줘요\n" +message.ToString() + "마리");
    }

    private void OnDestroy()
    {
        EA006_GameManager.SparrowCountEvent -= OnGetMessageEventFromGm;
        EA006_GameManager.SeqMessageEvent -= OnGetMessageEventFromGm;
    }
}
