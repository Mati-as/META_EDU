using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class EA038_Car : MonoBehaviour
{
    public int carValue;
    public bool canClicked = true;

    private Sequence _shakeSeq;

    private TextMeshPro valueText;

    private void Start()
    {
        valueText = GetComponentInChildren<TextMeshPro>();
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    public void SetValue(int newValue)
    {
        carValue = newValue;
    }
    
    public void Shake()
    {
        _shakeSeq?.Kill();

        _shakeSeq = DOTween.Sequence()
            .SetLink(gameObject) // 이 게임 오브젝트와 연결
            .SetDelay(Random.Range(0f, 0.5f))
            .Append(transform.DOShakeRotation(
                duration: 0.5f,
                strength: new Vector3(40f, 0, 40f),
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

    public void ChangeValueTMP(int newValue)
    {
        valueText.text = newValue.ToString();
    }

}