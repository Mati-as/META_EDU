using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using Random = System.Random;

public class EA009_Payload : IPayload
{
    public bool Checksum
    {
        get;
    }
    public string Narration
    {
        get;
    }

    public string CurrentCarName
    {
        get;
    }

    public EA009_Payload(string narration, bool isCustomOn = false)
    {
        Checksum = isCustomOn;
        Narration = narration;
    }
}


public class EA009_HealthyFood_GameManager : Ex_BaseGameManager
{
    public enum MainSeq
    {
        Default,
        AllFoodIntroduce,
        GoodFoodChangeToBadFood,
        BadFoodEatIntro,
        BadFoodEat_RoundA,
        BadFoodEat_RoundB,
        BadFoodEat_RoundC,
        Stomachache,
        GoodFoodChangeIntro,
        BadFoodRemoval,
        OnFinish
    }

    public enum RandomBadFoodRound
    {
        Cola,
        Cookie,
        IceCrea,
        Pizza,
        Chocolate,
        Cake,Donut,
    }
    
    private readonly Dictionary<int, Stack<GameObject>> _foodClonePool = new();
    
    private readonly Dictionary<int, GameObject> _activatedFoodOnTableMap = new();
    private readonly Dictionary<int, GameObject> allObj = new();
    private readonly Dictionary<int,Vector3> _defaultPosMap = new(); // 재생성관련
    private readonly Dictionary<int,bool> _isPosEmptyMap = new(); // 재생성관련 , true인경우 좋은음식은 여기서 생성
    
    
    private readonly Dictionary<int, GameObj> _disappearedBadFoodMap = new();
    [SerializeField]
    private bool _isDevMode;
    private float delayForDevMode = 0.0f;

    public enum GameObj
    {
        // 좋은 음식
        GoodFoodGroup,
        HealthyFoodGroup,
        FishA,
        MeatA,
        ChickenA,
        AppleA,
        EggA,
        MilkA,
        CarrotA,
        //FishB,
        //MeatB,
        //ChickenB,
        //AppleB,
        //EggB,
        //MilkB,
        //CarrotB,

        // 나쁜 음식
        ColaA,
        CookieA,
        IceCreamA,
        PizzaA,
        ChocolateA,
        CakeA,
        DonutA,
        //ColaB,
        //CookieB,
        //IceCreamB,
        //PizzaB,
        //ChocolateB,
        //CakeB,
        //DonutB
    }

    private enum BadFoodClickGameCategory
    {
        Cola,
        Cookie,
        IceCream,
        Pizza,
        Chocolate,
        Cake,
        Donut
    }

    private List<BadFoodClickGameCategory> _badFoodClickGameList = new List<BadFoodClickGameCategory>
    {
        BadFoodClickGameCategory.Cola,
        BadFoodClickGameCategory.Cookie,
        BadFoodClickGameCategory.IceCream,
        BadFoodClickGameCategory.Pizza,
        BadFoodClickGameCategory.Chocolate,
        BadFoodClickGameCategory.Cake,
        BadFoodClickGameCategory.Donut
    };

    private Dictionary<BadFoodClickGameCategory, string> _badFoodToKorean = new Dictionary<BadFoodClickGameCategory, string> {
        { BadFoodClickGameCategory.Cola, "콜라" },
        { BadFoodClickGameCategory.Cookie, "쿠키" },
        { BadFoodClickGameCategory.IceCream, "아이스크림" },
        { BadFoodClickGameCategory.Pizza, "피자" },
        { BadFoodClickGameCategory.Chocolate, "초콜릿" },
        { BadFoodClickGameCategory.Cake, "케이크" },
        { BadFoodClickGameCategory.Donut, "도넛" }
    };
    public enum SeqNar
    {
        HungryTimeToEat,
        ChangeToGoodFood,
        Delicious
    }


