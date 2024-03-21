using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using 기본컨텐츠.다양한악기놀이;

public class MusicIntruments_TrumpetController : MonoBehaviour,IMusicInstrumentsIOnClick
{
    public void OnClicked()
    {
        PlayRotation(transform,Random.Range(-40f,-60f),_defaultQuat);
        var randomChar = (char)Random.Range('A', 'B' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect,
            "Audio/기본컨텐츠/MusicInstruments/Trumpet" ,
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

        //var randomChar = (char)Random.Range('A', 'B' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect,
            "Audio/기본컨텐츠/MusicInstruments/Trumpet" ,
            0.3f,pitch:Random.Range(0.9f,1.1f));
        PlayAutomatic(transform,Random.Range(-12f,12f),_defaultQuat);
    }


    private void PlayRotation(Transform trumpet, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log($"{gameObject.name}");
#endif


        // 시퀀스 중복실행방지용
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;


        _seq = DOTween.Sequence();
        
        _automaticSeq.Kill();
        _automaticSeq = null;

        _seq.Append(trumpet.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, rotateAmount, 0), 0.08f)
                .SetEase(Ease.InOutSine))
            .AppendInterval(0.5f)
            .Append(trumpet.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 0.08f)
                .SetEase(Ease.OutQuint))
            .OnComplete(() =>
            {
                PlayAutomatic(transform,Random.Range(-12f,12f),_defaultQuat);
                _seq = null;
            });
    }
    
    private void PlayAutomatic(Transform drum, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("cymbal");
#endif


        // 시퀀스 중복실행방지용
        if (_automaticSeq != null && _automaticSeq.IsActive() && _automaticSeq.IsPlaying()) return;


        _automaticSeq = DOTween.Sequence();

        _automaticSeq
            .Append(drum.DORotateQuaternion(defaultRotation * Quaternion.Euler(Random.Range(-rotateAmount,rotateAmount), 0, 0), 0.18f)
                .SetEase(Ease.InOutSine))
            .Append(drum.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 0.18f)
                .SetEase(Ease.OutQuint))
            .AppendInterval(Random.Range(1.0f,1.1f))
            .SetLoops(-1, LoopType.Yoyo);
    }
}
