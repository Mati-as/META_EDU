using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lake_GameManager : IGameManager
{
  
    private Lake_IAnimalBehavior _lake_IAnimalBehavior;

    protected override void Init()
    {
        base.Init();
        DOTween.SetTweensCapacity(2000,50);
    }


    public override void OnRaySynced()
    {
        base.OnRaySynced();
        var hits = Physics.RaycastAll(GameManager_Ray);

        foreach (var hit in hits)
        {
            var obj =hit.transform.gameObject;
            obj.TryGetComponent(out _lake_IAnimalBehavior);
            _lake_IAnimalBehavior?.OnClicked();
        }
    }


}
