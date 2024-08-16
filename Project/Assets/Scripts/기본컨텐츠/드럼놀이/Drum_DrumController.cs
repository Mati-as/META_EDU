using DG.Tweening;
using UnityEngine;
using 기본컨텐츠.다양한악기놀이;

public class Drum_DrumController : MonoBehaviour,IMusicInstrumentsIOnClick
{

    private float _shrinkAmount = 0.92f;
    private Vector3 _defaultSize;
    private Sequence _seq;
    private Sequence _automaticSeq;

    [Range(0.5f,1.5f)]
    public float pitch;
 

    private Quaternion _defaultQuat;
    private void Start()
    {
        _defaultSize = transform.localScale;
        _defaultQuat = transform.rotation;
        PlayAutomatic(transform,Random.Range(-8f,8f),_defaultQuat);
    }


    public void OnClicked()
    {
        PlayBeadsDrumAnimation(transform,Random.Range(-20f,20f),_defaultQuat);
   
        var randomChar = (char)Random.Range('A', 'B' + 1);
        Managers.soundManager.Play(SoundManager.Sound.Effect,
            "Audio/기본컨텐츠/MusicInstruments/Drum" ,
            0.3f,pitch:pitch);
    }
    
    private void PlayBeadsDrumAnimation(Transform drum, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("cymbal");
#endif


        // 시퀀스 중복실행방지용
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;

     
        _automaticSeq.Kill();
        _automaticSeq = null;
        
        var rarndomValue = Random.Range(-rotateAmount, rotateAmount);
        var rotateValue = Mathf.Abs(rarndomValue) < 10? 10 : rarndomValue;

        _seq = DOTween.Sequence();
        _seq
            .Append(drum.DORotateQuaternion(defaultRotation * Quaternion.Euler(rotateValue, 0, 0), 0.08f)
                .SetEase(Ease.InOutSine))
            .Append(drum.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 0.08f)
                .SetEase(Ease.OutQuint))
            .OnComplete(() =>
            {
                PlayAutomatic(transform,Random.Range(-8f,8f),_defaultQuat);
                _seq = null;
            });
    }
    
    private void PlayAutomatic(Transform drum, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("Drum");
#endif


        // 시퀀스 중복실행방지용
        if (_automaticSeq != null && _automaticSeq.IsActive() && _automaticSeq.IsPlaying()) return;


        _automaticSeq = DOTween.Sequence();


        _automaticSeq
            .Append(drum.DORotateQuaternion(defaultRotation * Quaternion.Euler(Random.Range(-rotateAmount,rotateAmount), 0, 0), 0.08f)
                .SetEase(Ease.InOutSine))
            .Append(drum.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 0.08f)
                .SetEase(Ease.OutQuint))
            .AppendInterval(Random.Range(1.0f,1.1f))
            .SetLoops(-1, LoopType.Yoyo);
    }
}

