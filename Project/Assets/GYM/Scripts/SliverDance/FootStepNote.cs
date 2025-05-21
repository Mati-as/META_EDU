using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Xml.Serialization;

public enum FootType
{
    Left,
    Right
}

public class FootStepNote : MonoBehaviour
{
    private GYM_GameManager manager;
    [SerializeField] private bool IsStartStep;
    public bool IsStepped;

    public FootType footType;

    public List<string> narrationTexts;

    private SpriteRenderer spriteRenderer;
    private Color StepChangeColor = new Color(1f, 1f, 0.6f);
    private Tween colorTween;
    GameObject effectPrefab;

    [SerializeField] private GameObject FootStepNum;

    public string narrationPath;

    [SerializeField] private bool isDanceTime;
    public Sequence narrationSeq;

    void Start()
    {
        manager = FindObjectOfType<GYM_GameManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        effectPrefab = Resources.Load<GameObject>("Prefabs/Particle/CFX_Poof");

        if (manager != null)
        {
            var textColor = manager.MessageText.color;
            textColor.a = 1f;
            manager.MessageText.color = textColor;

            var imgColor = manager.MessageImg.color;
            imgColor.a = 1f;
            manager.MessageImg.color = imgColor;
        }

        if (!IsStartStep)
        {
            StartFlashing();
        }

        if (isDanceTime)
        {
            Managers.Sound.Stop(SoundManager.Sound.Bgm);
            Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/SliverDance/BGM_Dance");
            manager.IngameInfoUI.SetActive(false);
        }

    }

    public void TriggerStep()
    {
        if (IsStepped) return;
        manager.OnFootStepTriggered();

        colorTween.Kill();
        spriteRenderer.color = Color.white;
        FootStepNum?.SetActive(false);

        PlayNarrationSequence();

        // 이펙트, 사운드 등은 여기서 처리
        Managers.Sound.Play(SoundManager.Sound.Narration, "Audio/SliverDance/" + narrationPath, 1.5f);
        if (effectPrefab != null)
        {
            GameObject effect = Instantiate(effectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        IsStepped = true;
    }

    void PlayNarrationSequence()
    {
        if (narrationTexts == null || narrationTexts.Count == 0)
            return;

        narrationSeq = DOTween.Sequence();

        foreach (string text in narrationTexts)
        {
            string currentText = text;

            narrationSeq.AppendCallback(() =>
            {
                manager.MessageText.text = currentText;
            });

            narrationSeq.AppendInterval(2.2f);
        }

        narrationSeq.Append(manager.MessageText.DOFade(0f,2f));
        narrationSeq.Join(  manager.MessageImg.DOFade(0f, 2f));
      

    }
                      
    void StartFlashing()
    {
        colorTween = spriteRenderer.DOColor(StepChangeColor, 0.7f) //이색깔로 바꿈
            .SetLoops(-1, LoopType.Yoyo) // -1무한번, 원래색으로갔다가 바꾸려는 색깔로 바뀜
            .SetEase(Ease.InOutSine);   // 부드럽게 
    }
    
}