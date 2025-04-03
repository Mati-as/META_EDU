using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EA009_HealthyFood_GameManager : Ex_BaseGameManager
{
    private enum SequenceName
    {
        Default,
        BadFoodSelection,
        GoodFoodAppear,
        OnFinish
    }

    private enum Food
    {
        // 좋은 음식
        GoodFoodGroup,
        Fish = 1,
        Meat,
        Chicken,
        Apple,
        Egg,
        Milk,
        Carrot,

        // 나쁜 음식
        HealthyFoodGroup,
        Hamburger,
        Cookie,
        Icecream,
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

    private readonly int COUNT_OF_FOOD_TO_CHANGE = 7;
    private int _currentCountOfFoodChanged;
    public static event Action<int> SeqMessageEvent;

    private int currentCountOfFoodChanged
    {
        get
        {
            return _currentCountOfFoodChanged;
        }
        set
        {
            _currentCountOfFoodChanged = value;
            Logger.ContentTestLog($"현재 바뀐 음식 수 {value}");
            if (_currentCountOfFoodChanged >= COUNT_OF_FOOD_TO_CHANGE)
            {
                _currentCountOfFoodChanged = 0;
                OnAllFoodChanged();
            }
        }
    }

    private void OnAllFoodChanged()
    {
        Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA009/Delicious");
        SeqMessageEvent?.Invoke((int)SeqNar.Delicious);

        DOVirtual.DelayedCall(1.8f, () =>
        {
            foreach (var key in _meshRendererMap.Keys.ToArray())
            {
                Logger.ContentTestLog($"{_meshRendererMap[key].transform.gameObject.name} : 좋은음식 반짝이기 실행 ----");
                var mat = _meshRendererMap[key].material;

                var seq = DOTween.Sequence();
                // Emission 활성화
                mat.EnableKeyword("_EMISSION");

                // 처음 밝게 (120,120,120) → 어둡게 (0,0,0)
                Color brightColor = new Color(110 / 255f, 110 / 255f, 110 / 255f);
                Color darkColor = Color.black;

                // DOTween으로 _EmissionColor를 애니메이션
                seq.Append(DOTween.To(
                    () => mat.GetColor(EmissionColor),
                    x => mat.SetColor(EmissionColor, x),
                    brightColor,
                    0.15f // 밝아지는 시간
                ).OnComplete(() =>
                {
                    DOTween.To(
                        () => mat.GetColor(EmissionColor),
                        x => mat.SetColor(EmissionColor, x),
                        darkColor,
                        0.2f // 어두워지는 시간
                    );
                }).SetDelay(Random.Range(0.6f,0.7f))
                  );
                seq.SetLoops(10, LoopType.Yoyo);
            }
        });

    }

    private SequenceName _currentSequence = SequenceName.Default;

    private readonly Dictionary<int, Food> _idToFoodMap = new();
    private readonly Dictionary<Food, int> clickCountMap = new(); // 사라지는 애니메이션, 흔들리는 애니메이션구분
    private readonly Dictionary<Food, bool> isClickedMap = new(); // 사라지는 애니메이션, 흔들리는 애니메이션구분
    private const int CLICK_COUNT_TO_GET_RID_OF_BAD_FOOD = 3;


    private Dictionary<int, Food> transformID = new();


    private readonly Dictionary<Food, Transform> _goodFoodGroup = new();
    private readonly Dictionary<int, MeshRenderer> _meshRendererMap = new();
    private readonly Dictionary<Food, Transform> _badFoodGroup = new();
    private readonly Dictionary<Food, Transform> _allFoodGroup = new();
    private readonly Dictionary<Food, Vector3> _originalScaleMap = new();
    private Ease _disappearAnimEase = Ease.InOutSine;
    private readonly Ease _appearAnimEase = Ease.InOutSine;
    private Animator _mainCameraAnimator;

    private bool _isFoodClickable;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    protected override void Init()
    {
        psResourcePath = "Runtime/EA009/Fx_Click";
        base.Init();
        BindObject(typeof(Food));

        var goodFoods = new List<Food>
            { Food.Fish, Food.Meat, Food.Chicken, Food.Apple, Food.Egg, Food.Milk, Food.Carrot };
        var badFoods = new List<Food>
            { Food.Hamburger, Food.Cookie, Food.Icecream, Food.Pizza, Food.Chocolate, Food.Cake, Food.Donut };

        InitializeFoodGroup(goodFoods, _goodFoodGroup);
        InitializeFoodGroup(badFoods, _badFoodGroup);

        HideFoods(_goodFoodGroup);
        HideFoods(_badFoodGroup);
    }

    private void InitializeFoodGroup(List<Food> foods, Dictionary<Food, Transform> targetGroup)
    {
        foreach (var food in foods)
        {
            var obj = GetObject((int)food).transform;
            
            targetGroup[food] = obj;
            
            _allFoodGroup[food] = obj;
            
            _idToFoodMap[obj.GetInstanceID()] = food;
          
            var meshRenderers =obj.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in meshRenderers)
            {
                Logger.ContentTestLog($"{mr.transform.gameObject.name} :mesh renderer 추가");
                _meshRendererMap.TryAdd(mr.GetInstanceID(), mr);
            }
           
            
            _originalScaleMap[food] = obj.localScale;
            
            clickCountMap[food] = 0;
            isClickedMap[food] = false;

            Logger.Log($"[Init] Added Food: {food}, ID: {obj.GetInstanceID()}");
        }
    }

    private void HideFoods(Dictionary<Food, Transform> foodGroup)
    {
        foreach (var food in foodGroup.Values)
        {
            food.localScale = Vector3.zero;
            food.gameObject.SetActive(false);
        }
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();


        DOVirtual.DelayedCall(1.0f, () =>
        {
            Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA009/Hungry");

            DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length,
                () =>
                {
                    var interval = 0f;
                    foreach (var key in _badFoodGroup.Keys.ToArray())
                    {
                        _badFoodGroup[key].gameObject.SetActive(true);
                        _badFoodGroup[key].gameObject.transform
                            .DOScale(_originalScaleMap[key], Random.Range(0.1f, 0.3f)).SetEase(_appearAnimEase)
                            .SetEase(Ease.InOutBack)
                            .OnStart(() =>
                            {
                                Managers.Sound.Play(SoundManager.Sound.Effect, "SortedByScene/EA009/fxA");
                            })
                            .SetDelay(interval);

                        interval += 0.08f;
                        
                   
                        Logger.Log($"doscale : {_originalScaleMap[key]}");
                    }

                    DOVirtual.DelayedCall(
                        Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length - 3.8f,
                        () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScene/EA009/ChangeToGoodFood");
                        });


                    DOVirtual.DelayedCall(
                        Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length - 1f,
                        () =>
                        {
                            _isFoodClickable = true;
                            _currentSequence = SequenceName.BadFoodSelection;
                        });
                });
        });
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

            if (_currentSequence == SequenceName.BadFoodSelection)
            {
                if (isClickedMap[clickedFood])
                {
                    Logger.Log("이미 클릭된 음식입니다.");
                    return;
                }

                isClickedMap[clickedFood] = true;
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    isClickedMap[clickedFood] = false;
                });

                clickCountMap[clickedFood]++;
                Managers.Sound.Play(SoundManager.Sound.Effect, "SortedByScene/EA009/fxA");
                ShakeTransform(hit.transform, clickedFood);
                return;
            }
        }
    }

    private void ShakeTransform(Transform target, Food clickedFood)
    {
        target.DOShakePosition(Random.Range(0.3f, 0.6f), 0.12f).OnComplete(() =>
        {
            if (clickCountMap[clickedFood] > 2)
            {
                clickCountMap[clickedFood] = 0; // 중복실행 방지용
                currentCountOfFoodChanged++;
                Managers.Sound.Play(SoundManager.Sound.Effect, "SortedByScene/EA009/fxB");

                target.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InOutBounce)
                    .OnStart(() =>
                    {
                        target.DOMoveY(target.position.y + 0.5f, 0.05f).SetEase(Ease.InOutBounce);
                    }).OnComplete(() =>
                    {
                        target.gameObject.SetActive(false);

                        // 🎉 나쁜 음식이 사라졌다면 → 좋은 음식 등장
                        Managers.Sound.Play(SoundManager.Sound.Effect, "SortedByScene/EA009/fxC");
                        var goodFood = GetPairedGoodFood(clickedFood);
                        if (_goodFoodGroup.TryGetValue(goodFood, out var goodTransform))
                        {
                            goodTransform.gameObject.SetActive(true);
                            goodTransform.localScale = Vector3.zero;
                            goodTransform.DOScale(_originalScaleMap[goodFood], 0.2f)
                                .SetEase(Ease.InOutBounce);

                            Logger.Log($"[PairSwap] {clickedFood} 사라짐 → {goodFood} 등장");
                        }
                    });
            }
        });
    }

    private Food GetPairedGoodFood(Food badFood)
    {
        // 나쁜 음식 enum 값에서 7을 빼면 대응되는 좋은 음식이 나옴
        // (enum 순서상 Fish = 1, Hamburger = 8 → 8 - 7 = 1)
        return (Food)((int)badFood - 8);
    }
}