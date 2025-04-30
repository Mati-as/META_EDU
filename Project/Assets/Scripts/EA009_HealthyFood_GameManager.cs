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
        FishB,
        MeatB,
        ChickenB,
        AppleB,
        EggB,
        MilkB,
        CarrotB,

        // 나쁜 음식
        ColaA,
        CookieA,
        IceCreamA,
        PizzaA,
        ChocolateA,
        CakeA,
        DonutA,
        ColaB,
        CookieB,
        IceCreamB,
        PizzaB,
        ChocolateB,
        CakeB,
        DonutB
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
                    Messenger.Default.Publish(new EA009_Payload("(몸에 전부 안좋은 음식으로 바뀌었어요)!",true));
                    OnBadFoodEatIntro();
                    break;
                case MainSeq.BadFoodEat_RoundA:
                    OnBadFoodEat_RoundA();
                    Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}을 전부 터치해주세요",true));
                    break;
                case MainSeq.BadFoodEat_RoundB:
                    OnBadFoodEat_RoundB();
                    Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}을 전부 터치해주세요",true));
                    break;
                case MainSeq.BadFoodEat_RoundC:
                    OnBadFoodEat_RoundC();
                    Messenger.Default.Publish(new EA009_Payload($"{_currentBadFoodToClick}을 전부 터치해주세요",true));
                    break;
                case MainSeq.Stomachache:
                    Messenger.Default.Publish(new EA009_Payload($"나쁜음식을 너무 많이먹어\n배가 아픈 것 같아요",true));
                    OnStomachache();
                    break;
                case MainSeq.GoodFoodChangeIntro:
                    OnGoodFoodChangeIntro();
                    break;
                case MainSeq.BadFoodRemoval:
                    OnBadFoodRemoval();
                    break;
                case MainSeq.OnFinish:
                    OnBadFoodEat_Candy_Finished();
                    break;
            }
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
    private readonly Ease _appearAnimEase = Ease.InOutSine;
    private Animator _mainCameraAnimator;

    private bool _isFoodClickable;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

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
        
        for (int objEnum = (int)GameObj.FishA; objEnum <= (int)GameObj.DonutB; objEnum++)
        {
            
            allObj.Add(GetObject(objEnum).transform.GetInstanceID(), GetObject(objEnum));
            _defaultSizeMap.TryAdd(GetObject(objEnum).transform.GetInstanceID(), GetObject(objEnum).transform.localScale);
            _foodClonePool.Add(objEnum, new Stack<GameObject>());
       
            _defaultPosMap.Add(objEnum, GetObject(objEnum).transform.position);
            _isPosEmptyMap.Add(objEnum, false);
            
            
            for (int count = 0; count < 10; count++)
            {
                var instantiatedFood = Instantiate(GetObject((int)objEnum), PoolRoot.transform, true);
                instantiatedFood.name = ((GameObj)objEnum +$"{objEnum}").ToString();
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
    }


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;


        switch (currentMainSeq)
        {
            case MainSeq.Default:
                break;
            case MainSeq.AllFoodIntroduce:
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
                break;
            case MainSeq.OnFinish:
                break;
        }
        
        foreach (var hit in GameManager_Hits)
        {
            PlayParticleEffect(hit.point);
            Logger.Log($"clickedName {hit.transform.name}, clickedID {hit.transform.GetInstanceID()}");
            int id = hit.transform.GetInstanceID();
 
        }

        
    }

    private void OnDefault()
    {
        for (int i = (int)GameObj.FishA; i <= (int)GameObj.DonutB; i++)
        {
            GetObject(i).transform.localScale = UnityEngine.Vector3.zero;
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
        for (int i = (int)GameObj.FishA; i <= (int)GameObj.DonutB; i++)
        {
            int localIndex = i;
            var obj = GetObject(localIndex).transform;
            obj.localScale = Vector3.zero;

            _currentMasterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex] * 1.125f, 0.1f).SetEase(Ease.OutBack));
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
            _currentMasterSequence.AppendInterval(0.3f);
        }
        
        for (int i = (int)GameObj.ColaA; i <= (int)GameObj.DonutA; i++)
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
            _currentMasterSequence.AppendInterval(0.3f);
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
    
    List<GameObj> goodFoodList = new List<GameObj>
    {
        GameObj.FishA, GameObj.MeatA, GameObj.ChickenA, GameObj.AppleA,
        GameObj.EggA, GameObj.MilkA, GameObj.CarrotA,
        GameObj.FishB, GameObj.MeatB, GameObj.ChickenB, GameObj.AppleB,
        GameObj.EggB, GameObj.MilkB, GameObj.CarrotB
    };

    List<GameObj> badFoodList = new List<GameObj>
    {
        GameObj.ColaA, GameObj.CookieA, GameObj.IceCreamA, GameObj.PizzaA,
        GameObj.ChocolateA, GameObj.CakeA, GameObj.DonutA,
        GameObj.ColaB, GameObj.CookieB, GameObj.IceCreamB, GameObj.PizzaB,
        GameObj.ChocolateB, GameObj.CakeB, GameObj.DonutB
    };

    private void OnGoodFoodChangeToBadFood()
    {
        _currentMasterSequence?.Kill();
        _currentMasterSequence = DOTween.Sequence();
        
        

    // 좋은 음식과 대응하는 나쁜 음식 인덱스 (1:1 대응, 순서대로)

    foreach (var index  in goodFoodList)
    {
        _isPosEmptyMap[(int)index] = true;
    }

    for (int i = 0; i < goodFoodList.Count; i++)
    {
        int goodIndex = (int)goodFoodList[i];
        int badIndex = (int)badFoodList[i];

        var goodObj = GetObject(goodIndex).transform;
        var badPrefab = GetObject(badIndex); // Prefab or sample
        var badParent = PoolRoot.transform;

       
        _currentMasterSequence.AppendCallback(() =>
        {
//            Messenger.Default.Publish(new EA009_Payload("좋은음식이 나쁜음식으로 바뀌는 중.."));
        });

        // 1. 좋은 음식 작아지기
        _currentMasterSequence.Append(goodObj.DOScale(Vector3.zero, _isDevMode? delayForDevMode :0.15f).SetEase(_disappearAnimEase));

        // 2. 같은 위치에 나쁜 음식 생성
        _currentMasterSequence.AppendCallback(() =>
        {
            GameObject badClone = null;

            // Stack에서 복제 오브젝트 꺼내기 (중복 방지)
            if (_foodClonePool.ContainsKey(badIndex) && _foodClonePool[badIndex].Count > 0)
            {
                badClone = _foodClonePool[badIndex].Pop();
                badClone.SetActive(true);
            }
            else
            {
                // 복제본 부족 시 새로 생성
                Logger.ContentTestLog("복제본 부족, 다시생성 ");
                badClone = Instantiate(badPrefab, badParent);
            }

            badClone.transform.position = goodObj.position;
            badClone.transform.DOLocalRotate(new Vector3(badClone.transform.eulerAngles.x,UnityEngine.Random.Range(-360,360),badClone.transform.eulerAngles.z),
                _isDevMode? delayForDevMode :0.15f);
            badClone.transform.localScale = Vector3.zero;
            badClone.name = $"BadClone_{(GameObj)badIndex}_{i}";
            badClone.transform.DOScale(_defaultSizeMap[badIndex], _isDevMode? delayForDevMode :0.15f).SetEase(_appearAnimEase);
       //     Logger.ContentTestLog($"나쁜음식: {(GameObj)badIndex} 크기 :{_defaultSizeMap[badIndex]}");
        });


     

        _currentMasterSequence.AppendInterval(0.15f);
    }

    // 완료 후 다음 단계로
    _currentMasterSequence.AppendCallback(() =>
    {
        Logger.ContentTestLog("모든 음식이 나쁜 음식으로 변신 완료!");
        currentMainSeq = MainSeq.BadFoodEatIntro;
    });
    }

    #endregion

    #region Animation // BadfoodeatIntro, 좋은음식-> 나쁜음식으로 바뀌는 파트

    private void OnBadFoodEatIntro()
    {
        DOVirtual.DelayedCall(3f, () =>
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

        OnBadFoodRoundInit();
    }

    private void OnBadFoodRoundInit()
    {
        Logger.ContentTestLog(" 나쁜음식 클릭하기 시작---RoundInit 완료");

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
                        .DOScale(_defaultSizeMap[thisObjTransID] * 1.4f, _isDevMode? delayForDevMode :0.15f).SetEase(_appearAnimEase));
                    _badFoodClickRelatedSeq[thisObjTransID].AppendInterval(0.25f);
                    _badFoodClickRelatedSeq[thisObjTransID].Append(allObj[key].transform
                        .DOScale(_defaultSizeMap[thisObjTransID] * 0.7f, _isDevMode? delayForDevMode :0.15f).SetEase(_appearAnimEase));
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

        DOVirtual.DelayedCall(2f, () =>
        {
            isBadFoodClickable = true;
        });
      
    }
    private Dictionary<int, Sequence> _badFoodClickRelatedSeq = new Dictionary<int, Sequence>();
    private Dictionary<int, int> _clickedCountMap = new();
    
    private void OnRaySyncOnBadFoodEat()
    {
        if (!isBadFoodClickable)
        {
            Logger.ContentTestLog($"현재 클릭 불가 --{currentBadFoodClickGameCategory}");
            return;
        }

        foreach (var hit in GameManager_Hits)
        {
               if (hit.transform.gameObject.name.Contains(currentBadFoodClickGameCategory.ToString()))
            {
                int clickedFoodID = hit.transform.GetInstanceID();
                
                
                if (_tfIdToEnumMap.TryGetValue(clickedFoodID, out var foodEnum))
                {
                    int foodIndex = (int)foodEnum;
                    _isPosEmptyMap[foodIndex] = true;
                    _disappearedBadFoodMap[foodIndex] = (GameObj)foodEnum; // 어떤 badFood가 사라졌는지 기록
                }
                else
                {
                    Logger.ContentTestLog($"[ERROR] 사라진 badFood Enum 찾기 실패: {clickedFoodID}");
                }
                
                _isClickableMap.TryAdd(clickedFoodID, true);
                _clickedCountMap.TryAdd(clickedFoodID, 0);
                if (_isClickableMap[clickedFoodID])
                {
                    _clickedCountMap[clickedFoodID]++;

                    if (_clickedCountMap[clickedFoodID] >= 3)
                    {
                        _badFoodClickRelatedSeq[clickedFoodID]?.Kill();


                   
                    
                        _isClickableMap[clickedFoodID] = false;
                        Logger.ContentTestLog($" 나쁜음식 찾음! --현재 클릭할 나쁜음식 {currentBadFoodClickGameCategory}");
                        hit.transform.DOScale(Vector3.zero, 0.15f).SetEase(_disappearAnimEase);
                        
                        
                        
                        currentBadFoodClickedCount++;
                        if (currentBadFoodClickedCount >= BAD_FOOD_CLICK_TO_COUNT)
                        {
                            DOVirtual.DelayedCall(2f, () =>
                            {
                                if (currentMainSeq != MainSeq.BadFoodEat_RoundC) currentMainSeq++;
                                else currentMainSeq = MainSeq.Stomachache;

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
                else Logger.ContentTestLog($" 이미 클릭됨 {currentBadFoodClickGameCategory}");
            }
            else
                Logger.ContentTestLog($" 해당음식이 아님! --{hit.transform.gameObject.name}");
        }
         
    }

    private void OnBadFoodSelectFinished()
    {
        
    }

    private void OnBadFoodEat_Candy_Finished()
    {
        
    }

    #endregion

    #region Game // BadFoodEat_Chocolate, 나쁜음식먹기

    private void OnBadFoodEat_RoundB()
    {
        var badFoodToEat = UnityEngine.Random.Range(0, _badFoodClickGameList.Count);
        currentBadFoodClickGameCategory =  _badFoodClickGameList[badFoodToEat];
        _badFoodClickGameList.Remove((BadFoodClickGameCategory)badFoodToEat);
       
      
        _currentBadFoodToClick = currentBadFoodClickGameCategoryKorean.ToString();

        OnBadFoodRoundInit();
    }

    private void OnRaySyncOnBadFoodEat_Chocolate()
    {
    }

    private void OnBadFoodEat_Chocolate_Finished()
    {
    }

    #endregion

    #region Game // BadFoodEat_IceCream, 나쁜음식먹기-아이스크림

    private void OnBadFoodEat_RoundC()
    {
        var badFoodToEat = UnityEngine.Random.Range(0, _badFoodClickGameList.Count);
        currentBadFoodClickGameCategory =  _badFoodClickGameList[badFoodToEat];
        _badFoodClickGameList.Remove((BadFoodClickGameCategory)badFoodToEat);
       
      
        _currentBadFoodToClick = currentBadFoodClickGameCategoryKorean.ToString();

        OnBadFoodRoundInit();
    }

    private void OnRaySyncOnBadFoodEat_IceCream()
    {
    }

    private void OnBadFoodEat_Chocolate_IceCream()
    {
    }

    #endregion


    #region Animation //Stomachache, 배탈

    
    private void OnStomachache()
    {
        foreach (var key in _badFoodClickRelatedSeq.Keys.ToArray()) _badFoodClickRelatedSeq[key]?.Kill();
        
        var animationDurationOnStomachache = 3.0f;
        DOVirtual.DelayedCall(3.0f, () =>
        {
            currentMainSeq = MainSeq.GoodFoodChangeIntro;
        });
       
    }

    #endregion


    #region Animation // GoodFoodIntro, 좋은음식 

    private void OnGoodFoodChangeIntro()
    {
        float animationDurationOnStomachache = 3.0f;

        foreach (var key in _isPosEmptyMap.Keys.ToArray())
        {
            if (_isPosEmptyMap[key])
            {
                // 1. 위치 정보 획득
                if (!_defaultPosMap.TryGetValue(key, out var spawnPosition))
                {
                    Logger.ContentTestLog($"[ERROR] _defaultPosMap에 존재하지 않는 ID: {key}");
                    continue;
                }

                // 2. 랜덤 bad food 선택
                var randomBad = badFoodList[UnityEngine.Random.Range(0, badFoodList.Count)];
                int badIndex = (int)randomBad;

                GameObject badClone = null;

                // 3. 풀에서 꺼내거나 새로 생성
                if (_foodClonePool.ContainsKey(badIndex) && _foodClonePool[badIndex].Count > 0)
                {
                    badClone = _foodClonePool[badIndex].Pop();
                    badClone.SetActive(true);
                }
                else
                {
                    var badPrefab = GetObject(badIndex);
                    badClone = Instantiate(badPrefab, PoolRoot.transform, true);
                }

                // 4. 위치 및 초기화
                badClone.transform.position = spawnPosition;
                badClone.transform.localScale = Vector3.zero;

                if (_defaultSizeMap.TryGetValue(badClone.transform.GetInstanceID(), out var targetScale))
                {
                    badClone.transform.DOScale(targetScale, 0.25f).SetEase(_appearAnimEase);
                }
                else
                {
                    badClone.transform.DOScale(Vector3.one * 0.4f, 0.25f).SetEase(_appearAnimEase); // fallback
                }

                // 5. 등록 및 상태 업데이트
                badClone.name = $"BadClone_Intro_{badIndex}_{key}";
                allObj[badClone.transform.GetInstanceID()] = badClone;
                _isPosEmptyMap[key] = false;
            }
        }

        // 다음 단계로 전환
        DOVirtual.DelayedCall(animationDurationOnStomachache, () =>
        {
            currentMainSeq = MainSeq.BadFoodRemoval;
        });
    }

    #endregion

    #region Game // BadFoodRemoval, 나쁜음식 제거하기 

    private void OnBadFoodRemoval()
    {
    }

    private void OnRaySyncOnBadFoodRemoval()
    {
    }

    private void BadFoodRemovalFinished()
    {
    }

    #endregion
}