using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using MyGame.Messages;

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

        AudioClip audioClip = null;
        float clipLength = 0f;
        if (!string.IsNullOrEmpty(audioPath))
        {
            audioClip = Resources.Load<AudioClip>($"Construction/Audio/audio_{audioPath}");
            if (audioClip != null)
                clipLength = audioClip.length;
            else
                Debug.LogWarning($"[UIManager] 오디오 파일 없음: audio_{audioPath}");
        }

        seq?.Kill();
        seq = DOTween.Sequence();

        seq.AppendCallback(() => PopFromZeroInstructionUI(narrationText));

        //오디오 재생 (있을 때만)
        if (audioClip != null)
            seq.AppendCallback(() => Managers.Sound.Play(
                SoundManager.Sound.Narration,
                $"Construction/Audio/audio_{audioPath}"
            ));

        //대기시간 결정: 커스텀 값이 있으면 그만큼, 없으면 clipLength + 1초
        float waitTime = message.CustomDuration ?? (clipLength + 1f);
        seq.AppendInterval(waitTime);

        seq.AppendCallback(() => ShutInstructionUI(narrationText));
    }

    public void ForceCloseNarration()
    {
        seq?.Complete(true);
    }


}
