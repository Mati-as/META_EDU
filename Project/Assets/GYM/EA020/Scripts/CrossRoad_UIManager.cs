using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using MyGame.Messages;

public class CrossRoad_UIManager : Base_UIManager
{
    private CrossRoad_GameManager manager;

    private Sequence seq;

    protected override void Awake()
    {
        base.Awake();
        Messenger.Default.Subscribe<NarrationMessage>(OnNarrationFromGm);
        if (manager == null)
        {
            manager = GameObject.FindWithTag("GameManager").GetComponent<CrossRoad_GameManager>();
            Debug.Assert(manager != null, "GameManager가 씬에 없습니다");
        }

    }
    public override bool InitEssentialUI()
    {
        base.InitEssentialUI();
        InitInstructionUI();
        return true;

    }

    private void OnDestroy()
    {
        Messenger.Default.Unsubscribe<NarrationMessage>(OnNarrationFromGm);
    }

    private void OnNarrationFromGm(NarrationMessage message)
    {
        string narrationText = message.Narration;
        string audioPath = message.AudioPath;

        AudioClip audioClip = Resources.Load<AudioClip>($"CrossRoad/Audio/audio_{audioPath}");

        seq?.Kill();
        seq = DOTween.Sequence();

        seq.AppendCallback(() => PopFromZeroInstructionUI(narrationText));
        seq.AppendCallback(() => Managers.Sound.Play(SoundManager.Sound.Narration, $"CrossRoad/Audio/audio_{audioPath}"));
        seq.AppendInterval(audioClip.length + 0.2f);
        seq.AppendCallback(() => ShutInstructionUI(narrationText));
    }


}