    private MainSeq _currentMainSeq = MainSeq.Default;
    private Sequence _currentMasterSequence;
    private string _currentBadFoodToClick;
    private int currentBadFoodClickedCount;
    private const int BAD_FOOD_CLICK_TO_COUNT = 4;
    private MainSeq currentMainSeq
    {
        get
        {
            return _currentMainSeq;
        }
        set
        {
            _currentMainSeq = value;

            Messenger.Default.Publish(new EA009_Payload(_currentMainSeq.ToString()));
            Logger.ContentTestLog($"Current Sequence: {currentMainSeq.ToString()}");

            //common Init--------------------------------------
            isBadFoodClickable = false; 
            mainAnimator.SetInteger(SeqNum, 0 );
            switch (value)
            {
                case MainSeq.Default:
                    OnDefault();
                    break;
                case MainSeq.AllFoodIntroduce:
                    DOVirtual.DelayedCall(_isDevMode? delayForDevMode :3f, () =>
                    {
                        OnAllFoodIntroduce();
                    });
                    break;
                case MainSeq.GoodFoodChangeToBadFood:
                    DOVirtual.DelayedCall(_isDevMode? delayForDevMode :3f, () =>
                    {
                        OnGoodFoodChangeToBadFood();
                    });
                    break;
                case MainSeq.BadFoodEatIntro:
                   // Messenger.Default.Publish(new EA009_Payload("",true));
                    OnBadFoodEatIntro();
                    break;
                case MainSeq.BadFoodEat_RoundA:
                    OnBadFoodEat_RoundA();
                    PlayNarrationSound(_currentBadFoodToClick);
                    break;
                case MainSeq.BadFoodEat_RoundB:
                    OnBadFoodEat_RoundB();
                    PlayNarrationSound(_currentBadFoodToClick);
                    break;
                case MainSeq.BadFoodEat_RoundC:
                    OnBadFoodEat_RoundC();
                 
                    PlayNarrationSound(_currentBadFoodToClick);
                    break;
                case MainSeq.Stomachache:
                    Messenger.Default.Publish(new EA009_Payload($"나쁜음식을 너무 많이먹어\n배가 아픈 것 같아요",true));
                    Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "HavingStomachache");
                    mainAnimator.SetInteger(SeqNum, 1 );
                    OnStomachache();
                    break;
                case MainSeq.GoodFoodChangeIntro:
                    OnBadFoodRemovalIntro();
                    break;
                case MainSeq.BadFoodRemoval:
                    Messenger.Default.Publish(new EA009_Payload("몸에 좋지 않은 음식을 전부 터치해주세요!",true));
                    Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "LetsHelpEatGoodFood");
                    OnBadFoodRemoval();
                    break;
                case MainSeq.OnFinish:
                    Messenger.Default.Publish(new EA009_Payload("우리 친구들도 몸에 좋은 음식을 먹고 튼튼해져요!",true));
                    Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "LetsEatGoodFood");
                    int finish = 123;
                    mainAnimator.SetInteger(SeqNum, finish );
                    foreach (var key in allObj.Keys.ToArray())
                    {
                        allObj[key].transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutBack);
                        allObj[key].SetActive(false);
                    }
                    break;
            }
        }
    }

    private bool _isIntroducePossible = false;
    private Tween _introduceTween;

    private void IntroduceItselfByClick()
    {
        
        if(!_isIntroducePossible) return;
        
        
        foreach (var hit in GameManager_Hits)
        {
            int id = hit.transform.gameObject.GetInstanceID();

            if (_tfIdToEnumMap.ContainsKey(id))
            {
                Messenger.Default.Publish(new EA009_Payload(hit.transform.gameObject.name));
                
                _introduceTween = DOVirtual.DelayedCall(1f, () =>
                {
                    _isIntroducePossible = true;
                });

                _introduceTween.OnKill(() =>
                {
                    _isIntroducePossible = false;
                });
            }
           
        }
    }

    /// <summary>
    /// 안좋은음식 종류 식별 및 재생
    /// </summary>
    /// <param name="fruitName"></param>
    private void PlayNarrationSound(string fruitName)
    {

        switch (fruitName)
        {
            case "콜라":
                Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "EatCola");
                Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}를 전부 터치해주세요",true));
                break;
            case "쿠키":
                Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "EatCookie");
                Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}를 전부 터치해주세요",true));
                break;
            case "아이스크림":
                Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "EatIceCream");
                Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}을 전부 터치해주세요",true));
                break;
            case "피자":
                Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "EatPizza");
                Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}를 전부 터치해주세요",true));
                break;
            case "초콜릿":
                Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "EatChocolate");
                Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}을 전부 터치해주세요",true));
                break;
            case "케이크":
                Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "EatCake");
                Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}를 전부 터치해주세요",true));
                break;
            case "도넛":
                Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "EatDonut");
                Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}을 전부 터치해주세요",true));
                break;
            
        }
    }

    private readonly int COUNT_OF_FOOD_TO_CHANGE = 7;
    private int _currentCountOfFoodChanged;


    private readonly Dictionary<int, GameObj> _idToFoodMap = new();
    private readonly Dictionary<GameObj, int> clickCountMap = new(); // 사라지는 애니메이션, 흔들리는 애니메이션구분
    private readonly Dictionary<GameObj, bool> isClickedMap = new(); // 사라지는 애니메이션, 흔들리는 애니메이션구분
    private const int CLICK_COUNT_TO_GET_RID_OF_BAD_FOOD = 3;


    private Dictionary<int, GameObj> transformID = new();


    private Ease _disappearAnimEase = Ease.InOutSine;
    private readonly Ease   _appearAnimEase = Ease.InOutSine;
    private Animator _mainCameraAnimator;

    private bool _isFoodClickable;
   // private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    
    private string EA009soundPath = "Audio/SortedByScene/EA009/";
    private string _foodNarrationPath = "Audio/SortedByScene/EA009/Narration/";

    public GameObject PoolRoot
    {
        get
        {
            var root = GameObject.Find("@PoolRoot");
            if (root == null) root = new GameObject { name = "@PoolRoot" };

            
            root.gameObject.transform.localScale = Vector3.one*0.2772007f;
            return root;
        }
    }

    private void SetFoodPool()
    {
        
        for (int objEnum = (int)GameObj.FishA; objEnum <= (int)GameObj.DonutA; objEnum++)
        {
            
            allObj.Add(GetObject(objEnum).transform.GetInstanceID(), GetObject(objEnum));
            _defaultSizeMap.TryAdd(GetObject(objEnum).transform.GetInstanceID(), GetObject(objEnum).transform.localScale);
            _foodClonePool.Add(objEnum, new Stack<GameObject>());
       
            _defaultPosMap.Add(objEnum, GetObject(objEnum).transform.position);
            _isPosEmptyMap.Add(objEnum, false);
            
            
            for (int count = 0; count < 10; count++)
            {
                var instantiatedFood = Instantiate(GetObject((int)objEnum), PoolRoot.transform, true);
                instantiatedFood.name = ((GameObj)objEnum).ToString() +$"{objEnum}".ToString();
                _foodClonePool[objEnum].Push(instantiatedFood);
                allObj.Add(instantiatedFood.transform.GetInstanceID(), instantiatedFood);
                _defaultSizeMap.TryAdd(instantiatedFood.transform.GetInstanceID(), instantiatedFood.transform.localScale);
                instantiatedFood.SetActive(false);
                
            }
        }
   
    }


    protected override void Init()
    {
        PoolRoot.transform.SetParent(gameObject.transform);
        psResourcePath = "Runtime/EA009/Fx_Click";
        SHADOW_MAX_DISTANCE = 60;
        DOTween.SetTweensCapacity(700,1000);
        base.Init();
        BindObject(typeof(GameObj));
        SetFoodPool();

        currentMainSeq = MainSeq.Default;
        _currentMasterSequence = DOTween.Sequence();
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        currentMainSeq = MainSeq.AllFoodIntroduce;
        DOVirtual.DelayedCall(1.25f, () =>
        {
            Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "Hungry");
            Messenger.Default.Publish(new EA009_Payload("어떤 음식을 먹을까요?",true));
        });

    }


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;


        switch (currentMainSeq)
        {
            case MainSeq.Default:
                break;
            case MainSeq.AllFoodIntroduce:
                IntroduceItselfByClick();
                break;
            case MainSeq.GoodFoodChangeToBadFood:
                break;
            case MainSeq.BadFoodEatIntro:
                break;
            case MainSeq.BadFoodEat_RoundA:
                OnRaySyncOnBadFoodEat();
                break;
            case MainSeq.BadFoodEat_RoundB:
                OnRaySyncOnBadFoodEat();
                break;
            case MainSeq.BadFoodEat_RoundC:
                OnRaySyncOnBadFoodEat();
                break;
            case MainSeq.Stomachache:
                break;
            case MainSeq.GoodFoodChangeIntro:
                
                break;
            case MainSeq.BadFoodRemoval:
                OnRaySyncOnBadFoodRemoval();
                break;
            case MainSeq.OnFinish:
                break;
        }
        
     

        
    }

    private void OnDefault()
    {
        for (int i = (int)GameObj.FishA; i <= (int)GameObj.DonutA; i++)
        {
            GetObject(i).transform.localScale = UnityEngine.Vector3.zero;
            _activatedFoodOnTableMap.Add(GetObject(i).transform.GetInstanceID(),GetObject(i));
        }
    }
    
    #region Animation // AllFoodIntroduce, 좋은음식 나쁜음식 모두 표출 및 하나씩 읽어주는 파트
    /// <summary>
    /// 1. Increase size with Pop Anim
    /// 2. Narrate each food 
    /// </summary>
    private float introDuration=2.5f;
    private void OnAllFoodIntroduce()
    {
        _currentMasterSequence?.Kill();
        _currentMasterSequence = DOTween.Sequence();
// 🔹 masterInitSequence 내부 구성
        for (int i = (int)GameObj.FishA; i <= (int)GameObj.DonutA; i++)
        {
            int localIndex = i;
            var obj = GetObject(localIndex).transform;
            obj.localScale = Vector3.zero;

       
            _currentMasterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex] * 1.2f, 0.1f).SetEase(Ease.OutBack).OnStart(
                () =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect,"Audio/SortedByScene/EA009/fxA");
                }));
            _currentMasterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex], _isDevMode? delayForDevMode :0.075f));
            _currentMasterSequence.AppendInterval(0.1f);
       
    
        }

