using UnityEngine;
using DG.Tweening;
using Cinemachine;
using UnityEngine.UI;
using MyGame.Messages;
using SuperMaxim.Messaging;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;

public class CrossRoad_GameManager : Base_GameManager
{
    private RaycastHit[] _hits;

    [SerializeField] private TrafficLightController _lightController; //에디터에서 연결 우선

    [SerializeField] private int level = 0;
    [SerializeField] private int spawnChanceGreen = 30;
    [SerializeField] private int randomIndex;

    [SerializeField] private GameObject redcar;               //드래그앤드롭
    [SerializeField] private GameObject bluecar;              //드래그앤드롭
    [SerializeField] private GameObject eventcar;             //드래그앤드롭

    [SerializeField] private Transform redTarget;             //드래그앤드롭
    [SerializeField] private Transform blueTarget;            //드래그앤드롭

    [SerializeField] private Transform redStopTransform;      //드래그앤드롭
    [SerializeField] private Transform blueStopTransform;     //드래그앤드롭
    [SerializeField] private Transform eventStopTransform;    //드래그앤드롭

    [SerializeField] private CinemachineVirtualCamera introCamera;
    [SerializeField] private CinemachineVirtualCamera normalCamera;
    [SerializeField] private CinemachineVirtualCamera eventCamera;
    [SerializeField] private CinemachineVirtualCamera endCamera;

    [SerializeField] private float carMoveSpeed = 4f;

    [SerializeField] private float eventTime;

    [SerializeField] private bool playingGame = false;          //게임중 체크용 불값
        
    [SerializeField] private bool LoseEventOn = false;          //횡단보도 패배 신호 감지용 불값
    [SerializeField] private bool gameover = false;             //게임 오버 불값
        
    [SerializeField] private Image GameOverBG;                       //드래그앤드롭

    [SerializeField] private GameObject TrafficSignal;               //드래그앤드롭
    [SerializeField] private Image leftTrafficSignal;                //드래그앤드롭
    [SerializeField] private Image rightTrafficSignal;               //드래그앤드롭

    [SerializeField] private ChildAnimatorController chractors;                   //드래그앤드롭

    private Sprite RedTrafficSignalImg;
    private Sprite GreenTrafficSignalImg;
    private Sequence eventSeq;
    
    private Vector3 oriRedCarPosition = new Vector3(-277.89f, 34.67128f, 102.7547f);
    private Vector3 oriBlueCarPosition = new Vector3(-264.09f, 34.67128f, 107.41f);
    private Vector3 eventStartPosition = new Vector3(-335.8f, 34.67128f, 102.8137f);

    public LightColor CurrentColor
    {
        get;
        private set;
    }

    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        if (_lightController == null)
        {
            _lightController = FindObjectOfType<TrafficLightController>(true);
            Debug.Assert(_lightController != null, "TrafficLightController가 씬에 없습니다");
        }

        eventcar.transform.position = eventStartPosition;
        eventcar.SetActive(false);

        if (introCamera == null|| normalCamera == null || eventCamera == null)
        {
            var cams = GetComponentsInChildren<CinemachineVirtualCamera>();
            introCamera = cams[0];
            normalCamera = cams[1];
            eventCamera = cams[2];
        }

        RedTrafficSignalImg = Resources.Load<Sprite>("CrossRoad/Image/RedTrafficSignal");
        GreenTrafficSignalImg = Resources.Load<Sprite>("CrossRoad/Image/GreenTrafficSignal");

        Managers.Sound.Play(SoundManager.Sound.Bgm, "CrossRoad/Audio/CrossRoad_BGM");


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

