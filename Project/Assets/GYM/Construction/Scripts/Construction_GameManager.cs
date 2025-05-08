using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

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
    public CinemachineVirtualCamera excavatorVirtualCamera;
    public CinemachineVirtualCamera truckVirtualCamera;
    public CinemachineVirtualCamera rmcVirtualCamera;

    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

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
        excavatorVirtualCamera = cameras[2];
        truckVirtualCamera = cameras[3];

        UI_Scene_StartBtn.onGameStartBtnShut += StartGame;
    }

    private void StartGame()
    {
        //게임 시작 메서드
        //나레이션
        Sequence seq = DOTween.Sequence();
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("공사장에서 일하는 자동차들이 건물을 짓고있어요", "0_공사장에서_일하는_자동차들이_건물을_짓고있어요_"));
        });
        seq.AppendInterval(6f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("일하는 자동차들을 도와줘볼까요", "1_일하는_자동차들을_도와줘볼까요_"));
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("포크레인은 땅 속에 있는 흙을 팔 수 있어요", "2_포크레인은_땅_속에_있는_흙을_팔_수_있어요"));
        });
        seq.AppendInterval(6f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("트럭은 많은 흙을 옮겨 줄 수 있어요", "3_트럭은_많은_흙을_옮겨_줄_수_있어요"));
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("레미콘은 시멘트를 넣어줄 수 있어요", "4_레미콘은_시멘트를_넣어줄_수_있어요"));
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("앗 자동차들이 작동을 멈췄어요", "5_앗__자동차들이_작동을_멈췄어요_"));
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("우리 다같이 자동차들을 도와줘볼까요", "6_우리_다같이_자동차들을_도와줘볼까요_"));
        });
        seq.AppendInterval(5f);
        seq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("먼저 포크레인이 움직일 수 있게 터치해주세요", "7_먼저_포크레인이_움직일_수_있게_터치해주세요"));
        });
        seq.AppendInterval(5f);

        //카메라 이동        
        DOVirtual.DelayedCall(2f, () =>
        {
            startVirtualCamera.Priority = 10;
            introVirtualCamera.Priority = 20;
        });
        //셋다 idle 시동 상태
        //차례대로 애니메이션 재생 한번씩 하고

        //포크레인을 도와주세요
        //포크레인 클릭

    }

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync() || !isStartButtonClicked) return;

        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            string objectName = hit.collider.gameObject.name;
            //포크레인 -> 트럭 -> 레미콘 순으로 진행하게끔 
            if (objectName.Contains("Excavator") && !isExcavatorStage)
            {
                isExcavatorStage = true;
                Debug.Log("포크레인 스테이지 이동");
                introVirtualCamera.Priority = 10;
                excavatorVirtualCamera.Priority = 20;
                //포크레인 스테이지 시작
                return;
            }
            if (objectName.Contains("Truck") && isExcavatorStage && !isTruckStage)
            {
                isTruckStage = true; //중복방지용
                Debug.Log("트럭 스테이지 이동");
                //카메라 이동
                //트럭 스테이지 시작
                return;
            }
            if (objectName.Contains("Rmc") && isExcavatorStage && isTruckStage && !isRmcStage)
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


}
