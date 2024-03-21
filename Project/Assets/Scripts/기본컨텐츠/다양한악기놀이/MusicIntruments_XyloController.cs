using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using 기본컨텐츠.다양한악기놀이;

public class MusicIntruments_XyloController : MonoBehaviour,IMusicInstrumentsIOnClick
{
    private Transform _parentTransform;

    private float _shrinkAmount = 0.92f;
    private Vector3 _defaultSize;
    private Sequence _seq;
    private Sequence _stickSeq;
    private Quaternion _defaultQuat;
    private void Start()
    {
        //getComponentInParent는 자기자신 포함임에 주의
        _parentTransform = transform.parent;
#if UNITY_EDITOR
        Debug.Log($"{_parentTransform.gameObject.name}");
#endif
        _defaultQuat = _parentTransform.rotation;
    }

    public void OnClicked()
    {
        var randomChar = (char)Random.Range('A', 'B' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect,
            "Audio/기본컨텐츠/MusicInstruments/Xylophone" + randomChar ,
            0.3f,pitch:Random.Range(0.9f,1.1f));
        PlayRotation(_parentTransform,Random.Range(-5f,5f),_defaultQuat);
    }

    private void PlayRotation(Transform trumpet, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log($"{gameObject.name}");
#endif


        // 시퀀스 중복실행방지용
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;


        _seq = DOTween.Sequence();

        _seq
            .Append(trumpet.DORotateQuaternion(defaultRotation * Quaternion.Euler(rotateAmount,0 , 0), 0.04f)
                .SetEase(Ease.InOutSine))
            .AppendInterval(0.02f)
            .Append(trumpet.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 0.04f)
                .SetEase(Ease.OutQuint))
            .OnComplete(() => _seq = null);
    }
}
