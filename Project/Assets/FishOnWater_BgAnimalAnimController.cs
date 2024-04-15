using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class FishOnWater_BgAnimalAnimController : UI_Base
{
    enum AnimationController
    {
        MarineAnimals,
        Paths
    }

    private Vector3[][] _paths;
    private Transform[] _mrAnimals;




    private void Awake()
    {
        BindEvent();
    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= OnStartBtnShut;
    }
    private void BindEvent()
    {
        UI_Scene_Button.onBtnShut -= OnStartBtnShut;
        UI_Scene_Button.onBtnShut += OnStartBtnShut;
    }

    private void Start()
    {
        
        BindObject(typeof(AnimationController));

         var pathsParent = GetObject((int)AnimationController.Paths).transform;
         var vertexCount = 3;
         _paths = new Vector3[pathsParent.childCount][];
         
         for (int i = 0; i < pathsParent.childCount; i++)
         {
             _paths[i] = new Vector3[vertexCount + 1];
             for (int k = 0; k < vertexCount; k++)
             {
                 _paths[i][k] = pathsParent.GetChild(i).GetChild(k).position;
             }

             _paths[i][vertexCount] = _paths[i][0];
         }

         var pathsAnimals = GetObject((int)AnimationController.MarineAnimals).transform;
         _mrAnimals = new Transform[ pathsAnimals.childCount];
         for (int i = 0; i < pathsAnimals.childCount; i++)
         {
             _mrAnimals[i] = pathsAnimals.GetChild(i);
         }

         Shuffle(_mrAnimals);
        
  
         for (int i = 0; i < _mrAnimals.Length; i++)
         {
             
             _mrAnimals[i].position = _paths[i][0];
         }

  
     

    }

    private void OnStartBtnShut()
    {
        for (int i = 0; i < _mrAnimals.Length; i++)
        {
            _mrAnimals[i].DOPath(_paths[i], Random.Range(55f,65f), PathType.CatmullRom)
                .SetEase(Ease.InSine)
                .SetLookAt(-0.01f)
                .SetLoops(-1, LoopType.Restart).SetDelay(0.5f);
        }
    }
    
    void Shuffle(Transform[] list)
    {
        int n = list.Length;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Transform temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
    
    
}