// 🔹 Init 끝나고 → masterSequence 실행
        _currentMasterSequence.OnComplete(() =>
        {
            Logger.ContentTestLog("Init 애니메이션 완료, 다음 단계 실행");
            _currentMasterSequence.Play();
        });

// 🔹 masterSequence 구성 (차례대로 메시지 + 애니메이션)
        for (int i = (int)GameObj.FishA; i <= (int)GameObj.CarrotA; i++)
        {
            int localIndex = i;
            var obj = GetObject(localIndex).transform;

            _currentMasterSequence.AppendCallback(() =>
            {
                Messenger.Default.Publish(new EA009_Payload(obj.name));
                Logger.ContentTestLog($"Messenger: {obj.name}");
            });

            _currentMasterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex] * 1.4f, _isDevMode? delayForDevMode :0.15f).SetEase(Ease.InOutBack));
            _currentMasterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex], _isDevMode? delayForDevMode :0.15f).SetEase(Ease.InOutBack));
            _currentMasterSequence.AppendInterval(0.75f);
        }
        
        for (int i = (int)GameObj.ColaA; i <= (int)GameObj.DonutA; i++)
        {
            int localIndex = i;
            var obj = GetObject(localIndex).transform;

            _currentMasterSequence.AppendCallback(() =>
            {
                Messenger.Default.Publish(new EA009_Payload(obj.name));
                //Logger.ContentTestLog($"Messenger: {obj.name}");
            });

            _currentMasterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex] * 1.4f, _isDevMode? delayForDevMode :0.15f).SetEase(Ease.InOutBack));
            _currentMasterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex], _isDevMode? delayForDevMode :0.15f).SetEase(Ease.InOutBack));
            _currentMasterSequence.AppendInterval(0.75f);
        }
        
        
        _currentMasterSequence.AppendCallback(() =>
        {
            Messenger.Default.Publish(new EA009_Payload(MainSeq.OnFinish.ToString()));
            Logger.ContentTestLog($"Messenger: {MainSeq.OnFinish.ToString()}");

            currentMainSeq = MainSeq.GoodFoodChangeToBadFood;
        });
    }

    #endregion


    #region Animation //  GoodFoodChangeToBadFood, 좋은음식-> 나쁜음식으로 바뀌는 파트

    private readonly List<GameObj> goodFoodList = new() {
        GameObj.FishA, GameObj.MeatA, GameObj.ChickenA, GameObj.AppleA,
        GameObj.EggA, GameObj.MilkA, GameObj.CarrotA
        //  GameObj.FishB, GameObj.MeatB, GameObj.ChickenB, GameObj.AppleB,
        // GameObj.EggB, GameObj.MilkB, GameObj.CarrotB
    };

    private readonly List<GameObj> badFoodList = new() {
        GameObj.ColaA, GameObj.CookieA, GameObj.IceCreamA, GameObj.PizzaA,
        GameObj.ChocolateA, GameObj.CakeA, GameObj.DonutA
        // GameObj.ColaB, GameObj.CookieB, GameObj.IceCreamB, GameObj.PizzaB,
        //  GameObj.ChocolateB, GameObj.CakeB, GameObj.DonutA
    };

    
    private void OnGoodFoodChangeToBadFood()
    {
        _currentMasterSequence?.Kill();
        _currentMasterSequence = DOTween.Sequence();


        // 좋은 음식과 대응하는 나쁜 음식 인덱스 (1:1 대응, 순서대로)

        foreach (var index in goodFoodList) _isPosEmptyMap[(int)index] = true;

        Managers.Sound.Play(SoundManager.Sound.Effect, EA009soundPath + "FoodJump");

        for (int i = 0; i < goodFoodList.Count; i++)
        {
            int goodIndex = (int)goodFoodList[i];
            int badIndex = (int)badFoodList[i];

            var goodObj = GetObject(goodIndex).transform;
            var badPrefab = GetObject(badIndex); // Prefab or sample
            var badParent = PoolRoot.transform;


            _currentMasterSequence.AppendCallback(() =>
            {
            });

            // 1. 좋은 음식 작아지기
            _currentMasterSequence.Join(goodObj.DOScale(Vector3.zero, _isDevMode ? delayForDevMode : 0.15f)
                .SetEase(_disappearAnimEase).OnStart(() =>
                {
                    _activatedFoodOnTableMap.Remove(goodObj.GetInstanceID());
                    
                    Managers.Sound.Play(SoundManager.Sound.Effect, EA009soundPath + "fxA");
                }));

            // 2. 같은 위치에 나쁜 음식 생성 + 동시에 커지기
            _currentMasterSequence.Join(DOVirtual.DelayedCall(0f, () =>
            {
                GameObject badClone = null;

                if (_foodClonePool.ContainsKey(badIndex) && _foodClonePool[badIndex].Count > 0)
                {
                    badClone = _foodClonePool[badIndex].Pop();
                    badClone.SetActive(true);
                }
                else
                {
                    Logger.ContentTestLog("복제본 부족, 다시생성 ");
                    badClone = Instantiate(badPrefab, badParent);
                }

                badClone.transform.position = goodObj.position;
                badClone.transform.localScale = Vector3.zero;

                badClone.transform.DOLocalRotate(
                    new Vector3(badClone.transform.eulerAngles.x, UnityEngine.Random.Range(-360, 360),
                        badClone.transform.eulerAngles.z),
                    _isDevMode ? delayForDevMode : 0.15f);

                badClone.transform.DOScale(_defaultSizeMap[badIndex], _isDevMode ? delayForDevMode : 0.15f)
                    .SetEase(_appearAnimEase);
                
                _activatedFoodOnTableMap.Add(badClone.transform.GetInstanceID(), badClone);
            }));


            _currentMasterSequence.AppendInterval(0.0f);
        }

        // 완료 후 다음 단계로
        _currentMasterSequence.AppendCallback(() =>
        {
            Logger.ContentTestLog("모든 음식이 나쁜 음식으로 변신 완료!");
            Managers.Sound.Play(SoundManager.Sound.Narration, _foodNarrationPath + "LetsEatAllBadFood");
            currentMainSeq = MainSeq.BadFoodEatIntro;
        });
    }

    #endregion

    #region Animation // BadfoodeatIntro, 좋은음식-> 나쁜음식으로 바뀌는 파트

    private void OnBadFoodEatIntro()
    {
        DOVirtual.DelayedCall(6f, () =>
        {
            currentMainSeq = MainSeq.BadFoodEat_RoundA;
        });
    }

    #endregion

    #region Game // 첫번째 나쁜음식 클릭해서 먹기 파트 -----------------------
    private BadFoodClickGameCategory currentBadFoodClickGameCategory;
    private string currentBadFoodClickGameCategoryKorean
    {
        get
        {
            if (_badFoodToKorean.TryGetValue(currentBadFoodClickGameCategory, out var value))
            {
                return value;
            }
            return string.Empty;
        }
    }
    
    private bool isBadFoodClickable = false;
    private void OnBadFoodEat_RoundA()
    {  
     
        var badFoodToEat = UnityEngine.Random.Range(0, _badFoodClickGameList.Count);
        currentBadFoodClickGameCategory =  _badFoodClickGameList[badFoodToEat];
        _badFoodClickGameList.Remove((BadFoodClickGameCategory)badFoodToEat);
        _currentBadFoodToClick = currentBadFoodClickGameCategoryKorean;

        OnBadFoodEatRoundInit();
    }

    private void OnBadFoodEatRoundInit()
    {
        Logger.ContentTestLog(" 나쁜음식 클릭하기 시작---RoundInit 완료");
        
        
        foreach (var key in allObj.Keys.ToArray())
        {
      
            allObj[key].transform.DOScale(allObj[key].transform.localScale*1.1f, 0.1f).SetEase(Ease.OutBack);
            allObj[key].transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.OutBack);
        }

        foreach (var key in _isPosEmptyMap.Keys.ToArray())
        {
            _isPosEmptyMap[key] = true;
        }
        
        foreach (var key in _isPosEmptyMap.Keys.ToArray())
        {
            if (!_isPosEmptyMap[key]) continue;

            if (!_defaultPosMap.TryGetValue(key, out var spawnPos))
            {
                Logger.ContentTestLog($"[ERROR] 위치 정보 없음: {key}");
                continue;
            }

            var randomBad = badFoodList[UnityEngine.Random.Range(0, badFoodList.Count)];
            int badIndex = (int)randomBad;
            GameObject badClone = null;

            if (_foodClonePool.ContainsKey(badIndex) && _foodClonePool[badIndex].Count > 0)
            {
                badClone = _foodClonePool[badIndex].Pop();
                badClone.SetActive(true);
            }
            else
            {
                badClone = Instantiate(GetObject(badIndex), PoolRoot.transform, true);
            }

            badClone.transform.position = spawnPos;
            badClone.transform.localScale = Vector3.zero;

            //badClone.name = $"BadClone_{badIndex}_{key}";
            allObj[badClone.transform.GetInstanceID()] = badClone;
            _tfIdToEnumMap[badClone.transform.GetInstanceID()] = badIndex;
            _clickedCountMap[badClone.transform.GetInstanceID()] = 0;

            badClone.transform.DOScale(_defaultSizeMap[badClone.transform.GetInstanceID()], 0.25f);
            _isClickableMap[badClone.transform.GetInstanceID()] = true;
            _isPosEmptyMap[key] = false;

        }
        
        
        
        
        

        foreach (int key in allObj.Keys.ToArray())
        {
            if (allObj[key].name.Contains(currentBadFoodClickGameCategory.ToString()))
            {
                int thisObjTransID = allObj[key].transform.GetInstanceID();

                if (_defaultSizeMap.TryGetValue(thisObjTransID, out var size))
                {
                    _badFoodClickRelatedSeq.TryAdd(thisObjTransID, DOTween.Sequence());
                    _badFoodClickRelatedSeq[thisObjTransID]?.Kill();
                    _badFoodClickRelatedSeq[thisObjTransID] = DOTween.Sequence();

                    _badFoodClickRelatedSeq[thisObjTransID].Append(allObj[key].transform
                        .DOScale(_defaultSizeMap[thisObjTransID] * 1.5f, _isDevMode? delayForDevMode :0.15f).SetEase(_appearAnimEase));
                    _badFoodClickRelatedSeq[thisObjTransID].AppendInterval(0.21f);
                    _badFoodClickRelatedSeq[thisObjTransID].Append(allObj[key].transform
                        .DOScale(_defaultSizeMap[thisObjTransID] * 0.75f, _isDevMode? delayForDevMode :0.15f).SetEase(_appearAnimEase));
                    _badFoodClickRelatedSeq[thisObjTransID].SetLoops(150, LoopType.Yoyo);
                }
                else
                {
                    Logger.ContentTestLog($"[ERROR] _defaultSizeMap에 없는 Transform ID: {allObj[key].name} : {thisObjTransID}");
                }
            
           
            }
            
  
        }


        foreach (var key in _isClickableMap.Keys.ToArray())
        {
            currentBadFoodClickedCount = 0;
            _isClickableMap[key] = true;
        }
        Logger.ContentTestLog($" 현재 클릭할 나쁜음식 {currentBadFoodClickGameCategory}");

        DOVirtual.DelayedCall(1f, () =>
        {
            isBadFoodClickable = true;
        });
      
    }
    private Dictionary<int, Sequence> _badFoodClickRelatedSeq = new Dictionary<int, Sequence>();
    private Dictionary<int, int> _clickedCountMap = new();
    
