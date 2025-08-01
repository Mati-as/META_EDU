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

    public List<CinemachineVirtualCamera> cameras;

    public CinemachineVirtualCamera startVirtualCamera;
    public CinemachineVirtualCamera introVirtualCamera;

    public CinemachineVirtualCamera excavatorShowCamera;
    public CinemachineVirtualCamera truckShowCamera;
    public CinemachineVirtualCamera bulldozerShowCamera;

    public CinemachineVirtualCamera excavatorVirtualCamera;
    public CinemachineVirtualCamera truckVirtualCamera;
    public CinemachineVirtualCamera bulldozerVirtualCamera;

    public CinemachineVirtualCamera endVirtualCamera;

    public GameObject btnExcavatorIntro;
    public GameObject btnTruckIntro;
    public GameObject btnBulldozerIntro;

    public GameObject btnExcavatorStage;
    public GameObject btnTruckStage;
    public GameObject btnBulldozerStage;

    public AudioClip victoryAudioClip;

    public GameObject excavatorStageSoil;
    public GameObject truckStageSoil;
    public GameObject bulldozerStageSoil;

    private readonly Vector3 _originExcavatorStageSoilScale = new Vector3(8.37f, 18f, 7.8f);

    private bool _btnTwiceIssue = true;

    public AudioClip audioClipMove1;
    public AudioClip audioClipMove2;
    public AudioClip audioClipWork1;
    public AudioClip audioClipWork2;

    public AudioClip heavyMachinerySound; //지워야함 오류생겨서 일단 살린것임

    public Construction_UIManager constructionUIManager;

    public bool canNextBtnClick = false;

    public GameObject endBuilding;
    public void ReloadScene()
    {
        RestartScene(delay: 8);
    }
    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        //BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);
        Managers.Sound.Play(SoundManager.Sound.Bgm, "Construction/Audio/audio_BGM");

        excavatorStageSoil.transform.localScale = _originExcavatorStageSoilScale;
        truckStageSoil.transform.localScale = _originExcavatorStageSoilScale;
        bulldozerStageSoil.transform.localScale = Vector3.zero;

        victoryAudioClip = Resources.Load<AudioClip>("Construction/Audio/audio_Victory");

        audioClipMove1 = Resources.Load<AudioClip>("Construction/Audio/audio_move1");
        audioClipMove2 = Resources.Load<AudioClip>("Construction/Audio/audio_move2");
        audioClipWork1 = Resources.Load<AudioClip>("Construction/Audio/audio_work1");
        audioClipWork2 = Resources.Load<AudioClip>("Construction/Audio/audio_work2");

        btnExcavatorIntro.SetActive(false);
        btnBulldozerIntro.SetActive(false);
        btnTruckIntro.SetActive(false);

        btnExcavatorStage.SetActive(false);
        btnTruckStage.SetActive(false);
        btnBulldozerStage.SetActive(false);

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

        var foundCameras = GetComponentsInChildren<CinemachineVirtualCamera>();
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
        endVirtualCamera = cameras[8];

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
        var introSeq = DOTween.Sequence();

        introSeq.AppendCallback(() =>
        {
            startVirtualCamera.Priority = 10;
            introVirtualCamera.Priority = 20;
            Messenger.Default.Publish(new NarrationMessage("일하는 자동차들이 건물을 짓고있어요", "0_공사장에서_일하는_자동차들이_건물을_짓고있어요_"));
        });
        introSeq.AppendInterval(6f);
        introSeq.AppendCallback(() =>
        {
            Messenger.Default.Publish(new NarrationMessage("일하는 자동차들을 도와줘요", "0_일하는_자동차를_도와줘요"));
        });
        introSeq.AppendInterval(5f);
        introSeq.AppendCallback(() =>
        {
            introVirtualCamera.Priority = 10;
            excavatorShowCamera.Priority = 20;
            Messenger.Default.Publish(new NarrationMessage("포크레인은 땅 속에 있는 흙을 팔 수 있어요", "2_포크레인은_땅_속에_있는_흙을_팔_수_있어요"));
            excavatorAni.SetBool("Dig", true);
            Managers.Sound.Play(SoundManager.Sound.Effect, heavyMachinerySound);
            DOVirtual.DelayedCall(3.2f, () => { Managers.Sound.Stop(SoundManager.Sound.Effect); twiceAudioIssue = true; });

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
            btnExcavatorIntro.SetActive(true);
            btnExcavatorIntro.transform.DOScale(1f, 0.4f)
           .From(0.01f) // 원래 자리에서 작게 시작해서 커지도록
           .SetEase(Ease.OutBack)
           .OnComplete(() =>
           {
               btnExcavatorIntro.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
               canNextBtnClick = true;
           });
            Messenger.Default.Publish(new NarrationMessage("동작 버튼을 눌러 포크레인을 움직여주세요!", "1_동작_버튼을_눌러_포크레인을_움직여주세요"));
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
        if (canNextBtnClick)
        {
            constructionUIManager.ForceCloseNarration();
            ClickSound();

            if (_btnTwiceIssue)
            {
                _btnTwiceIssue = false;
                btnExcavatorIntro.transform.DOScale(0.01f, 0.3f)
                .SetEase(Ease.InBack) // 등장 시 OutBack이면 사라질 때는 InBack이 자연스러움
                .OnComplete(() =>
                {
                    btnExcavatorIntro.SetActive(false);
                });
                introVirtualCamera.Priority = 10;
                excavatorVirtualCamera.Priority = 20;

                var seq = DOTween.Sequence();
                seq.AppendInterval(3f);
                seq.AppendCallback(() =>
                {
                    Messenger.Default.Publish(new NarrationMessage("흙이 많이 쌓여있어요!\n포크레인으로 흙을 파요", "2_흙이_많이_쌓여있어요_포크레인으로_흙을_파요"));
                    btnExcavatorStage.SetActive(true);
                    btnExcavatorStage.transform.localScale = Vector3.zero;
                    _btnTwiceIssue = true;
                });
                seq.AppendInterval(5f);
                seq.Append(btnExcavatorStage.transform.DOScale(Vector3.one, 1f));
                seq.Append(btnExcavatorStage.transform.DOShakeScale(0.3f, 0.1f));
                seq.AppendCallback(() =>
                {
                    Messenger.Default.Publish(new NarrationMessage("운전대를 터치해요", "3_운전대를_터치해요"));

                });
                seq.AppendInterval(2f);
                seq.AppendCallback(() =>
                {
                    canNextBtnClick = false;
                });
            }
        }
    }

    public void Btn_TruckNext() //트럭 다음 버튼
    {
        if (canNextBtnClick)
        {
            ClickSound();
            constructionUIManager.ForceCloseNarration();
            if (_btnTwiceIssue)
            {
                _btnTwiceIssue = false;
                btnTruckIntro.transform.DOScale(0.01f, 0.3f)
                .SetEase(Ease.InBack) // 등장 시 OutBack이면 사라질 때는 InBack이 자연스러움
                .OnComplete(() =>
                {
                    btnTruckIntro.SetActive(false);
                });
                truckShowCamera.Priority = 10;
                truckVirtualCamera.Priority = 20;

                var seq = DOTween.Sequence();
                seq.AppendInterval(3f);
                seq.AppendCallback(() =>
                {
                    Messenger.Default.Publish(new NarrationMessage("트럭으로 흙을 옮겨봐요", "12_트럭으로_흙을_옮겨봐요_"));
                    btnTruckStage.SetActive(true);
                    btnTruckStage.transform.localScale = Vector3.zero;
                    _btnTwiceIssue = true;
                });
                seq.Append(btnTruckStage.transform.DOScale(Vector3.one, 1f));
                seq.Append(btnTruckStage.transform.DOShakeScale(0.3f, 0.1f));

                canNextBtnClick = false;
            }
        }
    }

    public void Btn_BulldozerNext() // 다음 버튼
    {
        if (canNextBtnClick)
        {
            ClickSound();
            constructionUIManager.ForceCloseNarration();
            if (_btnTwiceIssue)
            {
                _btnTwiceIssue = false;
                btnBulldozerIntro.transform.DOScale(0.01f, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    btnBulldozerIntro.SetActive(false);
                });
                bulldozerShowCamera.Priority = 10;
                bulldozerVirtualCamera.Priority = 20;

                var seq = DOTween.Sequence();
                seq.AppendInterval(3f);
                seq.AppendCallback(() =>
                {
                    Messenger.Default.Publish(new NarrationMessage("불도저로 흙을 밀어봐요", "22_불도저로_흙을_밀어봐요"));
                    btnBulldozerStage.SetActive(true);
                    btnBulldozerStage.transform.localScale = Vector3.zero;
                    _btnTwiceIssue = true;
                });
                seq.Append(btnBulldozerStage.transform.DOScale(Vector3.one, 1f));
                seq.Append(btnBulldozerStage.transform.DOShakeScale(0.3f, 0.1f));
                canNextBtnClick = false;
            }

        }
    }

    public bool twiceAudioIssue = true;
    public void Btn_ExcavatorAni()
    {
        if (canNextBtnClick)
        {
            ClickSound();
            float animationLength = 2.6f;
            if (twiceAudioIssue)
            {
                twiceAudioIssue = false;
                canNextBtnClick = false;
                excavatorAni.SetBool("Dig", true);
                btnExcavatorIntro.transform.transform.DOShakeScale(0.3f, 0.1f);
                DOVirtual.DelayedCall(0.3f, () => btnExcavatorIntro.transform.DOScale(0.01f, 0.5f));
                DOVirtual.DelayedCall(0.8f, () => btnExcavatorIntro.SetActive(false));
                Managers.Sound.Play(SoundManager.Sound.Effect, heavyMachinerySound);
                DOVirtual.DelayedCall(0.1f, () => excavatorAni.SetBool("Dig", false));
                DOVirtual.DelayedCall(animationLength, () =>
                {
                    Managers.Sound.Stop(SoundManager.Sound.Effect);
                    canNextBtnClick = true;
                    twiceAudioIssue = true;
                });
            }
            DOVirtual.DelayedCall(animationLength + 0.5f, Btn_ExcavatorNext);
        }
    }

    public void Btn_TruckAni()
    {
        if (canNextBtnClick)
        {
            ClickSound();

            float animationLength = 3f;
            if (twiceAudioIssue)
            {
                twiceAudioIssue = false;
                canNextBtnClick = false;
                btnTruckIntro.transform.transform.DOShakeScale(0.3f, 0.1f);
                DOVirtual.DelayedCall(0.3f, () => btnTruckIntro.transform.DOScale(0.01f, 0.5f));
                DOVirtual.DelayedCall(0.8f, () => btnTruckIntro.SetActive(false));
                truckAni.SetBool("LiftDown", false);
                truckAni.SetBool("LiftUp", true);
                Managers.Sound.Play(SoundManager.Sound.Effect, heavyMachinerySound);
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    truckAni.SetBool("LiftUp", false);
                    truckAni.SetBool("LiftDown", true);
                });
                DOVirtual.DelayedCall(animationLength, () =>
                {
                    Managers.Sound.Stop(SoundManager.Sound.Effect);
                    canNextBtnClick = true;
                    twiceAudioIssue = true;
                });
                DOVirtual.DelayedCall(animationLength + 0.5f, Btn_TruckNext);
            }
        }

    }

    public void Btn_BulldozerAni()
    {
        if (canNextBtnClick)
        {
            ClickSound();
            float animationLength = 2f;
            if (twiceAudioIssue)
            {
                twiceAudioIssue = false;
                canNextBtnClick = false;
                bulldozerAni.SetBool("Work", true);
                btnBulldozerIntro.transform.transform.DOShakeScale(0.3f, 0.1f);
                DOVirtual.DelayedCall(0.3f, () => btnBulldozerIntro.transform.DOScale(0.01f, 0.5f));
                DOVirtual.DelayedCall(0.8f, () => btnBulldozerIntro.SetActive(false));
                Managers.Sound.Play(SoundManager.Sound.Effect, heavyMachinerySound);
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    bulldozerAni.SetBool("Work", false);
                });
                DOVirtual.DelayedCall(animationLength, () =>
                {
                    Managers.Sound.Stop(SoundManager.Sound.Effect);
                    canNextBtnClick = true;
                    twiceAudioIssue = true;
                });
                DOVirtual.DelayedCall(animationLength + 0.5f, Btn_BulldozerNext);
            }
        }
    }

    public void PlayNarration(int path)
    {
        var clip = Resources.Load<AudioClip>($"Construction/Audio/audio_{path}");
        Managers.Sound.Play(SoundManager.Sound.Narration, clip);
        DOVirtual.DelayedCall(clip.length, () => { });

    }


    public void ClickSound()
    {
        char click = (char)('A' + Random.Range(0, 6));
        Managers.Sound.Play(SoundManager.Sound.Effect, $"Construction/Audio/Click_{click}", 1f);

    }

}
