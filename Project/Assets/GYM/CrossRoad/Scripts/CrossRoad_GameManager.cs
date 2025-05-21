using UnityEngine;
using DG.Tweening;
using System.Runtime.CompilerServices;

public class CrossRoad_GameManager : Base_GameManager
{
    private RaycastHit[] _hits;

    [SerializeField] private TrafficLightController _lightController; //에디터에서 연결 우선

    [SerializeField] private int level = 1;
    [SerializeField] private int spawnChanceGreen = 30;
    [SerializeField] private int randomIndex;

    [SerializeField] private GameObject redcar;              //드래그앤드롭
    [SerializeField] private GameObject bluecar;             //드래그앤드롭
    [SerializeField] private GameObject eventcar;             //드래그앤드롭

    [SerializeField] private Transform redTarget;            //드래그앤드롭
    [SerializeField] private Transform blueTarget;           //드래그앤드롭

    [SerializeField] private Transform redStopTransform;     //드래그앤드롭
    [SerializeField] private Transform blueStopTransform;    //드래그앤드롭
    [SerializeField] private Transform eventStopTransform;    //드래그앤드롭

    [SerializeField] private float carMoveSpeed = 4f;

    private Vector3 oriRedCarPosition = new Vector3(-277.89f, 34.67128f, 102.7547f);
    private Vector3 oriBlueCarPosition = new Vector3(-264.09f, 34.67128f, 107.41f);
    private Vector3 eventStartPosition = new Vector3(-313.17999267578127f, 34.6712760925293f, 102.813720703125f);

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
            if (hit.collider.CompareTag("toWork"))
            {
                switch (CurrentColor)
                {
                    case LightColor.Red:
                        LoseGame();
                        return;
                    case LightColor.Green:
                        //위치 파악용 이펙트 발생
                        return;
                }

                return;
            }
        }
    }

    private void LightChanged(LightColor color)
    {
        CurrentColor = color;
        randomIndex = Random.Range(0, 101);
        switch (color)
        {
            case LightColor.Red:
                // 빨간불일 때 실행할 로직
                Debug.Log("현재 빨간불");
                DoOnRedLight();
                break;
            case LightColor.Green:
                // 초록불일 때
                Debug.Log("현재 초록불");
                Debug.Log($"{randomIndex}");
                DoOnGreenLight();
                break;
        }
    }

    private void DoOnRedLight()
    {
        //양쪽 자동차가 시간차를 두고 지나가는 모습 
        Debug.Log("차량이동시작");
        redcar.transform.DOMove(redTarget.position, carMoveSpeed).SetEase(Ease.InQuad).OnComplete(() =>
        {
            redcar.transform.position = oriRedCarPosition;
        });

        bluecar.transform.DOMove(blueTarget.position, carMoveSpeed).SetEase(Ease.InQuad).OnComplete(() =>
        {
            bluecar.transform.position = oriBlueCarPosition;
        });
    }

    private void DoOnGreenLight()
    {
        if (level == 1)
        {
            //양쪽 자동차가 시간차를 두고 횡단보도 앞에 멈추는 모습
            redcar.transform.DOMove(redStopTransform.position, carMoveSpeed).SetEase(Ease.OutQuad);
            bluecar.transform.DOMove(blueStopTransform.position, carMoveSpeed).SetEase(Ease.OutQuad);
        }
        else if (level == 2 && randomIndex <= spawnChanceGreen)
        {
            //초록불 2레벨 차량 이벤트 발생
            eventcar.SetActive(true);
            eventStopTransform.DOMove(eventStopTransform.position, 4f).SetEase(Ease.Linear);
            //카메라로 이벤트 차량 따라서 보기 lookat으로
            //나레이션으로 위협감지 후 카메라 게임스테이지로
            //제한 시간 안에 횡단보도에서 나와야함
            //제한 시간 종료 후 안내 나레이션
        }
    }

    private void LoseGame()
    {
        Debug.Log("게임 패배");
        //빨간불인데 횡단보도에 있음
        //초록불이지만 이벤트 발생 후 제한시간 이내에 횡단보도에서 나가지 못함

        //화면 암전
        //게임 리셋
        //패배 나레이션 (패배 사유가 2개인데 사유마다 나레이션을 다르게 해야하나?)
        //패배 효과음
    }
}