        UI_InScene_StartBtn.onGameStartBtnShut -= StartIntro;
        UI_InScene_StartBtn.onGameStartBtnShut += StartIntro;
    }

    protected override void OnDestroy()
    {
        UI_InScene_StartBtn.onGameStartBtnShut -= StartIntro;
        base.OnDestroy();
    }

    private void StartIntro()
    {
        introCamera.Priority = 10;
        normalCamera.Priority = 12;

        //인트로 나레이션
        //Messenger.Default.Publish(new NarrationMessage("10초안에 횡단보도 밖으로 이동해야해요!", "4_10초안에_횡단보도_밖으로_이동해야해요_"));
        //나레이션 타이밍과 맞춰서 시작
        Sequence introSeq = DOTween.Sequence()
            .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("친구들과 다함께 횡단보도를 건너야해요", "0_친구들과_다함께_횡단보도를_건너야해요_")))
            .AppendInterval(5.5f)
            .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("양쪽 보도에서 시작해서\n반대쪽 횡단보도로 건너주세요", "0_양쪽_보도에서_시작해서_반대쪽_횡단보도로_건너주세요_")))
            .AppendInterval(6f)
            .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("횡단보도 신호를 보고\n올바른 신호에 건너보도록 해요", "1_횡단보도_신호를_보고_올바른_신호에_건너보도록_해요_")))
            .AppendInterval(6f)
            .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("준비~", "1_준비")))
            .AppendInterval(3f)
            .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("시작!", "2_시작")))
            .JoinCallback(()=> StartGame());

    }

    private void StartGame()
    {
        _lightController.ChangeTrafficLight();
        _lightController.OnLightChanged += LightChanged;

        TrafficSignal.SetActive(true);
        TrafficSignal.transform.DOScale(1f, 0.4f)
            .From(0.01f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                TrafficSignal.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
            });

        redcar.transform.position = oriRedCarPosition;
        bluecar.transform.position = oriBlueCarPosition;

    }


    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync() || !isStartButtonClicked) return;

        _hits = Physics.RaycastAll(GameManager_Ray);
        foreach (var hit in _hits)
        {
            if (hit.collider.CompareTag("toWork") && playingGame)
            {
                switch (CurrentColor)
                {
                    case LightColor.Red:
                            LoseGame();
                        return;
                    case LightColor.Green:
                        if (LoseEventOn)
                        {
                            LoseGame();
                            eventSeq.Kill();
                        }
                        else if (!LoseEventOn)
                        {
                            //위치 파악용 이펙트 발생

                        }
                        return;
                }
            }
        }
    }

    private void LightChanged(LightColor color)
    {
        if (level >= 7)     //게임종료
        {
            EndGame();
            return;
        }

        playingGame = true;
        CurrentColor = color;
        randomIndex = Random.Range(0, 101);

        switch (color)
        {
            case LightColor.Red:
                Debug.Log("현재 빨간불");
                DoOnRedLight();
                break;
            case LightColor.Green:
                Debug.Log("현재 초록불");
                Debug.Log($"{randomIndex}");
                DoOnGreenLight();
                break;
        }
    }

    private void DoOnRedLight()
    {
        leftTrafficSignal.sprite = RedTrafficSignalImg;
        rightTrafficSignal.sprite = RedTrafficSignalImg;

        level += 1;
        redcar.SetActive(true);
        bluecar.SetActive(true);


        DOVirtual.DelayedCall(1f, () => Managers.Sound.Play(SoundManager.Sound.Effect, "CrossRoad/Audio/Car_Pass_By_1"));
        redcar.transform.DOMove(redTarget.position, carMoveSpeed + 1).SetEase(Ease.InQuad).OnComplete(() =>
        {
            redcar.transform.position = oriRedCarPosition;
        });

        DOVirtual.DelayedCall(3f, () =>
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "CrossRoad/Audio/Car_Pass_By_2");
            bluecar.transform.DOMove(blueTarget.position, carMoveSpeed - 2).SetEase(Ease.InQuad).OnComplete(() =>
            {
                bluecar.transform.position = oriBlueCarPosition;
            });
        });
    }

    private void DoOnGreenLight()
    {
        leftTrafficSignal.sprite = GreenTrafficSignalImg;
        rightTrafficSignal.sprite = GreenTrafficSignalImg;

        if (level > 3 && randomIndex >= spawnChanceGreen)
        {
            //초록불 차량 이벤트 발생
            LoseEventOn = false;

            Sequence seq = DOTween.Sequence()
                .AppendInterval(5f)
                .AppendCallback(() =>
                {
                    TrafficSignal.transform.DOScale(0.01f, 0.3f)
                    .SetEase(Ease.InBack) // 등장 시 OutBack이면 사라질 때는 InBack이 자연스러움
                    .OnComplete(() =>
                    {
                        TrafficSignal.SetActive(false);
                    });

                    redcar.SetActive(false);
                    eventcar.SetActive(true);
                    eventcar.transform.position = eventStartPosition;
                    eventCamera.Priority = 12;
                    normalCamera.Priority = 10;
                    eventcar.transform.DOMove(eventStopTransform.position, 7f).SetEase(Ease.Linear);
                    Managers.Sound.Play(SoundManager.Sound.Effect, "CrossRoad/Audio/Car_horn_1");
                    DOVirtual.DelayedCall(2f, () => Managers.Sound.Play(SoundManager.Sound.Effect, "CrossRoad/Audio/Car_DriveFast"));
                    Messenger.Default.Publish(new NarrationMessage("차량이 달려오고있어요!", "3_차량이_달려오고있어요_"));
                    _lightController.lightSequence.Pause();
                })
                .AppendInterval(eventTime)
                .AppendCallback(() =>
                {
                    normalCamera.Priority = 12;
                    eventCamera.Priority = 10;
                })
                .AppendInterval(2f)
                .AppendCallback(() =>
                {
                    TrafficSignal.SetActive(true);
                    TrafficSignal.transform.DOScale(1f, 0.4f)
                        .From(0.01f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() =>
                        {
                            TrafficSignal.transform.DOShakeScale(0.2f, 0.2f, 10, 90f);
                        });
                    EventGame();
                });

        }
        else
        {
            redcar.transform.DOMove(redStopTransform.position, carMoveSpeed).SetEase(Ease.OutQuad);
            bluecar.transform.DOMove(blueStopTransform.position, carMoveSpeed).SetEase(Ease.OutQuad);
            chractors.Walk();
        }
    }

    private void EventGame()
    {
        eventSeq = DOTween.Sequence()
                .SetAutoKill(false);

        eventSeq.AppendCallback(() =>
        {
            _lightController.lightSequence.Play();
            Messenger.Default.Publish(new NarrationMessage("10초안에 횡단보도 밖으로 이동해야해요!", "4_10초안에_횡단보도_밖으로_이동해야해요_"));
        })
            .AppendInterval(11f)
            .AppendCallback(() =>
            {
                Debug.Log("게임 종료 감지 시작");
                LoseEventOn = true;
                eventcar.transform.DOMove(redTarget.position, carMoveSpeed - 2f).SetEase(Ease.InQuad);
                Managers.Sound.Play(SoundManager.Sound.Effect, "CrossRoad/Audio/Car_DriveFast");
                Messenger.Default.Publish(new NarrationMessage("초록불일때도 차량이 올 수 있어요!", "5_초록불일때도_차량이_올_수_있어요_"));

            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                eventcar.SetActive(false);
                LoseEventOn = false;
            });
    }            

    private void LoseGame()
    {
        Sequence seq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                playingGame = false;
                GameOverBG.DOFade(1f, 1f);
                _lightController.lightSequence.Pause();
                LoseEventOn = false;
                Managers.Sound.Play(SoundManager.Sound.Effect, "CrossRoad/Audio/Car_Skid");
                Messenger.Default.Publish(new NarrationMessage("횡단보도를 건너야 할 때는 신호를 보고\n난 후 좌우도 꼭 살펴야해요", "2_횡단보도를_건너야_할_때는_신호를_보고_난_후_좌우도_꼭_살펴야해요_"));

            })
            .AppendInterval(6f)
            .AppendCallback(() =>
            {
                //패배 효과음

                GameOverBG.DOFade(0f, 1f);
                _lightController.lightSequence.Play();
            });

    }

    private void EndGame()
    {

        _lightController.OnLightChanged -= LightChanged;
        playingGame = false;
        Sequence endSeq = DOTween.Sequence()
            .Append(GameOverBG.DOFade(1f, 1f))
            .AppendCallback(() =>
            {
                endCamera.Priority = 20;
                chractors.End();
                
                TrafficSignal.transform.DOScale(0.01f, 0.3f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        TrafficSignal.SetActive(false);
                    });
                Messenger.Default.Publish(new NarrationMessage("친구들과 함께 횡단보도를 건너줘서 고마워", "6_친구들과_함께_횡단보도를_건너줘서_고마워_"));
                
            })
            .AppendInterval(4f)
            .Append(GameOverBG.DOFade(0f, 1f))
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("지금처럼 신호를 보고 난 후\n좌우도 꼭 살피고 안전하게 건너야해", "7_지금처럼_신호를_보고_난_후_좌우도_꼭_살피고_안전하게_건너야해_"));

            });

    }


}