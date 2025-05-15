using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

public class NarrationMessage
{
    public string Narration
    {
        get; set;
    }
    public string AudioPath
    {
        get; set;
    }

    public NarrationMessage(string narrtionText, string audioPath)
    {
        Narration = narrtionText;
        AudioPath = audioPath;
    }
}

public class Construction_GameManager : Base_GameManager
{
    private RaycastHit[] _hits;

    public Animator excavatorAni;
    public Animator truckAni;
    public Animator rmcAni;

    public List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>(5);

    public CinemachineVirtualCamera startVirtualCamera;
    public CinemachineVirtualCamera introVirtualCamera;

    public CinemachineVirtualCamera ExcavatorShowCamera;
    public CinemachineVirtualCamera TruckShowCamera;

    public CinemachineVirtualCamera excavatorVirtualCamera;
    public CinemachineVirtualCamera truckVirtualCamera;
    public CinemachineVirtualCamera rmcVirtualCamera;

    public GameObject Btns_ExcavatorIntro;
    public GameObject Btns_TruckIntro;
    public GameObject Btns_RmcIntro;

    public GameObject Btn_ExcavatorStage;
    public GameObject Btn_TruckStage;
    public GameObject Btn_RmcStage;

    public AudioClip victoryAuidoClip;

    public GameObject ExcavatorStageSoil;
    public GameObject TruckStageSoil;
    public GameObject RmcStageSoil;
    private Vector3 originExcavatorStageSoilScale = new Vector3(8.37f, 18f, 7.8f);

    private bool Btn_TwiceIssue = true;

    public AudioClip HeavyMachinerySound;

    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);
        Managers.Sound.Play(SoundManager.Sound.Bgm, "Construction/Audio/audio_BGM");

        ExcavatorStageSoil.transform.localScale = originExcavatorStageSoilScale;
        TruckStageSoil.transform.localScale = originExcavatorStageSoilScale;
        RmcStageSoil.transform.localScale = originExcavatorStageSoilScale;

        victoryAuidoClip = Resources.Load<AudioClip>("Construction/Audio/audio_Victory");
        HeavyMachinerySound = Resources.Load<AudioClip>("Construction/Audio/HeavyMachinerySound");

        Btns_ExcavatorIntro.SetActive(false);
        Btns_RmcIntro.SetActive(false);
        Btns_TruckIntro.SetActive(false);
        Btn_ExcavatorStage.SetActive(false);
        Btn_TruckStage.SetActive(false);
        Btn_RmcStage.SetActive(false);

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
        ExcavatorShowCamera = cameras[2];
        TruckShowCamera = cameras[3];
        excavatorVirtualCamera = cameras[4];
        truckVirtualCamera = cameras[5];

        UI_Scene_StartBtn.onGameStartBtnShut += StartGame;
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
            ExcavatorShowCamera.Priority = 20;
            Messenger.Default.Publish(new NarrationMessage("포크레인은 땅 속에 있는 흙을 팔 수 있어요", "2_포크레인은_땅_속에_있는_흙을_팔_수_있어요"));
            excavatorAni.SetBool("Dig", true);

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
        if (Btn_TwiceIssue)
        {
            Btn_TwiceIssue = false;
            Btns_TruckIntro.transform.DOScale(0.01f, 0.3f)
            .SetEase(Ease.InBack) // 등장 시 OutBack이면 사라질 때는 InBack이 자연스러움
            .OnComplete(() =>
            {
                Btns_ExcavatorIntro.SetActive(false);
            });
            TruckShowCamera.Priority = 10;
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

    public void Btn_RmcNext() //레미콘 다음 버튼
    {
        if (Btn_TwiceIssue)
        {
            Btn_TwiceIssue = false;
            Btns_RmcIntro.SetActive(false);
            DOVirtual.DelayedCall(2f, () => { Btn_TwiceIssue = true; });
        }

        Sequence introSeq4 = DOTween.Sequence();
        Messenger.Default.Publish(new NarrationMessage("레미콘은 시멘트를 넣어줄 수 있어요", "4_레미콘은_시멘트를_넣어줄_수_있어요"));


        //Btns_RmcIntro.SetActive(true);
        //Btns_RmcIntro.transform.DOScale(1f, 0.4f)
        //    .From(0.01f)
        //    .SetEase(Ease.Flash)
        //    .OnComplete(() =>
        //    {
        //        Btns_RmcIntro.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
        //    });
        
    }

    private bool twiceAudioIssue = true;
    public void Btn_ExcavatorAni()
    {
        float digAnimationLength = 3.1f;
        if (twiceAudioIssue)
        {
            Debug.Log("오디오 재생중");
            twiceAudioIssue = false;
            excavatorAni.SetBool("Dig", true);
            Managers.Sound.Play(SoundManager.Sound.Narration, HeavyMachinerySound);
            DOVirtual.DelayedCall(0.1f, () => excavatorAni.SetBool("Dig", false));
            DOVirtual.DelayedCall(digAnimationLength, () => { Managers.Sound.Stop(SoundManager.Sound.Narration); twiceAudioIssue = true; });
        }
    }

    public void Btn_TruckAni()
    {
        float digAnimationLength = 3.75f;
        truckAni.SetBool("LiftDown", false);
        truckAni.SetBool("LiftUp", true);
        Managers.Sound.Play(SoundManager.Sound.Narration, HeavyMachinerySound);
        DOVirtual.DelayedCall(0.1f, () => {
            truckAni.SetBool("LiftUp", false);
            truckAni.SetBool("LiftDown", true);
        });
        DOVirtual.DelayedCall(digAnimationLength, () => Managers.Sound.Stop(SoundManager.Sound.Narration));
    }

    public void PlayNarration(int path)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Construction/Audio/audio_{path}");
        Managers.Sound.Play(SoundManager.Sound.Narration, clip);
        DOVirtual.DelayedCall(clip.length, () => { });

    }


}
