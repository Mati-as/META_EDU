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
            case nameof(EA012_GameManager.Sequence.Default):
            break;
            
            case nameof(EA012_GameManager.Sequence.SeqA_WheelStopGame):
                GetText((int)TMPs.TMP_Instruction).text = "먼저 친구들 각자 표시된 자리에 앉아주세요!";
            break;
            case nameof(EA012_GameManager.Sequence.SeqB_Ambulance):
            break;
            case nameof(EA012_GameManager.Sequence.SeqC_PoliceCar):
            break;
            case nameof(EA012_GameManager.Sequence.SeqD_FireTruck):
            break;
            case nameof(EA012_GameManager.Sequence.SeqE_Airplane):
            break;
            case nameof(EA012_GameManager.Sequence.SeqF_Train):
                break;
           
        }
      
    }


}
