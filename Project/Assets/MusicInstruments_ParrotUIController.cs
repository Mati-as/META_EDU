using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MusicInstruments_ParrotUIController : MonoBehaviour
{
    private Slider _parrotSlider;
    private Animator _parrotAnimator;

    private readonly int IDLE_ANIM = Animator.StringToHash("Idle");
    private readonly int FLY_ANIM = Animator.StringToHash("Fly");
    private readonly int SPIN_ANIM = Animator.StringToHash("Spin");


    private Sequence _autoSeq;
    private Quaternion _defaultQuat;
    private ParticleSystem _ps;
    private bool _isParticlePlaying;
   
    private void Start()
    {
        _parrotSlider = GameObject.Find("ParrotSlider").GetComponent<Slider>();
        _ps = GameObject.Find("CFX_MusicInstruments").GetComponent<ParticleSystem>();
        
        _parrotAnimator = GetComponent<Animator>();
        _defaultQuat = transform.rotation;
        PlayAutomatic(transform,80f,_defaultQuat);
        
        _parrotSlider.onValueChanged.AddListener(HandleSliderValueChanged);

    }

    private void OnDestroy()
    {
        _parrotSlider.onValueChanged.RemoveListener(HandleSliderValueChanged);
    }

    private void HandleSliderValueChanged(float value)
    {
        var currentVal = _parrotSlider.value;
        if (currentVal < 0.3f)
        {
            _parrotAnimator.SetBool(FLY_ANIM, false);
            _parrotAnimator.SetBool(SPIN_ANIM, false);

            _parrotAnimator.SetBool(IDLE_ANIM, true);
        }

        if (currentVal > 0.3f && currentVal < 0.66f)
        {
            _parrotAnimator.SetBool(IDLE_ANIM, false);
            _parrotAnimator.SetBool(SPIN_ANIM, false);

            _parrotAnimator.SetBool(FLY_ANIM, true);
            _isParticlePlaying = false;
        }

        if (currentVal > 0.66f)
        {
            _parrotAnimator.SetBool(IDLE_ANIM, false);
            _parrotAnimator.SetBool(FLY_ANIM, false);
            if (!_isParticlePlaying)
            {
                _isParticlePlaying = true;
                _ps.Play();
            }

            _parrotAnimator.SetBool(SPIN_ANIM, true);
        }
    }
 

    private void PlayAutomatic(Transform thisTransform, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("cymbal");
#endif


        // 시퀀스 중복실행방지용
        if (_autoSeq != null && _autoSeq.IsActive() && _autoSeq.IsPlaying()) return;


        _autoSeq = DOTween.Sequence();

        _autoSeq
            .Append(thisTransform.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, Random.Range(-rotateAmount,rotateAmount), 0), 1f)
            .SetEase(Ease.InOutSine))
            .AppendInterval(Random.Range(0.5f,0.8f))
            .Append(thisTransform.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 1)
            .SetEase(Ease.OutQuint))
            .AppendInterval(Random.Range(0.5f,1.1f))
            .SetLoops(-1, LoopType.Yoyo);
    }
}
