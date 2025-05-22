using UnityEngine;
using DG.Tweening;
using System.Runtime.CompilerServices;
using Cinemachine;
using UnityEngine.UI;

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

    [SerializeField] private CinemachineVirtualCamera normalCamera;
    [SerializeField] private CinemachineVirtualCamera eventCamera;

    [SerializeField] private float carMoveSpeed = 4f;

    [SerializeField] private float eventTime;

    [SerializeField] private bool playingGame = false;          //게임중 체크용 불값
    [SerializeField] private bool isEventOn = false;            //초록불 이벤트 발생 감지용 불값
        
    [SerializeField] private bool LoseEventOn = false;          //횡단보도 패배 신호 감지용 불값
    [SerializeField] private bool gameover = false;             //게임 오버 불값
        
    [SerializeField] private Image GameOverBG;                  //드래그앤드롭

    private Sequence eventSeq;
    
    private Vector3 oriRedCarPosition = new Vector3(-277.89f, 34.67128f, 102.7547f);
    private Vector3 oriBlueCarPosition = new Vector3(-264.09f, 34.67128f, 107.41f);
    private Vector3 eventStartPosition = new Vector3(-360.4842529296875f, 34.6712760925293f, 102.813720703125f);

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

        if (normalCamera == null || eventCamera == null)
        {
            var cams = GetComponentsInChildren<CinemachineVirtualCamera>();
            normalCamera = cams[0];
            eventCamera = cams[1];
        }

        //Messenger.Default.Publish(new NarrationMessage("공사장에서 일하는 자동차들이 건물을 짓고있어요", "0_공사장에서_일하는_자동차들이_건물을_짓고있어요_"));



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


        UI_Scene_StartBtn.onGameStartBtnShut += StartGame;
    }

    private void StartGame()
    {
        _lightController.ChangeTrafficLight();
        _lightController.OnLightChanged += LightChanged;

        //bg음악 시작

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
        if (isEventOn)
        {
            Debug.Log("이벤트발생중이라서 색이바뀌지않았습니다");
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
        level += 1;
        redcar.SetActive(true);
        bluecar.SetActive(true);

        Debug.Log("차량이동시작");
        redcar.transform.DOMove(redTarget.position, carMoveSpeed + 1).SetEase(Ease.InQuad).OnComplete(() =>
        {
            redcar.transform.position = oriRedCarPosition;
        });
        DOVirtual.DelayedCall(3f, () =>
        {
            bluecar.transform.DOMove(blueTarget.position, carMoveSpeed - 2).SetEase(Ease.InQuad).OnComplete(() =>
            {
                bluecar.transform.position = oriBlueCarPosition;
            });
        });
    }

    private void DoOnGreenLight()
    {
        if (level > 3 && randomIndex >= spawnChanceGreen)
        {
            //초록불 차량 이벤트 발생
            isEventOn = true;
            LoseEventOn = false;

            Sequence seq = DOTween.Sequence()
                .AppendInterval(5f)
                .AppendCallback(() =>
                {
                    redcar.SetActive(false);
                    eventcar.SetActive(true);
                    eventcar.transform.position = eventStartPosition;
                    eventCamera.Priority = 12;
                    normalCamera.Priority = 10;
                    eventcar.transform.DOMove(eventStopTransform.position, 7f).SetEase(Ease.Linear);
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
                    EventGame();
                });

        }
        else
        {
            redcar.transform.DOMove(redStopTransform.position, carMoveSpeed).SetEase(Ease.OutQuad);
            bluecar.transform.DOMove(blueStopTransform.position, carMoveSpeed).SetEase(Ease.OutQuad);
        }
    }



    private void EventGame()
    {
        eventSeq = DOTween.Sequence()
                .SetAutoKill(false);

        eventSeq.AppendCallback(() =>
            {
                Debug.Log("10초안에 밖으로 나가세요!");
                //제한시간안에 밖으로 나가야함
                //나레이션
                //효과음 
                //시간제한 체크용 ui
            })
            .AppendInterval(10f)
            .AppendCallback(() =>
            {
                Debug.Log("게임 종료 감지 시작");
                Debug.Log("초록불일때도 차가 올수있어요!");
                LoseEventOn = true;
                eventcar.transform.DOMove(redTarget.position, carMoveSpeed).SetEase(Ease.InQuad);
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                eventcar.SetActive(false);
                LoseEventOn = false;
                isEventOn = false;
            });
    }


    private void LoseGame()
    {
        Debug.Log("게임 패배");
        Sequence seq = DOTween.Sequence()
            .AppendCallback(() =>
            {
                playingGame = false;
                GameOverBG.DOFade(1f, 1f);
                isEventOn = true;
                LoseEventOn = false;
                //패배 효과음
                Debug.Log("횡단보도를 건너야 할 때는 신호를 보고난 후 좌우도 꼭 살펴야해요");
            })
            .AppendInterval(10f)
            .AppendCallback(() =>
            {
                isEventOn = false;
                //패배 효과음
                //게임 리셋
                GameOverBG.DOFade(0f, 1f);
            });

    }
}