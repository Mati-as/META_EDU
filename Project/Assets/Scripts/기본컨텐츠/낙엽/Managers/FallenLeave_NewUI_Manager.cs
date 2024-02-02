using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
public class FallenLeave_NewUI_Manager : MonoBehaviour
{


    private void Awake()
    {
        
      
    }

    // Update is called once per frame
    private void Start()
    {
        OnFallenLeaveStart();

    }

    public static event Action OnStart;
    void OnFallenLeaveStart()
    {

        DOVirtual.Float(0, 1, 3f, _ => _++).OnComplete(() =>
        {
            OnStart?.Invoke();
          
        });



    }
}
