using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class EA012_UIManager : Base_UIManager
{

    private void Awake()
    {
        // 메시지 구독
        Messenger.Default.Subscribe<EA012Payload>(OnNarrationReceived);
    }

    private void OnDestroy()
    {
        // 구독 해제
        Messenger.Default.Unsubscribe<EA012Payload>(OnNarrationReceived);
    }

    private void OnNarrationReceived(EA012Payload payload)
    {
        Debug.Log($"[Narration Received] {payload.Narration}");



        switch (payload.Narration)
        {
            case nameof(EA012_GameManager.MainSeq.Default):
            break;
            
            case nameof(EA012_GameManager.MainSeq.SeatSelection):
                GetText((int)TMPs.TMP_Instruction).text = "먼저 친구들 각자 표시된 자리에 앉아주세요!";
            break;
            case nameof(EA012_GameManager.MainSeq.SeqB_Ambulance):
                GetText((int)TMPs.TMP_Instruction).text = "(구급차)바퀴가 굴러다녀요";
            break;
            case nameof(EA012_GameManager.MainSeq.SeqC_PoliceCar):
            break;
            case nameof(EA012_GameManager.MainSeq.SeqD_FireTruck):
            break;
            case nameof(EA012_GameManager.MainSeq.SeqE_Taxi):
            break;
            case nameof(EA012_GameManager.MainSeq.SeqF_Bus):
            case nameof(EA012_GameManager.MainSeq.CarMoveHelpFinished):
                GetText((int)TMPs.TMP_Instruction).text = "바퀴를 다 멈췄어요!";
                break;
            
            case nameof(EA012_GameManager.MainSeq.SeqB_Ambulance_Move):
                GetText((int)TMPs.TMP_Instruction).text = "앰뷸런스를 움직일 수 있게 도와주세요! 길을 터치해주세요!";
                break;
            case nameof(EA012_GameManager.MainSeq.SeqC_PoliceCar_Move):
                GetText((int)TMPs.TMP_Instruction).text = "경찰차를 움직일 수 있게 도와주세요! 길을 터치해주세요!";
                break;
            case nameof(EA012_GameManager.MainSeq.SeqD_FireTruck_Move):
                GetText((int)TMPs.TMP_Instruction).text = "소방차를 움직일 수 있게 도와주세요! 길을 터치해주세요!";
                break;
            case nameof(EA012_GameManager.MainSeq.SeqE_Taxi_Move):
                GetText((int)TMPs.TMP_Instruction).text = "택시를 움직일 수 있게 도와주세요! 길을 터치해주세요!";
                break;
            case nameof(EA012_GameManager.MainSeq.SeqF_Bus_Move):
                GetText((int)TMPs.TMP_Instruction).text = "버스를 움직일 수 있게 도와주세요! 길을 터치해주세요!";
                break;
           
        }
      
    }


}
