using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using MyGame.Messages;

public class SideWalk_UIManager : Base_UIManager
{
    private SideWalk_GameManager _manager;

    private Sequence seq;
    
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
    public override bool InitOnLoad()
    {
        base.InitOnLoad();
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

        seq.AppendCallback(() => PopInstructionUIFromScaleZero(narrationText));
        seq.AppendCallback(() => Managers.Sound.Play(SoundManager.Sound.Narration, $"SideWalk/Audio/{audioPath}"));

        // CustomDuration이 유효한 값(0보다 큰 값)이면 그걸, 아니면 audioClip 길이에 0.2초 추가한 걸 사용
        float interval = message.CustomDuration 
                         ?? (audioClip.length + 0.3f);
        
        seq.AppendInterval(interval);

        seq.AppendCallback(() => ShutInstructionUI(narrationText));
    }


}
