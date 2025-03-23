using System;
using System.Collections;
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
      GoodFoodGroup, Fish = 1, Meat, Chicken, Apple, Egg, Milk, Carrot,

      // 나쁜 음식
      HealthyFoodGroup, Hamburger, Cookie, Icecream, Pizza, Chocolate, Cake, Donut,
   }
   
   private SequenceName _currentSequence = SequenceName.Default;

   private Dictionary<int, Food> _idToFoodMap =new();
   private Dictionary<Food, int> clickCountMap = new(); // 사라지는 애니메이션, 흔들리는 애니메이션구분
   private Dictionary<Food, bool> isClickedMap = new(); // 사라지는 애니메이션, 흔들리는 애니메이션구분
   private const int CLICK_COUNT_TO_GET_RID_OF_BAD_FOOD =3;
   
   
   private Dictionary<int, Food> transformID = new();
   
   
   private Dictionary<Food, Transform> _goodFoodGroup = new();
   private Dictionary<Food, Transform> _badFoodGroup = new();
   private Dictionary<Food, Transform> _allFoodGroup = new();
   private Dictionary<Food, Vector3> _originalScaleMap = new();
   private Ease _disappearAnimEase = Ease.InOutSine;
   private Ease _appearAnimEase = Ease.InOutSine;
   private Animator _mainCameraAnimator;

   private bool _isFoodClickable;

   protected override void Init()
   {
       base.Init();
       BindObject(typeof(Food));

       var goodFoods = new List<Food> { Food.Fish, Food.Meat, Food.Chicken, Food.Apple, Food.Egg, Food.Milk, Food.Carrot };
       var badFoods  = new List<Food> { Food.Hamburger, Food.Cookie, Food.Icecream, Food.Pizza, Food.Chocolate, Food.Cake, Food.Donut };
    
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

           // 그룹별로 등록
           targetGroup[food] = obj;

           // 전체 음식 목록에도 등록
           _allFoodGroup[food] = obj;

           // ID 매핑
           _idToFoodMap[obj.GetInstanceID()] = food;

           // 오리지널 스케일
           _originalScaleMap[food] = obj.localScale;

           // 클릭 관련 초기화
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
       
      
      DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length, () =>
      {
         DOVirtual.DelayedCall(0.5f, () =>
         {
            Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScenes/EA009/Hungry");
            
            DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length + 2f, () =>
            {
               Managers.Sound.Play(SoundManager.Sound.Narration, "SortedByScenes/EA009/ChangeToGoodFood");
               
               DOVirtual.DelayedCall(Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.length , () =>
               {
                  _isFoodClickable = true;
                  _currentSequence = SequenceName.BadFoodSelection;
               });
            });
         });
         
         foreach (var key in _badFoodGroup.Keys.ToArray())
         {
            _badFoodGroup[key].gameObject.SetActive(true);
            _badFoodGroup[key].gameObject.transform.DOScale(_originalScaleMap[key],Random.Range(0.5f,1.5f)).SetEase(_appearAnimEase).SetDelay(Random.Range(0.5f,1.5f));
            Logger.Log($"doscale : {_originalScaleMap[key]}");
         }

      });

   }

   public override void OnRaySynced()
   {
      if (!PreCheckOnRaySync()) return;


      foreach (var hit in GameManager_Hits)
      {
          Logger.Log($"clickedName {hit.transform.name}, clickedID {hit.transform.GetInstanceID()}");
          int id = hit.transform.GetInstanceID();
          bool isFood = _idToFoodMap.TryGetValue(id, out Food clickedFood);
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
              DOVirtual.DelayedCall(1f, () => { isClickedMap[clickedFood] = false; });
              
              clickCountMap[clickedFood]++;
              ShakeTransform(hit.transform,clickedFood);
              return;
          }
      }
      
      
      
   }

   private void ShakeTransform(Transform target,Food clickedFood)
   {
      target.DOShakePosition(Random.Range(1.0f,2.0f), 0.1f, 10, 90f, false).OnComplete(() =>
      {
          if (clickCountMap[clickedFood] > 3)
          {
              clickCountMap[clickedFood] = 0; // 중복실행 방지용
              target.DOScale(Vector3.zero, 1f).OnComplete(() =>
              {
                  target.gameObject.SetActive(false);
                  
                  // 🎉 나쁜 음식이 사라졌다면 → 좋은 음식 등장
                  Food goodFood = GetPairedGoodFood(clickedFood);
                  if (_goodFoodGroup.TryGetValue(goodFood, out var goodTransform))
                  {
                      goodTransform.gameObject.SetActive(true);
                      goodTransform.localScale = Vector3.zero;
                      goodTransform.DOScale(_originalScaleMap[goodFood], 0.7f)
                          .SetEase(_appearAnimEase);

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
