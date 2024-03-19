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

        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
        UI_Scene_Button.onBtnShut += OnStartButtonClicked;


    }

    private void OnDestroy()
    {
        UI_Scene_Button.onBtnShut -= OnStartButtonClicked;
    }

    private void OnStartButtonClicked()
    {
        PlayBeadsDrumAnimation(_beadsDrumLeft,_drumStickLeft, 10, _defaultQuatLeft);
        //PlayDrumStickAnimation(_drumStickLeft, 10, _defaultQuatLeft);
        DOVirtual.Float(0, 0, 1, _ => { }).OnComplete(() =>
        {
            PlayBeadsDrumAnimation(_beadsDrumRight,_drurmStickRight, 10, _defaultQuatRight);
          //  PlayDrumStickAnimation(_drurmStickRight, 10, _defaultQuatRight);
        });

    }

    private Sequence _seq;
    private Sequence _stickSeq;
    private void PlayBeadsDrumAnimation(Transform drum,Transform stick,float rotateAmount,Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("Beads Drum Animation Going On");
#endif
        _seq = DOTween.Sequence();
        
        _seq.AppendInterval(1.0f)
            .Append(stick.DORotateQuaternion(defaultRotation*Quaternion.Euler(rotateAmount,0,0), 0.08f).SetEase(Ease.InOutSine))
            .Append(stick.DORotateQuaternion(defaultRotation*Quaternion.Euler(-rotateAmount,0,0),0.08f).SetEase(Ease.OutQuint))
            .Append(drum.DOScale(_shrinkAmount, 0.15f).SetEase(Ease.InOutSine))
            .AppendInterval(0.05f)
            .Append(drum.DOScale(_defaultSize, 0.15f).SetEase(Ease.InOutSine))
            .Append(stick.DORotateQuaternion(defaultRotation*Quaternion.Euler(-rotateAmount,0,0), 0.08f).SetEase(Ease.InOutSine))
            .Append(stick.DORotateQuaternion(defaultRotation*Quaternion.Euler(rotateAmount,0,0),0.08f).SetEase(Ease.OutQuint))
            .AppendInterval(1.0f)
            .SetLoops(-1,LoopType.Yoyo);
    }

    // private void PlayDrumStickAnimation(Transform stick,float rotateAmount,Quaternion defaultRotation)
    // {
    //     _stickSeq = DOTween.Sequence();
    //     
    //     _stickSeq.AppendInterval(0.85f)
    //         .Append(stick.DORotateQuaternion(defaultRotation*Quaternion.Euler(rotateAmount,0,0), 0.08f).SetEase(Ease.InOutSine))
    //         .Append(stick.DORotateQuaternion(defaultRotation*Quaternion.Euler(-rotateAmount,0,0),0.08f).SetEase(Ease.OutQuint))
    //         .AppendInterval(0.05f)
    //         .Append(stick.DORotateQuaternion(defaultRotation*Quaternion.Euler(rotateAmount,0,0), 0.08f).SetEase(Ease.InOutSine))
    //         .Append(stick.DORotateQuaternion(defaultRotation*Quaternion.Euler(-rotateAmount,0,0),0.08f).SetEase(Ease.OutQuint))
    //         .AppendInterval(0.85f)
    //         .SetLoops(-1,LoopType.Yoyo);
    // }
    //
}
