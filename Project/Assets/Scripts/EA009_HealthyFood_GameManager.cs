using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class EA009_HealthyFood_GameManager : Ex_BaseGameManager
{

   private enum SequenceName { Default, SeqA, SeqB, OnFinish }

   private enum Food
   {
      // 좋은 음식
      GoodFoodGroup, Fish = 1, Meat, Chicken, Apple, Egg, Milk, Carrot,

      // 나쁜 음식
      HealthyFoodGroup, Hamburger, Cookie, Icecream, Pizza, Chocolate, Cake, Donut,
   }

   private Dictionary<Food, Transform> _goodFoodGroup = new();
   private Dictionary<Food, Transform> _badFoodGroup = new();
   private Dictionary<Food, Vector3> _originalScaleMap = new();
   private Ease _disappearAnimEase = Ease.InOutSine;
   private Ease _appearAnimEase = Ease.InOutSine;
   private Animator _mainCameraAnimator;

   protected override void Init()
   {
      base.Init();
      BindObject(typeof(Food));

 
      InitializeFoodGroup(new List<Food> { Food.Fish, Food.Meat, Food.Chicken, Food.Apple, Food.Egg, Food.Milk, Food.Carrot }, _goodFoodGroup);
      InitializeFoodGroup(new List<Food> { Food.Hamburger, Food.Cookie, Food.Icecream, Food.Pizza, Food.Chocolate, Food.Cake, Food.Donut }, _badFoodGroup);

      foreach (var food in _goodFoodGroup.Keys.Concat(_badFoodGroup.Keys))
         _originalScaleMap[food] = GetObject((int)food).transform.localScale;


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

      foreach (var key in _badFoodGroup.Keys.ToArray())
      {
         _badFoodGroup[key].gameObject.SetActive(true);
         _badFoodGroup[key].gameObject.transform.DOScale(_originalScaleMap[key],Random.Range(0.5f,1.5f)).SetEase(_appearAnimEase).SetDelay(Random.Range(0.5f,1.5f));
         Logger.Log($"doscale : {_originalScaleMap[key]}");
      }

   }


 
}
