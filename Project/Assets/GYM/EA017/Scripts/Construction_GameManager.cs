using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using MyGame.Messages;
using Random = UnityEngine.Random;

public class Construction_GameManager : Base_GameManager
{
    private RaycastHit[] _hits;

    public Animator excavatorAni;
    public Animator truckAni;
    public Animator bulldozerAni;

    public List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>(5);

    public CinemachineVirtualCamera startVirtualCamera;
    public CinemachineVirtualCamera introVirtualCamera;

    public CinemachineVirtualCamera excavatorShowCamera;
    public CinemachineVirtualCamera truckShowCamera;
    public CinemachineVirtualCamera bulldozerShowCamera;

    public CinemachineVirtualCamera excavatorVirtualCamera;
    public CinemachineVirtualCamera truckVirtualCamera;
    public CinemachineVirtualCamera bulldozerVirtualCamera;

    public GameObject Btns_ExcavatorIntro;
    public GameObject Btns_TruckIntro;
    public GameObject Btns_BulldozerIntro;

    public GameObject Btn_ExcavatorStage;
    public GameObject Btn_TruckStage;
    public GameObject Btn_BulldozerStage;

    public AudioClip victoryAuidoClip;

    public GameObject ExcavatorStageSoil;
    public GameObject TruckStageSoil;
    public GameObject BulldozerStageSoil;

    private Vector3 originExcavatorStageSoilScale = new Vector3(8.37f, 18f, 7.8f);

    private bool Btn_TwiceIssue = true;

    public AudioClip audioClipMove1;
    public AudioClip audioClipMove2;
    public AudioClip audioClipWork1;
    public AudioClip audioClipWork2;

    public AudioClip HeavyMachinerySound; //지워야함 오류생겨서 일단 살린것임

