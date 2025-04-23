using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using UnityEngine.Video;

public class GYM_GameManager : Base_GameManager
{
    private RaycastHit[] _hits;
    public List<GameObject> Footsteps;

    public GameObject lastLeftStep = null;
    public GameObject lastRightStep = null;

    public int currentIndex;    //발자국 댄스 시작 번호 지정

    public Text MessageText;
    public Image MessageImg;

    public Camera mainCamera;
    public Camera UICamera;

    private Sequence msgSeq;
    private bool hasGameStarted = false;

    public VideoPlayer player;
    public GameObject IngameInfoUI;

    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        Color MessageImgColor = MessageImg.color;
        MessageImgColor.a = 1f;
        MessageImg.color = MessageImgColor;

        Color MessageTextColor = MessageText.color;
        MessageTextColor.a = 1f;
        MessageText.color = MessageTextColor;

        if (mainCamera != null)
        {
            mainCamera.rect = new Rect(
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
                XmlManager.Instance.ScreenSize,
                XmlManager.Instance.ScreenSize
            );
        }

        if (UICamera != null)
        {
            UICamera.rect = new Rect(
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
                0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
                XmlManager.Instance.ScreenSize,
                XmlManager.Instance.ScreenSize
            );
        }

        Reset();

        UI_Scene_StartBtn.onGameStartBtnShut += PlayNarration;

    }

    private void PlayNarration()
    {
        Managers.Sound.Play(SoundManager.Sound.Bgm, "Audio/SliverDance/BGM_piano");
        Managers.Sound.Play(SoundManager.Sound.Narration, "Audio/SliverDance/audio_0~3");

        player.Play();

        msgSeq?.Kill();
        msgSeq = DOTween.Sequence();
        msgSeq.AppendInterval(0.7f);
        msgSeq.AppendCallback(() => MessageText.text = "먼저 힐터치 동작에 대해 알아보겠습니다");
        msgSeq.AppendInterval(2.6f);
        msgSeq.AppendCallback(() => MessageText.text = "힐터치 동작은 앞꿈치 찍기로 한쪽 다리를 골반 넓이만큼 옆으로 이동해 찍는 동작입니다");
        msgSeq.AppendInterval(5.5f);
        msgSeq.AppendCallback(() => MessageText.text = "먼저 오른쪽부터 왼쪽으로 각 2회 연습해보겠습니다.");
        msgSeq.AppendInterval(4f);
        msgSeq.AppendCallback(() => MessageText.text = "화면 중앙 위치로 이동해주세요");
        msgSeq.AppendCallback(() => MessageImg.DOFade(0f, 1f).SetDelay(2f));
        msgSeq.AppendCallback(() => MessageText.DOFade(0f, 1f).SetDelay(2f));

        UI_Scene_StartBtn.onGameStartBtnShut -= PlayNarration;
    }

    //private Dictionary<int, Sequence> sequenceMap =new Dictionary<int, Sequence>();
    //sequenceMap.Add(obj.GetInstanceID, DOTween.Sequence());

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync() || !isStartButtonClicked) return;

        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            var stepNote = hit.collider.GetComponent<FootStepNote>();
            if (stepNote != null)
            {
                foreach(var obj in Footsteps)
                {
                    obj.GetComponent<FootStepNote>().narrationSeq?.Kill();
                }
                stepNote.TriggerStep();
                return;
            }
        }
    }

    void Reset()
    {
        foreach (GameObject foot in Footsteps)
        {
            foot.SetActive(false);
        }

        lastLeftStep = Footsteps[0];
        lastRightStep = Footsteps[1];

        lastLeftStep.SetActive(true);
        lastRightStep.SetActive(true);

        currentIndex = 2;

    }
    
    void ForceStartGame()
    {
        if (hasGameStarted) return;
        hasGameStarted = true;
        msgSeq?.Kill();
    }

    public void OnFootStepTriggered()
    {
        ForceStartGame();
        if (currentIndex < 0 || currentIndex >= Footsteps.Count)
            return;

        GameObject currentStep = Footsteps[currentIndex];

        if (currentStep == null)
            return;

        currentStep.SetActive(true);

        FootStepNote nextStepNote = currentStep.GetComponent<FootStepNote>();
        if (nextStepNote == null)
            return;

        FootType nextFootType = nextStepNote.footType;

        if (nextFootType == FootType.Left)
        {
            lastLeftStep?.SetActive(false);
            lastLeftStep = currentStep;
        }
        else if (nextFootType == FootType.Right)
        {
            lastRightStep?.SetActive(false);
            lastRightStep = currentStep;
        }

        currentIndex++;
        // 2초 동안 다른 발자국을 밟지 않도록 IsStepped를 설정

        nextStepNote.IsStepped = true;
        DOVirtual.DelayedCall(2f, () =>
        {
            nextStepNote.IsStepped = false;
        });
        
    }

}