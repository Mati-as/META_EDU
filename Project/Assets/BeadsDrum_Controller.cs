using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BeadsDrum_Controller : MonoBehaviour
{

    private Transform _beadsDrumLeft;
    private Transform _beadsDrumRight;
    
    private Transform _drumStickLeft;
    private Transform _drurmStickRight;
    
    private float _shrinkAmount = 0.92f;
    private float _defaultSize;
    private Quaternion _defaultQuatLeft;
    private Quaternion _defaultQuatRight;

    public static event Action OnStickHitLeft; 
    public static event Action OnStickHitRight; 

    enum BeadsDrum
    {
        Left,
        Right,
       
    }

    enum DrumStick
    {
        DrumstickLeft,
        DrumstickRight
    }

    private void Start()
    {
        _beadsDrumLeft = transform.GetChild((int)BeadsDrum.Left);
        _beadsDrumRight = transform.GetChild((int)BeadsDrum.Right);
        _defaultSize = _beadsDrumLeft.localScale.x;
        
        var drumSticks = transform.GetChild(2);
        _drumStickLeft = drumSticks.GetChild((int)DrumStick.DrumstickLeft);
        _drurmStickRight =drumSticks.GetChild((int)DrumStick.DrumstickRight);

        _defaultQuatLeft = _drumStickLeft.rotation;
        _defaultQuatRight = _drurmStickRight.rotation;


        BeadsDrum_GameManager.OnLeftDrumClicked -= OnLeftDrumClicked;
        BeadsDrum_GameManager.OnLeftDrumClicked += OnLeftDrumClicked;
        
        BeadsDrum_GameManager.OnRightDrumClicked -= OnRightDrumClicked;
        BeadsDrum_GameManager.OnRightDrumClicked += OnRightDrumClicked;


    }

    private void OnDestroy()
    {
        BeadsDrum_GameManager.OnLeftDrumClicked -= OnLeftDrumClicked;
        BeadsDrum_GameManager.OnRightDrumClicked -= OnRightDrumClicked;
    }

    // private void OnStartButtonClicked()
    // {
    //     PlayDrumAndStickAnimation(_beadsDrumLeft,_drumStickLeft, -10, _defaultQuatLeft,OnStickHitLeft);
    //     
    //     DOVirtual.Float(0, 0, 1, _ => { }).OnComplete(() =>
    //     {
    //         PlayDrumAndStickAnimation(_beadsDrumRight,_drurmStickRight, 10, _defaultQuatRight,OnStickHitRight);
    //     });
    //
    // }

    private void OnLeftDrumClicked()
    {
        PlayDrumAndStickAnimation(_beadsDrumLeft,_drumStickLeft, -10, _defaultQuatLeft,OnStickHitLeft);
    }

    private void OnRightDrumClicked()
    {
        PlayDrumAndStickAnimation(_beadsDrumRight,_drurmStickRight, 10, _defaultQuatRight,OnStickHitRight);
    }

    private Sequence _seq;
    private Sequence _stickSeq;
    private void PlayDrumAndStickAnimation(Transform drum,Transform stick,float rotateAmount,Quaternion defaultRotation,Action action)
    {

        if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;
        _seq = DOTween.Sequence();

        _seq.Append(stick.DORotateQuaternion(defaultRotation * Quaternion.Euler
                (rotateAmount, 0, 0), 0.07f).SetEase(Ease.InOutSine))
            .AppendCallback(() =>
            {

                action?.Invoke();
            })
            .AppendInterval(0.02f)
            .Append(drum.DOScale(_shrinkAmount, 0.10f).SetEase(Ease.InOutSine))
            .AppendInterval(0.01f)
            .Append(drum.DOScale(_defaultSize, 0.08f).SetEase(Ease.InOutSine))
            .Append(stick.DORotateQuaternion(defaultRotation, 0.04f).SetEase(Ease.InOutSine))
            .AppendCallback(() => { _seq = null;});


    }

}