private Dictionary<int, Sequence> _badFoodClickMoveSeqMap = new(); // 도망 애니메이션 전용

private void OnRaySyncOnBadFoodEat()
{
    if (!isBadFoodClickable)
    {
        Logger.ContentTestLog($"현재 클릭 불가 --{currentBadFoodClickGameCategory}");
        return;
    }

    foreach (var hit in GameManager_Hits)
    {
        
        PlayParticleEffect(hit.point);
       // Logger.Log($"clickedName {hit.transform.name}, clickedID {hit.transform.GetInstanceID()}");
        
        if (!hit.transform.gameObject.name.Contains(currentBadFoodClickGameCategory.ToString()))
        {
            Logger.ContentTestLog($" 해당음식이 아님! --{hit.transform.gameObject.name}");
            continue;
        }

        int clickedFoodID = hit.transform.GetInstanceID();

        _isClickableMap.TryAdd(clickedFoodID, true);
        _clickedCountMap.TryAdd(clickedFoodID, 0);

        if (!_isClickableMap[clickedFoodID])
        {
            Logger.ContentTestLog($" 이미 클릭됨 {currentBadFoodClickGameCategory}");
            continue;
        }

        char RandomChar = (char)UnityEngine.Random.Range('A', 'C' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect,EA009soundPath+"fx" + RandomChar, 0.3f);
        _clickedCountMap[clickedFoodID]++;

        // 🔁 도망가기 처리 (1,2번째 클릭 시)
        if (_clickedCountMap[clickedFoodID] < 3)
        {
            // 기존 도망 시퀀스 있으면 중지
            if (_badFoodClickMoveSeqMap.TryGetValue(clickedFoodID, out var existingSeq))
            {
                existingSeq.Kill();
            }

            Vector3 currentPos = hit.transform.position;

            var candidatePositions = _defaultPosMap
                .Where(pair => pair.Value != currentPos) // 동일 위치 제외
                .OrderBy(pos => Vector3.Distance(currentPos, pos.Value))
                .Take(5)
                .ToList();

            if (candidatePositions.Count > 0)
            {
                var newTarget = candidatePositions[UnityEngine.Random.Range(0, candidatePositions.Count)].Value;

                var seq = DOTween.Sequence();
                seq.Append(hit.transform.DOMove(newTarget, 0.3f).SetEase(Ease.InOutBack));
                seq.OnKill(() => Logger.ContentTestLog($"[SEQ] 도망 애니메이션 종료: {hit.transform.name}"));

                _badFoodClickMoveSeqMap[clickedFoodID] = seq;
            }

            Logger.ContentTestLog($"도망 처리됨 ({_clickedCountMap[clickedFoodID]}회 클릭): {hit.transform.name}");
            continue;
        }

        // 🔴 3번째 클릭 → 사라지기 처리
        _badFoodClickRelatedSeq[clickedFoodID]?.Kill();
        _badFoodClickMoveSeqMap[clickedFoodID]?.Kill(); // 도망 시퀀스도 정리

        hit.transform.DOScale(Vector3.zero, 0.15f).SetEase(_disappearAnimEase).OnStart(() =>
        {
            Managers.Sound.Play(SoundManager.Sound.Effect,"Audio/SortedByScene/EA009/OnBadFoodDisappear");
        });
        _isClickableMap[clickedFoodID] = false;

        if (_tfIdToEnumMap.TryGetValue(clickedFoodID, out var foodEnum))
        {
            int foodIndex = (int)foodEnum;
            _isPosEmptyMap[foodIndex] = true;
            _disappearedBadFoodMap[foodIndex] = (GameObj)foodEnum;
        }

        Logger.ContentTestLog($" 나쁜음식 찾음! --현재 클릭할 나쁜음식 {currentBadFoodClickGameCategory}");

        currentBadFoodClickedCount++;

        if (currentBadFoodClickedCount >= BAD_FOOD_CLICK_TO_COUNT)
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, EA009soundPath + "OnFinishEatingBadFood");
            DOVirtual.DelayedCall(2f, () =>
            {
                currentMainSeq = currentMainSeq != MainSeq.BadFoodEat_RoundC
                    ? currentMainSeq + 1
                    : MainSeq.Stomachache;

                currentBadFoodClickedCount = 0;
            });

            isBadFoodClickable = false;
            Messenger.Default.Publish(
                new EA009_Payload($"{currentBadFoodClickGameCategoryKorean} 다 먹었어요!", true));
            Logger.ContentTestLog($" 클릭 완료! 다음 라운드로 넘어감 {currentBadFoodClickGameCategory}");
        }
        else
        {
            Messenger.Default.Publish(new EA009_Payload(
                $"{currentBadFoodClickGameCategoryKorean} X {currentBadFoodClickedCount}", true));
            Logger.ContentTestLog($" 나쁜음식 클릭 카운트 {currentBadFoodClickedCount}");
        }
    }
}
    #endregion

    #region Game // BadFoodEat_Chocolate, 나쁜음식먹기

    private void OnBadFoodEat_RoundB()
    {
        var badFoodToEat = UnityEngine.Random.Range(0, _badFoodClickGameList.Count);
        currentBadFoodClickGameCategory =  _badFoodClickGameList[badFoodToEat];
        _badFoodClickGameList.Remove((BadFoodClickGameCategory)badFoodToEat);
       
      
        _currentBadFoodToClick = currentBadFoodClickGameCategoryKorean.ToString();

        OnBadFoodEatRoundInit();
    }

    #endregion

    #region Game // BadFoodEat_IceCream, 나쁜음식먹기-아이스크림

    private void OnBadFoodEat_RoundC()
    {
        var badFoodToEat = UnityEngine.Random.Range(0, _badFoodClickGameList.Count);
        currentBadFoodClickGameCategory =  _badFoodClickGameList[badFoodToEat];
        _badFoodClickGameList.Remove((BadFoodClickGameCategory)badFoodToEat);
       
      
        _currentBadFoodToClick = currentBadFoodClickGameCategoryKorean.ToString();

        OnBadFoodEatRoundInit();
    }
    #endregion


    #region Animation //Stomachache, 배탈

    
    private void OnStomachache()
    {
       
        
        var animationDurationOnStomachache = 3.0f;
        DOVirtual.DelayedCall(6.5f, () =>
        {
            foreach (var key in _badFoodClickRelatedSeq.Keys.ToArray()) _badFoodClickRelatedSeq[key]?.Kill();
            currentMainSeq = MainSeq.GoodFoodChangeIntro;
        });
       
    }

    #endregion


    #region Animation // GoodFoodIntro, 좋은음식 

   private void OnBadFoodRemovalIntro()
{
    float animationDuration = 3.0f;
    
    _badFoodClickMoveSeqMap.Clear();
    _badFoodClickMoveSeqMap = new Dictionary<int, Sequence>();
    
    List<int> placedFoodIds = new();
    
    
    //재사용주의 



    foreach (var key in _isPosEmptyMap.Keys.ToArray())
    {
        if (!_isPosEmptyMap[key]) continue;

        if (!_defaultPosMap.TryGetValue(key, out var spawnPos))
        {
            Logger.ContentTestLog($"[ERROR] 위치 정보 없음: {key}");
            continue;
        }

        var randomBad = badFoodList[UnityEngine.Random.Range(0, badFoodList.Count)];
        int badIndex = (int)randomBad;
        GameObject badClone = null;

        if (_foodClonePool.ContainsKey(badIndex) && _foodClonePool[badIndex].Count > 0)
        {
            badClone = _foodClonePool[badIndex].Pop();
            badClone.SetActive(true);
        }
        else
        {
            badClone = Instantiate(GetObject(badIndex), PoolRoot.transform, true);
        }

        badClone.transform.position = spawnPos;
        badClone.transform.localScale = Vector3.zero;

       //badClone.name = $"BadClone_{badIndex}_{key}";
        allObj[badClone.transform.GetInstanceID()] = badClone;
        _tfIdToEnumMap[badClone.transform.GetInstanceID()] = badIndex;
        _clickedCountMap[badClone.transform.GetInstanceID()] = 0;

        badClone.transform.DOScale(_defaultSizeMap[badClone.transform.GetInstanceID()], 0.25f);
        _isClickableMap[badClone.transform.GetInstanceID()] = true;
        _isPosEmptyMap[key] = false;

        placedFoodIds.Add(badClone.transform.GetInstanceID());
       
    }

    // 무작위 절반은 상시움직임 부여
    int moveCount = placedFoodIds.Count / 2;
    var jumpers = placedFoodIds.OrderBy(x => UnityEngine.Random.value).Take(moveCount).ToList();

    foreach (var id in jumpers)
    {
        var obj = allObj[id];
        Sequence loopSeq = DOTween.Sequence();
        loopSeq.AppendInterval(1f);
        loopSeq.AppendCallback(() =>
        {
            var pos = _defaultPosMap.Values.OrderBy(p => UnityEngine.Random.value).First();
            obj.transform.DOJump(pos, 0.5f, 1, 0.5f).OnStart(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "SortedByScene/EA009/FoodJump");
            });
        });
        loopSeq.SetLoops(50);
        _badFoodClickMoveSeqMap.TryAdd(id, loopSeq);
    }

    DOVirtual.DelayedCall(animationDuration, () => currentMainSeq = MainSeq.BadFoodRemoval);
}


    #endregion

    #region Game // BadFoodRemoval, 나쁜음식 제거하기 

    private void OnBadFoodRemoval()
    {
        
        _badFoodClickGameList = new List<BadFoodClickGameCategory>
        {
            BadFoodClickGameCategory.Cola,
            BadFoodClickGameCategory.Cookie,
            BadFoodClickGameCategory.IceCream,
            BadFoodClickGameCategory.Pizza,
            BadFoodClickGameCategory.Chocolate,
            BadFoodClickGameCategory.Cake,
            BadFoodClickGameCategory.Donut
        };
    }
