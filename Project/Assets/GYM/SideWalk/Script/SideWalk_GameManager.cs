using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using UnityEngine;

public class SideWalk_GameManager : Base_GameManager
{
    private enum EventStage
    {
        Intro, CarExample, HumanExample, Road, SideWalk, CrossRoad, EndSideWalk, EndRoad
    }
    private EventStage _stage = EventStage.Intro;

    public List<CinemachineVirtualCamera> cams = new List<CinemachineVirtualCamera>(7);

    private MoveFunction[] movers;

    [SerializeField] private GameObject eventCar;
    [SerializeField] private GameObject eventChild;

    private Animator eventChildAnim;

    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        movers = FindObjectsOfType<MoveFunction>();
        // 씬에 있는 모든 MoveFunction 컴포넌트 찾기

        eventCar.SetActive(false);
        eventChild.SetActive(false);

        eventChildAnim = eventChild.GetComponent<Animator>();

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

        UI_InScene_StartBtn.onGameStartBtnShut += StartGame;
    }

    private void StartGame()
    {
        OnGameStart();
    }

    public void OnGameStart()
    {
        if (_stage == EventStage.Intro)
            NextStage(EventStage.Intro);

    }

    private void NextStage(EventStage next)
    {
        _stage = next;
        switch (next)
        {
            case EventStage.Intro: StartIntroStage(); break;
            case EventStage.HumanExample: StartHumanExampleStage(); break;
            case EventStage.CarExample: StartCarExampleStage(); break;
            case EventStage.Road: StartRoadStage(); break;
            case EventStage.SideWalk: StartSideWalkStage(); break;
            case EventStage.CrossRoad: StartCrossRoadStage(); break;
            case EventStage.EndSideWalk: EndSceneSideWalkStage(); break;
            case EventStage.EndRoad: EndSceneRoadStage(); break;
        }

        Logger.Log($"{next}스테이지로 변경");
    }

   

    private void StartIntroStage()
    {
        Sequence sequence = DOTween.Sequence()
            .AppendCallback(() =>
            {
                TriggerAllMoves();
                Messenger.Default.Publish(new NarrationMessage("차도와 인도는 달라요", "audio_0_차도와_인도는_달라요"));
            })
            .AppendInterval(4.5f)
            .AppendCallback(() =>
            {
                cams[0].Priority = 9;
                cams[1].Priority = 11;
                Messenger.Default.Publish(new NarrationMessage("사람들이 다니는 길과 차가\n다니는길을 알아볼까요?", "audio_1_사람들이_다니는_길과_차가_다니는길을_알아볼까요_"));
            })
            .AppendInterval(6f)
            .AppendCallback(() => NextStage(EventStage.HumanExample));

    }

    private void StartHumanExampleStage()
    {
        Sequence sequence = DOTween.Sequence()
            .AppendCallback(() =>
            {
                eventChild.SetActive(true);
                eventCar.SetActive(true);
                eventCar.transform.DOMoveX(eventCar.transform.position.x + 5f, 2f).SetEase(Ease.OutQuad);
                eventChild.transform.DOMoveX(eventChild.transform.position.x - 5f, 2f).SetEase(Ease.OutQuad);
            })
            .AppendInterval(2f)
            .AppendCallback( () => 
            {
                Messenger.Default.Publish(new NarrationMessage("멈춰요!", "audio_2_삐__멈춰요_"));
                //화면에 위험 손바닥 표기
            })
            .AppendInterval(5f)
            .AppendCallback(() =>
            {
                Messenger.Default.Publish(new NarrationMessage("사람은 인도로 다녀요", "audio_3_사람은_인도로_다녀요_"));
                eventChildAnim.SetTrigger("GetOutRoad");

                //아이는 인도로 다시 이동 차는 다시 주행 -> 다음으로 버튼 화면에 활성화

            })


            ;
                
                //NextStage(EventStage.CarExample);
            }

    private void StartCarExampleStage()
    {
        //차 1대가 보도로 넘어와 아바타와 부딪힐뻔한 상황 연출
        //삐! 멈춰요!
        //연출해소
        //차는 도로로 다녀요! ->다음으로 버튼 활성화


        //NextStage(EventStage.Road);
    }

    private void StartRoadStage()
    {
        //자동차가 가는 길이 없어졌어요! 다시만들어볼까요?
        //차도 블록을 터치해서 차도를 만들어주세요
        //도로 블랙홀에 사라지는듯한? 연출
        //2d로 도로 조각난거 보여주면서 게임 시작
        //총 7조각 각각 1회 클릭 7개 다 클릭하면 종료
        //완성 후 2d종료 다시 블랙홀에 생성되는 듯한? 연출
        //성공 효과음 및 차가 다시 쌩쌩다니는 듯한 연출
        //완성! 차는 도로로 다녀요!



        //NextStage(EventStage.SideWalk);
    }

    private void StartSideWalkStage()
    {
        //위 상황과 동일



        //NextStage(EventStage.CrossRoad);
    }


    private void StartCrossRoadStage()
    {
        //횡단보도는 고려중


        //NextStage(EventStage.EndSideWalk);
    }

   

    private void EndSceneSideWalkStage()
    {
        //마지막으로 오늘 배운걸 다시 한번 볼까요?
        //기존 차가 지나다니는 1번상황에서 인도를 스케일 조정으로 눈에 띄게 표시
        //하단에 텍스트로 "인도" 표시 , 다음으로 버튼


        //NextStage(EventStage.EndRoad);
    }

    private void EndSceneRoadStage()
    {
        //마지막으로 오늘 배운걸 다시 한번 볼까요?
        //기존 차가 지나다니는 1번상황에서 인도를 스케일 조정으로 눈에 띄게 표시
        //하단에 텍스트로 "인도" 표시 , 다음으로 버튼


    }


    public void TriggerAllMoves() //시작할때 사람이랑 자동차들 움직이게 시작하는 함수
    {
        foreach (var m in movers)
        {
            m.BeginMove();
        }
    }

}
