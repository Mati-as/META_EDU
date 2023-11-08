using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class IdleLakeGameManager : MonoBehaviour
{
    private void Start()
    {
        DOTween.SetTweensCapacity(2000,50);
    }
}