private int TARGET_BAD_FOOD_COUNT = 28;
private int currentRemovedBadFoodCunt = 0;
private static readonly int SeqNum = Animator.StringToHash("seqNum");

private void OnRaySyncOnBadFoodRemoval()
{
    HashSet<Vector3> usedGoodFoodPositions = new HashSet<Vector3>();

    foreach (var hit in GameManager_Hits)
    {
        var id = hit.transform.GetInstanceID();
        _isClickableMap.TryAdd(id, true);

        string hitName = hit.transform.gameObject.name;

        // 이름 기반으로 badFood인지 확인
        bool isBadFood = _badFoodClickGameList.Any(bad => hitName.Contains(bad.ToString()));
        if (!isBadFood)
        {
            Logger.ContentTestLog($"[SKIP] 클릭된 객체는 badFood가 아님: {hitName}");
            continue;
        }

        _clickedCountMap.TryAdd(id, 0);
        _clickedCountMap[id]++;

        if (_clickedCountMap[id] < 3)
        {
            // 1,2회 클릭 → 랜덤 점프
            if (_badFoodClickMoveSeqMap.TryGetValue(id, out var moveSeq))
            {
                moveSeq.Kill();
                PlayParticleEffect(hit.point);
                var target = _defaultPosMap.Values
                    .Where(p => Vector3.Distance(p, hit.transform.position) > 0.5f)
                    .OrderBy(p => UnityEngine.Random.value)
                    .FirstOrDefault();

                if (target != Vector3.zero)
                {
                        
                    Managers.Sound.Play(SoundManager.Sound.Effect, EA009soundPath + "fxA");
                    Sequence newSeq = DOTween.Sequence();
                    newSeq.Append(hit.transform.DOJump(target, 0.8f, 1, 0.5f));
                    _badFoodClickMoveSeqMap[id] = newSeq;
                }
            }
        }
        else
        {
            // ✅ 3회 클릭 → 제거 + 좋은 음식 랜덤 생성
            _isClickableMap[id] = false;
            if (_badFoodClickMoveSeqMap.ContainsKey(id))
                _badFoodClickMoveSeqMap[id]?.Kill();
            PlayParticleEffect(hit.point);
            if (_tfIdToEnumMap.TryGetValue(id, out var foodEnum))
            {
                int enumIndex = (int)foodEnum;
                _isPosEmptyMap[enumIndex] = true;
                _disappearedBadFoodMap[enumIndex] = (GameObj)foodEnum;

                //if (_defaultPosMap.TryGetValue(enumIndex, out var pos))
                //{
                    //if (!usedGoodFoodPositions.Contains(pos) && _isPosEmptyMap[enumIndex]) // 추가된 조건
                  //  {
                        Managers.Sound.Play(SoundManager.Sound.Effect, EA009soundPath + "OnChangeToGoodFood");
                        //usedGoodFoodPositions.Add(pos);
                        _isPosEmptyMap[enumIndex] = false; // 위치 사용됨

                        GameObj[] goodFoods = new GameObj[]
                        {
                            GameObj.FishA, GameObj.MeatA, GameObj.ChickenA, GameObj.AppleA,
                            GameObj.EggA, GameObj.MilkA, GameObj.CarrotA
                        };
                        GameObj randomGood = goodFoods[UnityEngine.Random.Range(0, goodFoods.Length)];

                        GameObject good = Instantiate(GetObject((int)randomGood), PoolRoot.transform, true);
                        good.name = $"Good_{randomGood}_{id}";
                        good.transform.position = hit.transform.position;
                        good.transform.localScale = Vector3.zero;

                        good.transform.DOScale(_defaultSizeMap[(int)randomGood], 0.3f).SetEase(Ease.OutBack)
                            .OnStart(() =>
                            {
                                Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/SortedByScene/EA009/fxC");
                            });

                        allObj[good.transform.GetInstanceID()] = good;

                        currentRemovedBadFoodCunt++;
                        if (currentRemovedBadFoodCunt >= TARGET_BAD_FOOD_COUNT)
                        {
                            Managers.Sound.Play(SoundManager.Sound.Effect, EA009soundPath + "OnFinishEatingBadFood");
                            currentMainSeq = MainSeq.OnFinish;
                            currentRemovedBadFoodCunt = 0;
                        }
                //    }
                    else
                    {
                      //  Logger.ContentTestLog($"[SKIP] 위치 중복 방지로 좋은 음식 생성 안함: {pos}");
                    }
              //  }
            }

            hit.transform.DOScale(Vector3.zero, 0.2f).OnStart(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect,"Audio/SortedByScene/EA009/OnBadFoodDisappear");
            });
        }
    }
}



    private void BadFoodRemovalFinished()
    {
    }

    #endregion
}