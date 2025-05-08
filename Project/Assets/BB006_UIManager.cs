using System.Collections;
using System.Collections.Generic;
using SuperMaxim.Messaging;
using UnityEngine;

public class BB006_UIManager : Base_UIManager
{
    private BB006_GameManager _gm;

    private void Awake()
    {
        // 메시지 구독
        Messenger.Default.Subscribe<UI_Payload>(OnGetMessageEventFromGm);
        
        if (_gm == null) _gm = GameObject.FindWithTag("GameManager").GetComponent<BB006_GameManager>();
        Debug.Assert(_gm != null, "GameManager not found");
        
      
    }

    public override bool Init()
    {
        base.Init();
        InitInstructionUI();
        return true;
    }
    
    
    private void OnGetMessageEventFromGm(UI_Payload payload)
    {
    
        
        if (payload.IsCustom)
        {
            if(payload.IsPopFromZero) PopFromZeroInstructionUI($"{payload.Narration}");
            else PopAndChangeUI(payload.Narration);
            return;
        }
        Logger.ContentTestLog($"Get Message ---- {payload.Narration}");
        switch (payload.Narration)
        {
            case nameof(EA009_HealthyFood_GameManager.GameObj.FishA):
                if(payload.IsPopFromZero) PopFromZeroInstructionUI($"{payload.Narration}");
                else PopAndChangeUI(payload.Narration);
                Managers.Sound.Play(SoundManager.Sound.Narration,  "Fish");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.AppleA):
                if(payload.IsPopFromZero) PopFromZeroInstructionUI($"{payload.Narration}");
                else PopAndChangeUI(payload.Narration);
                Managers.Sound.Play(SoundManager.Sound.Narration,  "Apple");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.MeatA):
                if(payload.IsPopFromZero) PopFromZeroInstructionUI($"{payload.Narration}");
                else PopAndChangeUI(payload.Narration);
                Managers.Sound.Play(SoundManager.Sound.Narration,  "Meat");
                break;
            
            default: break;
        }
    }

}
