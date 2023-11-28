using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Lake_GameManager : MonoBehaviour
{
    public Ray ray { get; set;}
    
    private void Start()
    {
        DOTween.SetTweensCapacity(2000,50);
    }
}
