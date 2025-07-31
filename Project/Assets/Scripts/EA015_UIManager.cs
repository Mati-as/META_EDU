using System.Collections;
using System.Collections.Generic;
using SuperMaxim.Messaging;
using UnityEngine;

public class EA015_UIManager : Base_UIManager
{
private EA015_GameManager _gm;

    protected override void Awake()
    {
        base.Awake();
        // 메시지 구독
        Messenger.Default.Subscribe<EA015_Payload>(OnGetMessageEventFromGm);
        if (_gm == null) _gm = GameObject.FindWithTag("GameManager").GetComponent<EA015_GameManager>();

        Debug.Assert(_gm != null, "GameManager not found");
        
    }

    private void OnDestroy()
    {
        // 구독 해제
        Messenger.Default.Unsubscribe<EA015_Payload>(OnGetMessageEventFromGm);
    }

    private string _foodNarPath = "Audio/SortedByScene/EA009/Food/";

    private void OnGetMessageEventFromGm(EA015_Payload payload)
    {
        if (payload.IsCustom)
        {
            if(payload.IsPopFromZero) PopInstructionUIFromScaleZero($"{payload.Narration}");
            else PopAndChangeUI(payload.Narration);
            return;
        }
        Logger.ContentTestLog($"Get Message ---- {payload.Narration}");
        switch (payload.Narration)
        {
            case nameof(EA009_HealthyFood_GameManager.GameObj.FishA):
                PopInstructionUIFromScaleZero("생선");
                Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Fish");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.AppleA):
                PopInstructionUIFromScaleZero("사과");
                Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Apple");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.MeatA):
                Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Meat");
                break;
            
            default: break;
        }
    }
}
