using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class PlaygroundVer3_HoleController : MonoBehaviour
{
    private Transform[] _holes;
    private Sequence[] _seqs;
    private Vector3 _defaultSize;

    private void Start()
    {
        var holeCount = transform.childCount;
        _holes = new Transform[holeCount];
        _seqs = new Sequence[holeCount];

        for (var i = 0; i < holeCount; i++) _holes[i] = transform.GetChild(i);

        _defaultSize = _holes[0].localScale;
        for (var i = 0; i < holeCount; i++)
        {
            var seq = DOTween.Sequence();
            seq.Append(_holes[i].DOScale(_defaultSize * 1.3f, 1.1f).SetEase(Ease.InSine));
            seq.Append(_holes[i].DOScale(_defaultSize, 1.1f).SetEase(Ease.InSine).SetDelay(Random.Range(0.1f,0.3f)));
            seq.SetLoops(-1, LoopType.Yoyo);
            seq.Play();
            _seqs[i] = seq;
        }
    }


}