    public Construction_UIManager construction_UIManager;
    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);
        Managers.Sound.Play(SoundManager.Sound.Bgm, "Construction/Audio/audio_BGM");

        ExcavatorStageSoil.transform.localScale = originExcavatorStageSoilScale;
        TruckStageSoil.transform.localScale = originExcavatorStageSoilScale;
        BulldozerStageSoil.transform.localScale = originExcavatorStageSoilScale;

        victoryAuidoClip = Resources.Load<AudioClip>("Construction/Audio/audio_Victory");

        audioClipMove1 = Resources.Load<AudioClip>("Construction/Audio/audio_move1");
        audioClipMove2 = Resources.Load<AudioClip>("Construction/Audio/audio_move2");
        audioClipWork1 = Resources.Load<AudioClip>("Construction/Audio/audio_work1");
        audioClipWork2 = Resources.Load<AudioClip>("Construction/Audio/audio_work2");

        Btns_ExcavatorIntro.SetActive(false);
        Btns_BulldozerIntro.SetActive(false);
        Btns_TruckIntro.SetActive(false);

        Btn_ExcavatorStage.SetActive(false);
        Btn_TruckStage.SetActive(false);
        Btn_BulldozerStage.SetActive(false);

        //if (mainCamera != null)
        //{
        //    mainCamera.rect = new Rect(
        //        0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
        //        0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
        //        XmlManager.Instance.ScreenSize,
        //        XmlManager.Instance.ScreenSize
        //    );
        //}

        //if (UICamera != null)
        //{
        //    UICamera.rect = new Rect(
        //        0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetX - 0.5f),
        //        0.5f - XmlManager.Instance.ScreenSize / 2f + (XmlManager.Instance.ScreenPositionOffsetY - 0.5f),
        //        XmlManager.Instance.ScreenSize,
        //        XmlManager.Instance.ScreenSize
        //    );
        //}

        CinemachineVirtualCamera[] foundCameras = GetComponentsInChildren<CinemachineVirtualCamera>();
        cameras.Clear();
        cameras.AddRange(foundCameras);

        startVirtualCamera = cameras[0];
        introVirtualCamera = cameras[1];
        excavatorShowCamera = cameras[2];
        truckShowCamera = cameras[3];
        bulldozerShowCamera = cameras[4];
        excavatorVirtualCamera = cameras[5];
        truckVirtualCamera = cameras[6];
        bulldozerVirtualCamera = cameras[7];

        //구독해제 하셔야합니다
        UI_InScene_StartBtn.onGameStartBtnShut -= StartGame;
        
        UI_InScene_StartBtn.onGameStartBtnShut += StartGame;
    }

    //OnDestroy 메서드에서 구독 해제 추가해야함
    protected override void OnDestroy()
    {
       
        UI_InScene_StartBtn.onGameStartBtnShut -= StartGame;
        base.OnDestroy();
    }

    private void StartGame()
    {
        Sequence introSeq = DOTween.Sequence();

        introSeq.AppendCallback(() =>
        {
            startVirtualCamera.Priority = 10;
            introVirtualCamera.Priority = 20;
            Messenger.Default.Publish(new NarrationMessage("공사장에서 일하는 자동차들이 건물을 짓고있어요", "0_공사장에서_일하는_자동차들이_건물을_짓고있어요_"));
        });
        introSeq.AppendInterval(6f);
        introSeq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("일하는 자동차들을 도와줘볼까요", "1_일하는_자동차들을_도와줘볼까요_"));
        });
        introSeq.AppendInterval(5f);
        introSeq.AppendCallback(() =>
        {
            introVirtualCamera.Priority = 10;
            excavatorShowCamera.Priority = 20;
            Messenger.Default.Publish(new NarrationMessage("포크레인은 땅 속에 있는 흙을 팔 수 있어요", "2_포크레인은_땅_속에_있는_흙을_팔_수_있어요"));
            excavatorAni.SetBool("Dig", true);
            Managers.Sound.Play(SoundManager.Sound.Effect, HeavyMachinerySound);
            DOVirtual.DelayedCall(3.2f, () => { Managers.Sound.Stop(SoundManager.Sound.Effect); twiceAudioIssue = true; });

            Btns_ExcavatorIntro.SetActive(true);
            Btns_ExcavatorIntro.transform.DOScale(1f, 0.4f)
            .From(0.01f) // 원래 자리에서 작게 시작해서 커지도록
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                Btns_ExcavatorIntro.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
            });

        });
        introSeq.AppendInterval(1f);
        introSeq.AppendCallback(() =>
        {
            excavatorAni.SetBool("Dig", false);

        });
        introSeq.AppendInterval(4.3f);
        introSeq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("포크레인이 멈췄어요!", "26_포크레인이_멈췄어요_"));

        });
        introSeq.AppendInterval(4f);
        introSeq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("버튼을 눌러 포크레인을 움직여보세요!", "23_버튼을_눌러_포크레인을_움직여보세요_", 20f));

        });

    }


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync() || !isStartButtonClicked) return;

        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            //string objectName = hit.collider.gameObject.name;
            ////포크레인 -> 트럭 -> 레미콘 순으로 진행하게끔 
            //if (objectName.Contains("Excavator") && !isExcavatorStage && isIntroSceneEnd)
            //{
            //    isExcavatorStage = true;
            //    introVirtualCamera.Priority = 10;
            //    excavatorVirtualCamera.Priority = 20;
            //    DOVirtual.DelayedCall(4f, () =>
            //    {
            //        Messenger.Default.Publish(new NarrationMessage("포크레인으로 흙을 파봐요", "8_포크레인으로_흙을_파봐요_"));
            //        Btn_Excavator.SetActive(true);
            //    });
            //    return;
            //}
            //if (objectName.Contains("Truck") && isExcavatorStage && !isTruckStage && isIntroSceneEnd)
            //{
            //    isTruckStage = true;
            //    Debug.Log("트럭 스테이지 이동");

            //    introVirtualCamera.Priority=10;
            //    truckVirtualCamera.Priority = 20;

            //    DOVirtual.DelayedCall(3.4f, () =>
            //    {
            //        Messenger.Default.Publish(new NarrationMessage("트럭으로 흙을 옮겨봐요", "12_트럭으로_흙을_옮겨봐요_"));
            //        Btn_Truck.SetActive(true);
            //    });
                
            //    return;
            //}
            //if (objectName.Contains("Rmc") && isExcavatorStage && isTruckStage && !isRmcStage && isIntroSceneEnd)
            //{
            //    isRmcStage = true; //중복방지용
            //    Debug.Log("레미콘 스테이지 이동");
            //    //카메라 이동
            //    //레미콘 스테이지 시작
            //    return;
            //}

        }

    }


    public void Btn_ExcavatorNext() //포크레인 다음 버튼
    {
        construction_UIManager.ForceCloseNarration();
        ClickSound();

        if (Btn_TwiceIssue)
        {
            Btn_TwiceIssue = false;
            Btns_ExcavatorIntro.transform.DOScale(0.01f, 0.3f)
            .SetEase(Ease.InBack) // 등장 시 OutBack이면 사라질 때는 InBack이 자연스러움
            .OnComplete(() =>
            {
                Btns_ExcavatorIntro.SetActive(false);
            });
            introVirtualCamera.Priority = 10;
            excavatorVirtualCamera.Priority = 20;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(3f);
            seq.AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("포크레인으로 흙을 파봐요", "8_포크레인으로_흙을_파봐요_"));
                Btn_ExcavatorStage.SetActive(true);
                Btn_ExcavatorStage.transform.localScale = Vector3.zero;
                Btn_TwiceIssue = true;
            });
            seq.Append(Btn_ExcavatorStage.transform.DOScale(Vector3.one, 1f));
            seq.Append(Btn_ExcavatorStage.transform.DOShakeScale(0.3f, 0.1f));

        }

    }

    public void Btn_TruckNext() //트럭 다음 버튼
    {
        ClickSound();
        construction_UIManager.ForceCloseNarration();
        if (Btn_TwiceIssue)
        {
            Btn_TwiceIssue = false;
            Btns_TruckIntro.transform.DOScale(0.01f, 0.3f)
            .SetEase(Ease.InBack) // 등장 시 OutBack이면 사라질 때는 InBack이 자연스러움
            .OnComplete(() =>
            {
                Btns_TruckIntro.SetActive(false);
            });
            truckShowCamera.Priority = 10;
            truckVirtualCamera.Priority = 20;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(3f);
            seq.AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("트럭으로 흙을 옮겨봐요", "12_트럭으로_흙을_옮겨봐요_"));
                Btn_TruckStage.SetActive(true);
                Btn_TruckStage.transform.localScale = Vector3.zero;
                Btn_TwiceIssue = true;
            });
            seq.Append(Btn_TruckStage.transform.DOScale(Vector3.one, 1f));
            seq.Append(Btn_TruckStage.transform.DOShakeScale(0.3f, 0.1f));

        }

        
    }
    
    public void Btn_BulldozerNext() // 다음 버튼
    {
        ClickSound();
        construction_UIManager.ForceCloseNarration();
        if (Btn_TwiceIssue)
        {
            Btn_TwiceIssue = false;
            Btns_BulldozerIntro.transform.DOScale(0.01f, 0.3f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Btns_BulldozerIntro.SetActive(false);
            });
            bulldozerShowCamera.Priority = 10;
            bulldozerVirtualCamera.Priority = 20;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(3f);
            seq.AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("불도저로 흙을 밀어봐요", "22_불도저로_흙을_밀어봐요"));
                Btn_BulldozerStage.SetActive(true);
                Btn_BulldozerStage.transform.localScale = Vector3.zero;
                Btn_TwiceIssue = true;
            });
            seq.Append(Btn_BulldozerStage.transform.DOScale(Vector3.one, 1f));
            seq.Append(Btn_BulldozerStage.transform.DOShakeScale(0.3f, 0.1f));

        }

    }

    public bool twiceAudioIssue = true;
    public void Btn_ExcavatorAni()
    {
        ClickSound();
        float AnimationLength = 3.1f;
        if (twiceAudioIssue)
        {
            twiceAudioIssue = false;
            excavatorAni.SetBool("Dig", true);
            Managers.Sound.Play(SoundManager.Sound.Effect, HeavyMachinerySound);
            DOVirtual.DelayedCall(0.1f, () => excavatorAni.SetBool("Dig", false));
            DOVirtual.DelayedCall(AnimationLength, () => { Managers.Sound.Stop(SoundManager.Sound.Effect); twiceAudioIssue = true; });
        }
    }

    public void Btn_TruckAni()
    {
        ClickSound();

        float AnimationLength = 3.75f;
        truckAni.SetBool("LiftDown", false);
        truckAni.SetBool("LiftUp", true);
        Managers.Sound.Play(SoundManager.Sound.Effect, HeavyMachinerySound);
        DOVirtual.DelayedCall(0.1f, () => {
            truckAni.SetBool("LiftUp", false);
            truckAni.SetBool("LiftDown", true);
        });
        DOVirtual.DelayedCall(AnimationLength, () => Managers.Sound.Stop(SoundManager.Sound.Effect));
    }

    public void Btn_BulldozerAni()
    {
        ClickSound();
        float AnimationLength = 3.1f;
        bulldozerAni.SetBool("Work", true);
        Managers.Sound.Play(SoundManager.Sound.Effect, HeavyMachinerySound);
        DOVirtual.DelayedCall(0.1f, () => {
            bulldozerAni.SetBool("Work", false);
        });
        DOVirtual.DelayedCall(AnimationLength, () => Managers.Sound.Stop(SoundManager.Sound.Effect));
    }

    public void PlayNarration(int path)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Construction/Audio/audio_{path}");
        Managers.Sound.Play(SoundManager.Sound.Narration, clip);
        DOVirtual.DelayedCall(clip.length, () => { });

    }


    public void ClickSound()
    {
        var click = (char)('A' + Random.Range(0, 6));
        Managers.Sound.Play(SoundManager.Sound.Effect, $"Construction/Audio/Click_{click}");

    }

}
