using DG.Tweening;
using UnityEngine;

public class BB001EX1_ArrowAnimController : MonoBehaviour
{
    private Vector3 _defaultSize;

    private void Start()
    {
        _defaultSize = transform.localScale;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOScale(_defaultSize * 1.2f, 1.1f).SetEase(Ease.InSine));
        seq.Append(transform.DOScale(_defaultSize, 1.1f).SetEase(Ease.InSine));
        seq.SetLoops(-1, LoopType.Yoyo);
        seq.Play();
    }
}