using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using 기본컨텐츠.다양한악기놀이;

public class MusicInstruments_CymbalController : MonoBehaviour,IMusicInstrumentsIOnClick
{
    public void OnClicked()
    {
        PlayBeadsDrumAnimation(transform,Random.Range(-10f,10f),_defaultQuat);
       // var randomChar = (char)Random.Range('A', 'B' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect,
            "Audio/기본컨텐츠/MusicInstruments/Cymbal" ,
            0.3f,pitch:Random.Range(0.9f,1.1f));
    }

    private float _shrinkAmount = 0.92f;
    private Vector3 _defaultSize;
    private Sequence _seq;
    private Sequence _automaticSeq;
    private Quaternion _defaultQuat;
    private void Start()
    {
        _defaultSize = transform.localScale;
        _defaultQuat = transform.rotation;
        
        PlayAutomatic(transform,Random.Range(-10f,10f),_defaultQuat);
    }


    private void PlayBeadsDrumAnimation(Transform guitar, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("cymbal");
#endif


        // 시퀀스 중복실행방지용
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;


        _seq = DOTween.Sequence();

        _seq
            .Append(guitar.DORotateQuaternion(defaultRotation * Quaternion.Euler(rotateAmount, 0, 0), 0.08f)
                .SetEase(Ease.InOutSine))
            .Append(guitar.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 0.08f)
                .SetEase(Ease.OutQuint))
            .OnComplete(() => _seq = null);
    }
    
    private void PlayAutomatic(Transform cymbal, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("cymbal Auto");
#endif


        // 시퀀스 중복실행방지용
        if (_automaticSeq != null && _automaticSeq.IsActive() && _automaticSeq.IsPlaying()) return;


        _automaticSeq = DOTween.Sequence();

        _automaticSeq
            .Append(cymbal.DORotateQuaternion(defaultRotation * Quaternion.Euler(Random.Range(-rotateAmount,rotateAmount), 0, 0), 0.08f)
                .SetEase(Ease.InOutSine))
            .Append(cymbal.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 0.08f)
                .SetEase(Ease.OutQuint))
            .AppendInterval(1.5f)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
