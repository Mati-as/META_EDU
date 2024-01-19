using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IGameManager: MonoBehaviour
{
   public static Ray GameManager_Ray { get; private set; }
   public static event Action onRaySync ;
   
   protected virtual void BindEvent()
   {
      Image_Move.OnGetInputFromUser -= OnClicked;
      Image_Move.OnGetInputFromUser += OnClicked;
   }

   protected void OnClicked()
   {
      GameManager_Ray = Image_Move.ray_ImageMove;
      onRaySync?.Invoke();
   }

}
