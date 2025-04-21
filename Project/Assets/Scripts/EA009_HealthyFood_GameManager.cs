using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class EA009_Payload : IPayload
{
    public string Narration
    {
        get;
    }

    public string CurrentCarName
    {
        get;
    }

    public EA009_Payload(string narration, string carname = "car")
    {
        Narration = narration;
        CurrentCarName = carname;
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
        GoodFoodIntro,
        BadFoodRemoval,
        OnFinish
    }

    private readonly Dictionary<int, Stack<GameObject>> _foodClonePool = new();

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

    public enum SeqNar
    {
        HungryTimeToEat,
        ChangeToGoodFood,
        Delicious
    }


    private MainSeq _currentMainSeq = MainSeq.Default;

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

            switch (value)
            {
                case MainSeq.Default:
                    OnDefault();
                    break;
                case MainSeq.AllFoodIntroduce:
                    OnAllFoodIntroduce();
                    break;
                case MainSeq.GoodFoodChangeToBadFood:
                    OnGoodFoodChangeToBadFood();
                    break;
                case MainSeq.BadFoodEatIntro:
                    OnBadFoodEatIntro();
                    break;
                case MainSeq.BadFoodEat_RoundA:
                    OnBadFoodEat_RoundA();
                    break;
                case MainSeq.BadFoodEat_RoundB:
                    OnBadFoodEat_RoundB();
                    break;
                case MainSeq.BadFoodEat_RoundC:
                    OnBadFoodEat_RoundC();
                    break;
                case MainSeq.Stomachache:
                    OnStomachache();
                    break;
                case MainSeq.GoodFoodIntro:
                    OnGoodFoodIntro();
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

            return root;
        }
    }

    private void SetFoodPool()
    {
        for (int i = (int)GameObj.FishA; i <= (int)GameObj.CarrotA; i++)
        {
            _foodClonePool.Add(i, new Stack<GameObject>());
            for (int count = 0; count < 10; count++)
            {
                var food = Instantiate(GetObject((int)GameObj.ColaA));
                food.transform.SetParent(PoolRoot.transform);
                food.SetActive(false);
                _foodClonePool[i].Push(food);
            }
        }
   
    }


    protected override void Init()
    {
        PoolRoot.transform.SetParent(gameObject.transform);
        psResourcePath = "Runtime/EA009/Fx_Click";
        SHADOW_MAX_DISTANCE = 60;
        base.Init();
        BindObject(typeof(GameObj));
        SetFoodPool();

        currentMainSeq = MainSeq.Default;
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        currentMainSeq = MainSeq.AllFoodIntroduce;
    }


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;


        foreach (var hit in GameManager_Hits)
        {
            PlayParticleEffect(hit.point);
            Logger.Log($"clickedName {hit.transform.name}, clickedID {hit.transform.GetInstanceID()}");
            int id = hit.transform.GetInstanceID();
            bool isFood = _idToFoodMap.TryGetValue(id, out var clickedFood);
            if (!isFood)
            {
                Logger.Log($"ID에 해당하는 음식이 없습니다.{id}");
                return;
            }
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
        Sequence masterInitSequence = DOTween.Sequence();
        Sequence masterSequence = DOTween.Sequence();
        masterSequence.Pause(); // 먼저 멈춰놓고 대기

// 🔹 masterInitSequence 내부 구성
        for (int i = (int)GameObj.FishA; i <= (int)GameObj.DonutB; i++)
        {
            int localIndex = i;
            var obj = GetObject(localIndex).transform;
            obj.localScale = Vector3.zero;

            masterInitSequence.Append(obj.DOScale(_defaultSizeMap[localIndex] * 1.1f, 0.2f).SetEase(Ease.OutBack));
            masterInitSequence.Append(obj.DOScale(_defaultSizeMap[localIndex], 0.1f));
            masterInitSequence.AppendInterval(0.1f);
        }

// 🔹 Init 끝나고 → masterSequence 실행
        masterInitSequence.OnComplete(() =>
        {
            Logger.ContentTestLog("Init 애니메이션 완료, 다음 단계 실행");
            masterSequence.Play();
        });

// 🔹 masterSequence 구성 (차례대로 메시지 + 애니메이션)
        for (int i = (int)GameObj.FishA; i <= (int)GameObj.CarrotA; i++)
        {
            int localIndex = i;
            var obj = GetObject(localIndex).transform;

            masterSequence.AppendCallback(() =>
            {
                Messenger.Default.Publish(new EA009_Payload(obj.name));
                Logger.ContentTestLog($"Messenger: {obj.name}");
            });

            masterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex] * 1.4f, 0.15f).SetEase(Ease.InOutBack));
            masterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex], 0.15f).SetEase(Ease.InOutBack));
            masterSequence.AppendInterval(0.2f);
        }
        
        for (int i = (int)GameObj.ColaA; i <= (int)GameObj.DonutA; i++)
        {
            int localIndex = i;
            var obj = GetObject(localIndex).transform;

            masterSequence.AppendCallback(() =>
            {
                Messenger.Default.Publish(new EA009_Payload(obj.name));
                Logger.ContentTestLog($"Messenger: {obj.name}");
            });

            masterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex] * 1.4f, 0.15f).SetEase(Ease.InOutBack));
            masterSequence.Append(obj.DOScale(_defaultSizeMap[localIndex], 0.15f).SetEase(Ease.InOutBack));
            masterSequence.AppendInterval(0.2f);
        }
    }

    #endregion


    #region Animation //  GoodFoodChangeToBadFood, 좋은음식-> 나쁜음식으로 바뀌는 파트

    private void OnGoodFoodChangeToBadFood()
    {
    }

    #endregion

    #region Animation // BadfoodeatIntro, 좋은음식-> 나쁜음식으로 바뀌는 파트

    private void OnBadFoodEatIntro()
    {
    }

    #endregion

    #region Game // BadFoodEat_Candy, 사탕먹기

    private void OnBadFoodEat_RoundA()
    {
    }

    private void OnRaySyncOnBadFoodEat_Candy()
    {
    }

    private void OnBadFoodEat_Candy_Finished()
    {
    }

    #endregion

    #region Game // BadFoodEat_Chocolate, 나쁜음식먹기

    private void OnBadFoodEat_RoundB()
    {
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
    }

    #endregion


    #region Animation // GoodFoodIntro, 좋은음식 

    private void OnGoodFoodIntro()
    {
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