using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EA038_Card : MonoBehaviour
{
    public int cardValue;
    public bool canClicked = true;
    
    private Sequence _shakeSeq;
    
    private TextMeshPro cardValueText;

    private void Start()
    {
        cardValueText = GetComponentInChildren<TextMeshPro>();
        transform.localScale = Vector3.zero;
    }

    public void SetValue(int newValue)
    {
        cardValue = newValue;
    }

    public void Shake()
    {
        _shakeSeq?.Kill();

        _shakeSeq = DOTween.Sequence()
            .SetLink(gameObject)  // 이 게임 오브젝트와 연결
            .SetDelay(Random.Range(0f, 0.5f))
            .Append(transform.DOShakeRotation(
                duration: 0.5f,
                strength: new Vector3(10f, 0, 10f),
                vibrato: 10,
                randomness: 90f
            ))
            .AppendInterval(2.5f)
            .SetLoops(-1, LoopType.Restart);
    }

    public void KillShake()
    {
        if (_shakeSeq != null)
        {
            _shakeSeq.Kill();
            _shakeSeq = null;
        }
    }

    public void ChangeCardValueTMP(int newValue)
    {
        cardValueText.text = newValue.ToString();
    }
    
}
