using MyCustomizedEditor.Common.Util;
using SuperMaxim.Messaging;
using UnityEngine;



/// <summary>
/// 좋은음식을 먹어요 UIManager-------------------------------------
/// </summary>
public class EA009_UIManager : Base_UIManager
{

    
    //순서주의
    public enum EA009_UI
    {
        Sprite_Cola,
        Sprite_Cookie,
        Sprite_IceCream,
        Sprite_Pizza,
        Sprite_Chocolate,
        Sprite_Cake,
        Sprite_Donut,
        
    }
    private EA009_HealthyFood_GameManager _gm;

    private void Awake()
    {
        // 메시지 구독
        Messenger.Default.Subscribe<EA009_Payload>(OnGetMessageEventFromGm);
        if (_gm == null) _gm = GameObject.FindWithTag("GameManager").GetComponent<EA009_HealthyFood_GameManager>();

        Debug.Assert(_gm != null, "GameManager not found");
        
    }

    public override bool InitEssentialUI()
    {
      
        base.InitEssentialUI();
        InitInstructionUI();
        
        BindObject(typeof(EA009_UI));

Logger.ContentTestLog("EA009 UI Manager Init ------------------------");
        TurnOffAllFoodSprite();
        return true;
    }

    public void TurnOffAllFoodSprite()
    {
        for(int i = 0; i <= (int)EA009_UI.Sprite_Donut; i++)
        {
            GetObject(i).SetActive(false);
        }
    }
    public void TurnOnSprite(int index)
    {
        for(int i = 0; i < (int)EA009_UI.Sprite_Chocolate; i++)
        {
            GetObject(i).SetActive(false);
        }
        
        GetObject(index).SetActive(true);
    }
    private void OnDestroy()
    {
        // 구독 해제
        Messenger.Default.Unsubscribe<EA009_Payload>(OnGetMessageEventFromGm);
    }

    private string _foodNarPath = "Audio/SortedByScene/EA009/Food/";

    private void OnGetMessageEventFromGm(EA009_Payload payload)
    {
        if (payload.Checksum)
        {
            PopFromZeroInstructionUI($"{payload.Narration}");
            return;
        }
        Logger.ContentTestLog($"Get Message ---- {payload.Narration}");
        switch (payload.Narration)
        {
            case nameof(EA009_HealthyFood_GameManager.GameObj.FishA):
                PopFromZeroInstructionUI("생선");
                Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Fish");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.AppleA):
                PopFromZeroInstructionUI("사과");
                Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Apple");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.MeatA):
                Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Meat");
                PopFromZeroInstructionUI("고기");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.ChickenA):
                PopFromZeroInstructionUI("닭");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Chicken");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.EggA):
                PopFromZeroInstructionUI("계란");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Egg");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.MilkA):
                PopFromZeroInstructionUI("우유");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Milk");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.CarrotA):
                PopFromZeroInstructionUI("당근");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Carrot");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.ColaA):
                PopFromZeroInstructionUI("콜라");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Cola");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.CookieA):
                PopFromZeroInstructionUI("쿠키");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Cookie");
                break;
            case nameof(EA009_HealthyFood_GameManager.GameObj.IceCreamA):
                PopFromZeroInstructionUI("아이스크림");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "IceCream");
                break;

            case nameof(EA009_HealthyFood_GameManager.GameObj.PizzaA):
                PopFromZeroInstructionUI("피자");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Pizza");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.ChocolateA):
                PopFromZeroInstructionUI("초콜릿");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Chocolate");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.CakeA):
                PopFromZeroInstructionUI("케이크");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Cake");
                break;
            
            case nameof(EA009_HealthyFood_GameManager.GameObj.DonutA):
                PopFromZeroInstructionUI("도넛");
                 Managers.Sound.Play(SoundManager.Sound.Narration,_foodNarPath + "Donut");
                break;

            case nameof(EA009_HealthyFood_GameManager.MainSeq.OnFinish):
                break;
            
            default:
          //      ChangeText("기타 나레이션" + payload.Narration + ")");
                break;
        }
    }
}