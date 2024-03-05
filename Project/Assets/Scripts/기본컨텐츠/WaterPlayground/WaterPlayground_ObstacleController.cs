using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class WaterPlayground_ObstacleController : MonoBehaviour
{
    private Transform[] _propellers;

    private int CHILD_COUNT;
    private Vector3 _defaultScale;

    private float _elapsed;

    [Range(0, 60)] public float propellerAppearInterval;

    [Range(0, 10000)] public float rotationAmount;
    [Range(0, 30)] public float rotationDuration;

    private void Awake()
    {
        CHILD_COUNT = transform.childCount;
        _propellers = new Transform[CHILD_COUNT];

        for (int i = 0; i < CHILD_COUNT; i++)
        {
            _propellers[i] = transform.GetChild(i);
            _defaultScale = _propellers[i].localScale;
            _propellers[i].localScale = Vector3.zero;
            _propellers[i].gameObject.SetActive(false);
        }
    }
    
    

    private void Update()
    {
        if (!isRunning) _elapsed += Time.deltaTime;

        if (_elapsed > propellerAppearInterval)
        {
            TurnOnPropellers();
            _elapsed = 0;
        }
    }

    private bool isRunning;

    private void TurnOnPropellers()
    {

        isRunning = true;
        Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/WaterPlayground/PropellerAppear",0.5f);
        for (var i = 0; i < CHILD_COUNT; i++)
        {
            var i1 = i;
            _propellers[i].gameObject.SetActive(true);
            _propellers[i].DOScale(_defaultScale, 0.8f)
                .SetDelay(Random.Range(0.1f, 0.185f))
                .OnComplete(() =>
                {
                    // Do not use DoQuaternion 
                    float rotationAmountDegrees
                        = Random.Range(0, 2) % 2 == 1 ? rotationAmount : -rotationAmount;
                    Vector3 targetEulerRotation
                        = _propellers[i1].transform.eulerAngles + new Vector3(0, rotationAmountDegrees, 0);

                    _propellers[i1].transform
                        .DORotate(targetEulerRotation, rotationDuration, RotateMode.FastBeyond360)
                        .SetEase(Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/WaterPlayground/PropellerAppear",0.5f);
                            _propellers[i1].DOScale(Vector3.zero, 0.8f).SetDelay(Random.Range(0.8f,1.2f));
                            isRunning = false;
                        });
                });
        }
    }
}

