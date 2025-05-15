using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class Construction_UIManager : Base_UIManager
{
    private Construction_GameManager manager;

    private Sequence seq;

    protected override void Awake()
    {
        base.Awake();
        Messenger.Default.Subscribe<NarrationMessage>(OnNarrationFromGm);
        if (manager == null) manager = GameObject.FindWithTag("GameManager").GetComponent<Construction_GameManager>();
        Debug.Assert(manager != null, "GameManager not found");

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

        AudioClip audioClip = Resources.Load<AudioClip>($"Construction/Audio/audio_{audioPath}");

        seq?.Kill();
        seq = DOTween.Sequence();

        seq.AppendCallback(() => PopFromZeroInstructionUI(narrationText));
        seq.AppendCallback(() => Managers.Sound.Play(SoundManager.Sound.Narration, $"Construction/Audio/audio_{audioPath}"));
        seq.AppendInterval(audioClip.length + 1);
        seq.AppendCallback(() => ShutInstructionUI(narrationText));
    }


}
