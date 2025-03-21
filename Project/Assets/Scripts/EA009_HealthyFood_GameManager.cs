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
   private Dictionary<Food, Vector3> _originalScaleMap = new();
   private Ease _disappearAnimEase = Ease.InOutSine;
   private Ease _appearAnimEase = Ease.InOutSine;
   private Animator _mainCameraAnimator;

   private bool _isFoodClickable;

   protected override void Init()
   {
      base.Init();
      BindObject(typeof(Food));

 
      InitializeFoodGroup(new List<Food> { Food.Fish, Food.Meat, Food.Chicken, Food.Apple, Food.Egg, Food.Milk, Food.Carrot }, _goodFoodGroup);
      InitializeFoodGroup(new List<Food> { Food.Hamburger, Food.Cookie, Food.Icecream, Food.Pizza, Food.Chocolate, Food.Cake, Food.Donut }, _badFoodGroup);

      foreach (var food in _goodFoodGroup.Keys.Concat(_badFoodGroup.Keys))
         _originalScaleMap[food] = GetObject((int)food).transform.localScale;

      foreach (var food in _goodFoodGroup.Keys.Concat(_badFoodGroup.Keys))
          clickCountMap.Add(food, 0);
      
      foreach (var food in _goodFoodGroup.Keys.Concat(_badFoodGroup.Keys))
          isClickedMap.Add(food, false);

      foreach (var food in _goodFoodGroup.Keys.Concat(_badFoodGroup.Keys))
          _idToFoodMap.Add(_goodFoodGroup[food].transform.GetInstanceID(),food);
      
      HideFoods(_goodFoodGroup);
      HideFoods(_badFoodGroup);
   }
   private void InitializeFoodGroup(List<Food> foods, Dictionary<Food, Transform> foodGroup)
   {
      foreach (var food in foods)
         foodGroup[food] = GetObject((int)food).transform;
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

   public sealed override void OnRaySynced()
   {
      if (!PreCheckOnRaySync()) return;


      foreach (var hit in GameManager_Hits)
      {
          int id = hit.transform.GetInstanceID();
          _idToFoodMap.TryGetValue(id, out Food clickedFood);
          
          if (_currentSequence == SequenceName.BadFoodSelection)
          {
              if (!isClickedMap[clickedFood])
              {
                  return;
                  Logger.Log("너무 빨리 클릭중");
              }
              isClickedMap[clickedFood] = true;
              clickCountMap[clickedFood]++;
              ShakeTransform(hit.transform,clickedFood);
          }
      }
      
      
      
   }

   private void ShakeTransform(Transform target,Food clickedFood)
   {
      target.DOShakePosition(Random.Range(1.0f,2.0f), 10f, 10, 90f, false).OnComplete(() =>
      {
          isClickedMap[clickedFood] = true;
      });
   }
}
