using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lake_GameManager : MonoBehaviour
{
    public static Ray ray { get; set;}
    private Lake_IAnimalBehavior _lake_IAnimalBehavior;
    
    private void Start()
    {
        DOTween.SetTweensCapacity(2000,50);
        Lake_Image_Move.OnStep -= OnStep;
        Lake_Image_Move.OnStep += OnStep;
    }


    private void OnDestroy()
    {
        Lake_Image_Move.OnStep -= OnStep;
    }

    public void OnStep()
    {
        var hits = Physics.RaycastAll(ray);

        foreach (var hit in hits)
        {
            var obj =hit.transform.gameObject;
            obj.TryGetComponent<Lake_IAnimalBehavior>(out _lake_IAnimalBehavior);
            _lake_IAnimalBehavior?.OnClicked();
#if UNITY_EDITOR

#endif
            
        }
    }
}
