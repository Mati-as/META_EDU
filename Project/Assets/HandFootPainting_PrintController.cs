using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandFootPainting_PrintController : MonoBehaviour
{
    private Vector3 _defaultSize;

    private void Awake()
    {
        _defaultSize = transform.localScale;
    }

    private void Start()
    {
        if (transform.localScale == Vector3.zero)
        {
            Debug.LogError("Default size is zero. Awake order must be changed");
        }
       

    }
    private void OnEnable()
    {
        
        if (!HandFootPainting_GameManager.isInit)
        {
            Debug.Log("game is not initialized yet.");
            return;
        }
        
        transform.localScale = Vector3.zero;
        transform.DOScale(_defaultSize, 0.125f).SetEase(Ease.OutBounce);
    }



}
