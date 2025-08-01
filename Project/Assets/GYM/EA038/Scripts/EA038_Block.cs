using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BlockType
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Purple
}

public class EA038_Block : MonoBehaviour
{
    public int Value;
    public bool canClicked = true;

    private Sequence _shakeSeq;

    private TextMeshPro valueText;

    [SerializeField] private BlockType blockType;

    private EA038_GameManager gameManager;

    public Vector3 originalScale;
    
    private void Awake()
    {
        gameManager = FindAnyObjectByType<EA038_GameManager>();
        valueText = GetComponentInChildren<TextMeshPro>();
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        transform.localScale = originalScale;
    }

    public void Shake()
    {
        _shakeSeq?.Kill();

        _shakeSeq = DOTween.Sequence()
            .SetLink(gameObject) // 이 게임 오브젝트와 연결
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

    public void SetValue(int newValue)
    {
        Value = newValue;
    }
    
    public void ChangeValueTMP(int newValue)
    {
        valueText.text = newValue.ToString();
    }

    public void settingBlock()
    {
        int value = gameManager.blockCodeMap[blockType];
        Value = value;
        valueText.text = value.ToString();
    }
    
}