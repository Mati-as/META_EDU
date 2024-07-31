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

    private readonly int RUN_ANIM = Animator.StringToHash("Run");
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

    private bool _isSoundPlaying;
    private void HandleSliderValueChanged(float value)
    {
        var currentVal = _parrotSlider.value;
        if (currentVal < 0.3f)
        {
            _parrotAnimator.SetBool(FLY_ANIM, false);
            _parrotAnimator.SetBool(SPIN_ANIM, false);
            _parrotAnimator.SetBool(RUN_ANIM, false);

            _parrotAnimator.SetBool(IDLE_ANIM, true);
            
            
            _isSoundPlaying = false;
        }

        if (currentVal > 0.3f && currentVal < 0.66f)
        {
            _parrotAnimator.SetBool(IDLE_ANIM, false);
            _parrotAnimator.SetBool(SPIN_ANIM, false);
            _parrotAnimator.SetBool(FLY_ANIM, false);

            _parrotAnimator.SetBool(RUN_ANIM, true);
            
            _isParticlePlaying = false;

            if (!_isSoundPlaying)
            {
                _isSoundPlaying = true;
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/MusicInstruments/Run",0.3f);
            }
       
            
          
        
        }

        if (currentVal > 0.66f && currentVal < 0.90f)
        {
            if (currentVal < 0.77f)
            {
                _isSoundPlaying = false;
            }
            _parrotAnimator.SetBool(IDLE_ANIM, false);
            _parrotAnimator.SetBool(SPIN_ANIM, false);
            _parrotAnimator.SetBool(RUN_ANIM, false);

          
            _parrotAnimator.SetBool(FLY_ANIM, true);
        }

        if (currentVal > 0.90f)
        {
            _parrotAnimator.SetBool(IDLE_ANIM, false);
            _parrotAnimator.SetBool(RUN_ANIM, false);
            _parrotAnimator.SetBool(FLY_ANIM, false);
            _parrotAnimator.SetBool(SPIN_ANIM, true);
            if (!_isSoundPlaying)
            {
                _isSoundPlaying = true;
#if UNITY_EDITOR
                Debug.Log("spinSound");
#endif
                Managers.soundManager.Play(SoundManager.Sound.Effect, "Audio/기본컨텐츠/MusicInstruments/Spin",0.2f);
            }
            
            
          
            if (!_isParticlePlaying)
            {
                _isParticlePlaying = true;
                _ps.Play();
            }
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
