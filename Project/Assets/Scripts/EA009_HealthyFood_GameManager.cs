using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EA009_HealthyFood_GameManager : Base_GameManager
{
   private enum FoodType
   {
      BadFoodGroup,
      Fish=1,
      Meat,
      Chicken,
      Apple,
      Egg,
      Milk,
      Carrot,
      
      //------ Bad 
      HealthyFoodGroup,

      Hamburger,
      Cookie,
      Icecream,
      Pizza,
      Chocolate,
      Cake,
      Donut,
      
   }

   private Ease _disappearAnimEase = Ease.InOutSine;
   private Ease _appearAnimEase = Ease.InOutSine;
   private Animator _mainCameraAnimator;
  
   protected override void Init()
   {
      //BindObject(typeof(FoodType));
      _mainCameraAnimator = Camera.main.GetComponent<Animator>();
   }
   
 
}
