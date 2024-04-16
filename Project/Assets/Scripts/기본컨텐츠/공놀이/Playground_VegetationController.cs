using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;


public class Playground_VegetationController : MonoBehaviour
{
    private GameObject[] vegetations;
    private int _currentVeggie = 0;
    public Vector3 _veggieGrowPosition { get; set; }
    
    void Start()
    {

        _veggieGrowPosition = Vector3.zero;
        
        int childrenCount = transform.childCount;
        vegetations = new GameObject[childrenCount];

        for (int i = 0; i < childrenCount; i++) {
            vegetations[i] = transform.GetChild(i).gameObject;
        }
        
        Playground_Ball_Base.OnBallIsInTheHole -=OnBallIntoHole;
        Playground_Ball_Base.OnBallIsInTheHole +=OnBallIntoHole;
    }


    private void OnDestroy()
    {
        Playground_Ball_Base.OnBallIsInTheHole -=OnBallIntoHole;
    }


    private bool _onGrowing;
    
    void OnBallIntoHole()
    {
        if (!_onGrowing)
        {
            _onGrowing = true;
            
            vegetations[_currentVeggie % vegetations.Length].transform.localScale = Vector3.zero;
            vegetations[_currentVeggie % vegetations.Length].transform.position = _veggieGrowPosition;
            vegetations[_currentVeggie % vegetations.Length].transform.DOScale(200f, 1.5f)
                .SetEase(Ease.InOutBounce)
                .SetDelay(2f)
                .OnComplete(() =>
                {
                    vegetations[_currentVeggie % vegetations.Length].transform.DOScale(0f, 1.5f)
                        .SetDelay(2f)
                        .OnComplete(() =>
                        {
                            _currentVeggie++;
                            _onGrowing = false;
                        });


                });
        }
       

        
    }
}
