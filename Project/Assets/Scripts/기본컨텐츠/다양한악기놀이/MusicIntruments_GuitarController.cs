using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using 기본컨텐츠.다양한악기놀이;

public class MusicIntruments_GuitarController : MonoBehaviour, IMusicInstrumentsIOnClick
{
    public void OnClicked()
    {
        PlayRotateSeqAnimation(transform, Random.Range(40f, 50f), _defaultQuat);

        //  var randomChar = (char)Random.Range('A', 'B' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect,
            "Audio/기본컨텐츠/MusicInstruments/GuitarStrumA",
            0.3f, pitch: Random.Range(1.1f, 1.3f));
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
        PlayAutomatic(transform, Random.Range(-10f, -10f), _defaultQuat);

    }


    private void PlayRotateSeqAnimation(Transform guitar, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("Beads Drum Animation Going On");
#endif


        // 시퀀스 중복실행방지용
        if (_seq != null && _seq.IsActive() && _seq.IsPlaying()) return;

        _automaticSeq.Kill();
        _automaticSeq = null;

        _seq = DOTween.Sequence();
           

        _seq
            .Append(guitar.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, rotateAmount, 0), 0.08f)
                .SetEase(Ease.InOutSine))
            .AppendInterval(0.25f)
            .Append(guitar.DORotateQuaternion(defaultRotation * Quaternion.Euler(0, 0, 0), 0.08f)
                .SetEase(Ease.OutQuint))
            .OnComplete(() =>
            {
                PlayAutomatic(transform, Random.Range(-10f, -10f), _defaultQuat);
                _seq = null;
            });
        
     
    }

    private void PlayAutomatic(Transform guitar, float rotateAmount, Quaternion defaultRotation)
    {
#if UNITY_EDITOR
        Debug.Log("Automatic Animation Attempted");
#endif

        // PlayRotateSeqAnimation (_seq)이 활성화되어 있고 재생 중일 때는 PlayAutomatic을 시작하지 않습니다.
  
        // 여기서부터 PlayAutomatic 시퀀스를 시작합니다.
        // automaticSeq가 이미 활성화되어 있고 실행 중이라면, 중복 실행을 방지합니다.
        if (_automaticSeq != null && _automaticSeq.IsActive() && _automaticSeq.IsPlaying()) return;

        _automaticSeq = DOTween.Sequence();

        _automaticSeq
            .Append(guitar
                .DORotateQuaternion(defaultRotation * Quaternion.Euler(Random.Range(-rotateAmount, rotateAmount), 0, 0),
                    0.18f)
                .SetEase(Ease.InOutSine))
            .Append(guitar.DORotateQuaternion(defaultRotation, 0.18f)
                .SetEase(Ease.OutQuint))
            .AppendInterval(0.5f)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
