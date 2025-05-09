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

    [SerializeField] private Animator excavatorAni;
    [SerializeField] private Animator truckAni;
    [SerializeField] private Animator rmcAni;

    [SerializeField] private bool isExcavatorStage = false;
    [SerializeField] private bool isTruckStage = false;
    [SerializeField] private bool isRmcStage = false;

    public List<CinemachineVirtualCamera> cameras = new List<CinemachineVirtualCamera>(5);

    public CinemachineVirtualCamera startVirtualCamera;
    public CinemachineVirtualCamera introVirtualCamera;
    public CinemachineVirtualCamera ExcavatorShowCamera;
    public CinemachineVirtualCamera TruckShowCamera;
    public CinemachineVirtualCamera excavatorVirtualCamera;
    public CinemachineVirtualCamera truckVirtualCamera;
    public CinemachineVirtualCamera rmcVirtualCamera;

    private bool isIntroSceneEnd = false;

    public GameObject Btns_ExcavatorIntro;
    public GameObject Btns_TruckIntro;
    public GameObject Btns_RmcIntro;

    public GameObject Btn_Excavator;
    public GameObject Btn_Truck;
    public GameObject Btn_Rmc;

    public bool excavatorStageEnd = false;
    public bool truckStageEnd = false;
    public bool rmcStageEnd = false;

    public AudioClip victoryAuidoClip;

    public GameObject ExcavatorStageSoil;
    private Vector3 originExcavatorStageSoilScale = new Vector3(15, 14, 14);
    

    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);
        Managers.Sound.Play(SoundManager.Sound.Bgm, "Construction/Audio/audio_BGM");

        ExcavatorStageSoil.transform.localScale = originExcavatorStageSoilScale;

        victoryAuidoClip = Resources.Load<AudioClip>("Construction/Audio/audio_Victory");

        Btns_ExcavatorIntro.SetActive(false);
        Btns_RmcIntro.SetActive(false);
        Btns_TruckIntro.SetActive(false);
        Btn_Excavator.SetActive(false);
        Btn_Truck.SetActive(false);
        Btn_Rmc.SetActive(false);

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
        });
        introSeq.AppendInterval(0.1f);
        introSeq.AppendCallback(() =>
        {
            excavatorAni.SetBool("Dig", false);
        });

    }

    public void ExcavatorNextBtnClicked()
    {
        Sequence introSeq2 = DOTween.Sequence();

        introSeq2.AppendCallback(() =>
        {
            ExcavatorShowCamera.Priority = 10;
            TruckShowCamera.Priority = 20;
            Messenger.Default.Publish(new NarrationMessage("트럭은 많은 흙을 옮겨 줄 수 있어요", "3_트럭은_많은_흙을_옮겨_줄_수_있어요"));
            truckAni.SetBool("LiftUp", true);
        });
        introSeq2.AppendInterval(1f);
        introSeq2.AppendCallback(() =>
        {
            truckAni.SetBool("LiftUp", false);
            truckAni.SetBool("LiftDown", true);
        });
    }

    public void TruckNextBtnClicked()
    {
        Sequence introSeq3 = DOTween.Sequence();
        introSeq3.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("레미콘은 시멘트를 넣어줄 수 있어요", "4_레미콘은_시멘트를_넣어줄_수_있어요"));
        });
    }

    public void RmcNextBtnClicked()
    {
        Sequence introSeq4 = DOTween.Sequence();

        introSeq4.AppendCallback(() =>
        {
            TruckShowCamera.Priority = 10;
            introVirtualCamera.Priority = 20;
            Messenger.Default.Publish(new NarrationMessage("앗 자동차들이 작동을 멈췄어요", "5_앗__자동차들이_작동을_멈췄어요_"));
        });
        introSeq4.AppendInterval(5f);
        introSeq4.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("우리 다같이 자동차들을 도와줘볼까요", "6_우리_다같이_자동차들을_도와줘볼까요_"));
        });
        introSeq4.AppendInterval(5f);
        introSeq4.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("먼저 포크레인이 움직일 수 있게 터치해주세요", "7_먼저_포크레인이_움직일_수_있게_터치해주세요"));
        });
        introSeq4.AppendInterval(3f);
        introSeq4.AppendCallback(() => isIntroSceneEnd = true);
    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync() || !isStartButtonClicked) return;

        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            string objectName = hit.collider.gameObject.name;
            //포크레인 -> 트럭 -> 레미콘 순으로 진행하게끔 
            if (objectName.Contains("Excavator") && !isExcavatorStage && isIntroSceneEnd)
            {
                isExcavatorStage = true;
                introVirtualCamera.Priority = 10;
                excavatorVirtualCamera.Priority = 20;
                DOVirtual.DelayedCall(4f, () =>
                {
                    Messenger.Default.Publish(new NarrationMessage("포크레인으로 흙을 파봐요", "8_포크레인으로_흙을_파봐요_"));
                    Btn_Excavator.SetActive(true);
                });
                return;
            }
            if (objectName.Contains("Truck") && isExcavatorStage && !isTruckStage && isIntroSceneEnd)
            {
                isTruckStage = true; //중복방지용
                Debug.Log("트럭 스테이지 이동");
                //카메라 이동
                //트럭 스테이지 시작
                return;
            }
            if (objectName.Contains("Rmc") && isExcavatorStage && isTruckStage && !isRmcStage && isIntroSceneEnd)
            {
                isRmcStage = true; //중복방지용
                Debug.Log("레미콘 스테이지 이동");
                //카메라 이동
                //레미콘 스테이지 시작
                return;
            }

        }

    }


    public void PlayNarration(int path)
    {
        AudioClip clip = Resources.Load<AudioClip>($"Construction/Audio/audio_{path}");
        Managers.Sound.Play(SoundManager.Sound.Narration, clip);
        DOVirtual.DelayedCall(clip.length, () => { });

    }

    public void Btn_IntroNext() //포크레인 다음 버튼
    {
        ExcavatorNextBtnClicked();
        Btns_ExcavatorIntro.SetActive(false);
        Btns_TruckIntro.SetActive(true);
    }

    public void Btn_IntroNext2() //트럭 다음 버튼
    {
        TruckNextBtnClicked();
        Btns_TruckIntro.SetActive(false);
        Btns_RmcIntro.SetActive(true);
    }

    public void Btn_IntroNext3() //레미콘 다음 버튼
    {
        RmcNextBtnClicked();
        Btns_RmcIntro.SetActive(false);
    }

    public void Btn_ExcavatorAni()
    {
        excavatorAni.SetBool("Dig", true);
        DOVirtual.DelayedCall(0.1f, () => excavatorAni.SetBool("Dig", false));
    }

    public void Btn_TruckAni()
    {
        truckAni.SetBool("LiftDown", false);
        truckAni.SetBool("LiftUp", true);
        DOVirtual.DelayedCall(0.1f, () => {
            truckAni.SetBool("LiftUp", false);
            truckAni.SetBool("LiftDown", true);
        });
    }


}
