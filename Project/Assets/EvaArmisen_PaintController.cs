using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class EvaArmisen_PaintController : MonoBehaviour
{

    private static Vector3 _defaultScale = Vector3.one*0.22f;

    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(_defaultScale, 0.08f).SetEase(Ease.InOutSine);
    }


    private void Awake()
    {
        EvaArmisen_GameManager.OnStampingFinished -= OnStampingFinished;
        EvaArmisen_GameManager.OnStampingFinished += OnStampingFinished;
    }

    

    private void OnDestroy()
    {
        EvaArmisen_GameManager.OnStampingFinished -= OnStampingFinished;
    }

    private void OnStampingFinished()
    {
        transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InOutSine).SetDelay(Random.Range(9+ 0.05f, 10+ 0.2f));
    }
}
