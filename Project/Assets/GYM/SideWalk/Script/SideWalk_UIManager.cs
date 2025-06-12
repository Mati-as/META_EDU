using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using MyGame.Messages;

public class SideWalk_UIManager : Base_UIManager
{
    private SideWalk_GameManager manager;

    private Sequence seq;

    protected override void Awake()
    {
        base.Awake();
        Messenger.Default.Subscribe<NarrationMessage>(OnNarrationFromGm);
        if (manager == null)
        {
            manager = GameObject.FindWithTag("GameManager").GetComponent<SideWalk_GameManager>();
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

        AudioClip audioClip = Resources.Load<AudioClip>($"SideWalk/Audio/{audioPath}");

        seq?.Kill();
        seq = DOTween.Sequence();

        seq.AppendCallback(() => PopFromZeroInstructionUI(narrationText));
        seq.AppendCallback(() => Managers.Sound.Play(SoundManager.Sound.Narration, $"SideWalk/Audio/{audioPath}"));
        seq.AppendInterval(audioClip.length + 2f);
        seq.AppendCallback(() => ShutInstructionUI(narrationText));
    }


}
