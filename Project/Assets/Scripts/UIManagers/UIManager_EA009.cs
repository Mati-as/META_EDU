using MyCustomizedEditor.Common.Util;
using SuperMaxim.Messaging;
using UnityEngine;

public class UIManager_EA009 : Base_UIManager
{

    private EA009_HealthyFood_GameManager _gm;

    private void Awake()
    {
        // 메시지 구독
        Messenger.Default.Subscribe<EA009_Payload>(OnGetMessageEventFromGm);
        if (_gm == null) _gm = GameObject.FindWithTag("GameManager").GetComponent<EA009_HealthyFood_GameManager>();

        Debug.Assert(_gm != null, "GameManager not found");
    }

    private void OnDestroy()
    {
        // 구독 해제
        Messenger.Default.Unsubscribe<EA009_Payload>(OnGetMessageEventFromGm);
    }

    private void ChangeText(string text)
    {
        if(GetText((int)TMPs.TMP_Instruction)!=null) GetText((int)TMPs.TMP_Instruction).text = text;
    }


    private void OnGetMessageEventFromGm(EA009_Payload payload)
    {
        Logger.ContentTestLog($"Get Message ---- {payload.Narration}");
        switch (payload.Narration)
        {
            case nameof(EA009_HealthyFood_GameManager.GameObj.FishA):
                ChangeText("생선");
                Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.MeatA):
                Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                ChangeText("고기");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.ChickenA):
                ChangeText("닭");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.EggA):
                ChangeText("계란");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.MilkA):
                ChangeText("우유");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.CarrotA):
                ChangeText("당근");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.ColaA):
                ChangeText("콜라");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.CookieA):
                ChangeText("쿠키");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;
            case nameof(EA009_HealthyFood_GameManager.GameObj.IceCreamA):
                ChangeText("아이스크림");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.PizzaA):
                ChangeText("피자");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.ChocolateA):
                ChangeText("초콜릿");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.CakeA):
                ChangeText("케이크");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.DonutA):
                ChangeText("도넛");
                 Managers.Sound.Play(SoundManager.Sound.Narration,"EA009/Narration/Fish");
                break;

            case nameof(EA009_HealthyFood_GameManager.MainSeq.OnFinish):

                break;
            
            default:
          //      ChangeText("기타 나레이션" + payload.Narration + ")");
                break;
        }
    }
}