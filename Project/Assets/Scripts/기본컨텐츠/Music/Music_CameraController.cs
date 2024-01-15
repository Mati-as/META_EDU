using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.Rendering.HighDefinition;

public class Music_CameraController : MonoBehaviour
{

    private Vector3 _defaultPosition;
    void Start()
    {
        BindEvent();
        _defaultPosition = transform.position;
    }


    private void OnDestroy()
    {
        Music_BubbleController.bigBubbleEvent -= OnBigBubbleExplode;
        Music_BubbleController.onPatternEnd -= OnClearOffBubbles;
    }

    private bool _isShaking;
    private void OnBigBubbleExplode()
    {
        transform.DOShakePosition(1.5f, 2, 11)
            .OnStart(() =>
            {
                _isShaking = true;
            })
            .OnComplete(() =>
        {
            _isShaking = false;
            transform.DOMove(_defaultPosition, 0.2f);
        });
    }

    private void OnClearOffBubbles()
    {
        transform.DOShakePosition(1.5f, 0.5f, 5).OnComplete(() =>
        {
            transform.DOMove(_defaultPosition, 0.2f);
        });
    }
    private void BindEvent()
    {
        Music_BubbleController.bigBubbleEvent -= OnBigBubbleExplode;
        Music_BubbleController.bigBubbleEvent += OnBigBubbleExplode;

        Music_BubbleController.onPatternEnd -= OnClearOffBubbles;
        Music_BubbleController.onPatternEnd += OnClearOffBubbles;
    }
}
