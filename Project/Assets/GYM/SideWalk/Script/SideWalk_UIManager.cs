using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using MyGame.Messages;

public class SideWalk_UIManager : Base_UIManager
{
    private SideWalk_GameManager _manager;

    private Sequence _seq;

    protected override void Awake()
    {
        base.Awake();
        Messenger.Default.Subscribe<NarrationMessage>(OnNarrationFromGm);
        if (_manager == null)
        {
            _manager = GameObject.FindWithTag("GameManager").GetComponent<SideWalk_GameManager>();
            Debug.Assert(_manager != null, "GameManager가 씬에 없습니다");
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

        var audioClip = Resources.Load<AudioClip>($"SideWalk/Audio/{audioPath}");

        // CustomDuration이 유효하면 그걸 아니면 audioClip 길이에 2초 추가
        float interval = message.CustomDuration ?? (audioClip.length + 2f);
        
        _seq?.Kill();
        _seq = DOTween.Sequence();

        _seq.AppendCallback(() => PopFromZeroInstructionUI(narrationText));
        _seq.AppendCallback(() => Managers.Sound.Play(SoundManager.Sound.Narration, $"SideWalk/Audio/{audioPath}"));
        _seq.AppendInterval(interval);
        _seq.AppendCallback(() => ShutInstructionUI(narrationText));
    }


}
