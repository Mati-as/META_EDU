using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SideWalk_GameManager : Base_GameManager
{
    private enum EventStage
    {
        Intro,
        CarExample,
        HumanExample,
        Road,
        SideWalk,
        EndSideWalk,
        EndRoad,
        EndScene
    }

    private EventStage _stage = EventStage.Intro;

    private enum GameStage
    {
        SideWalk,
        Road
    }

    [SerializeField] private List<CinemachineVirtualCamera> cams = new List<CinemachineVirtualCamera>(7);

    private MoveFunction[] _movers;

    [SerializeField] private GameObject eventCar;
    [SerializeField] private GameObject eventChild;

    [SerializeField] private Image warningImg;

    [SerializeField] private Button nextCarBtn;
    [SerializeField] private Button nextRoadBtn;
    [SerializeField] private Button nextEndSideWalkBtn;
    [SerializeField] private Button nextEndSceneBtn;

    [SerializeField] private Transform eventChildSideWalkTransform;
    [SerializeField] private Transform eventCarSideWalkTransform;

    [SerializeField] private GameObject blackoutSideWalk;
    [SerializeField] private GameObject blackoutRoad;

    [SerializeField] private SpriteRenderer gameStageBg;
    [SerializeField] private List<SpriteRenderer> sideWalkImg;
    [SerializeField] private List<SpriteRenderer> roadImg;

    public int puzzleCounter;
    //private int bellSoundCounter = 1;

    public Image roadTextBg;
    public Image sidewalkTextBg;
    public TMP_Text roadText;
    public TMP_Text sidewalkText;

    [SerializeField] private List<ParticleSystem> victoryParticles;
    private readonly Color _c = new Color(1, 1, 1, 0);

    [SerializeField] private bool canTouch;

    public Transform carStartPosTf;
    public Transform carMidPoint1Tf;
    public Transform carMidPoint2Tf;
    public Transform carMidPoint3Tf;
    public Transform carEndPosTf;

    public Transform childStartPosTf;
    public Transform childMidPoint1Tf;
    public Transform childMidPoint2Tf;
    public Transform childMidPoint3Tf;
    public Transform childEndPosTf;

    private bool canNext;

    protected override void Init()
    {
        SensorSensitivity = 0.18f;
        //BGM_VOLUME = 0.2f;
        base.Init();
        ManageProjectSettings(150, 0.15f);

        _movers = FindObjectsOfType<MoveFunction>();
        // 씬에 있는 모든 MoveFunction 컴포넌트 찾기

        eventCar.SetActive(false);
        eventChild.SetActive(false);

        warningImg.gameObject.SetActive(false);
        nextCarBtn.gameObject.SetActive(false);
        nextRoadBtn.gameObject.SetActive(false);

        Managers.Sound.Play(SoundManager.Sound.Bgm, "SideWalk/Audio/BGM");

        gameStageBg.color = _c;
        foreach (var v in sideWalkImg)
        {
            v.color = _c;
            v.gameObject.SetActive(false);
        }

        foreach (var v in roadImg)
        {
            v.color = _c;
            v.gameObject.SetActive(false);
        }

        PsResourcePath = "SideWalk/Asset/Fx_Click";
        SetPool();

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


        UI_InScene_StartBtn.onGameStartBtnShut -= StartGame;

        UI_InScene_StartBtn.onGameStartBtnShut += StartGame;
    }

    protected override void OnDestroy()
    {
        UI_InScene_StartBtn.onGameStartBtnShut -= StartGame;
        base.OnDestroy();
    }

    private Vector3 effectPos;

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync() || !isStartButtonClicked) return;

        foreach (var hit in GameManager_Hits)
        {
            if (hit.collider.CompareTag("toWork") && canTouch)
            {
                var mover = hit.collider.GetComponent<ClickableRandomMover>();

                effectPos = hit.point;
                effectPos.z -= 0.2f;
                PlayParticleEffect(effectPos);

                mover.OnMove();

                ClickSound();

                // Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/Bell_" + bellSoundCounter);
                // bellSoundCounter++;

                if (puzzleCounter == 12)
                {
                    canTouch = false;
                    //bellSoundCounter = 1;
                    DOTween.Sequence()
                        .Append(gameStageBg.DOFade(0, 0.5f))
                        .JoinCallback(() =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/audio_Victory");
                            foreach (var particle in victoryParticles)
                            {
                                particle.Play();
                            }
                        })
                        .AppendInterval(0.5f)
                        .Append(blackoutRoad.transform.DOScale(Vector3.one, 2).SetEase(Ease.InQuad))
                        .Join(blackoutRoad.transform.DOLocalRotate(new Vector3(0, 0, 0), 2, RotateMode.FastBeyond360))
                        .AppendCallback(TriggerAllMoves)
                        .AppendInterval(1f)
                        .AppendCallback(() =>
                            Messenger.Default.Publish(new NarrationMessage("차는 도로로 다녀요!", "audio_9_차는_도로로_다녀요_")))
                        .AppendInterval(5f)
                        .AppendCallback(() => NextStage(EventStage.SideWalk));
                }
                else if (puzzleCounter == 24)
                {
                    canTouch = false;
                    DOTween.Sequence()
                        .Append(gameStageBg.DOFade(0, 0.5f))
                        .JoinCallback(() =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/audio_Victory");
                            foreach (var particle in victoryParticles)
                            {
                                particle.Play();
                            }
                        })
                        .AppendInterval(0.5f)
                        .Append(blackoutSideWalk.transform.DOScale(Vector3.one, 2).SetEase(Ease.InQuad))
                        .Join(blackoutSideWalk.transform.DOLocalRotate(new Vector3(0, 0, 0), 2,
                            RotateMode.FastBeyond360))
                        .AppendCallback(TriggerAllMoves)
                        .AppendInterval(1f)
                        .AppendCallback(() =>
                            Messenger.Default.Publish(new NarrationMessage("친구들은 보도로 다녀요!", "audio_12_친구들은_보도로_다녀요_")))
                        .AppendInterval(5f)
                        .AppendCallback(() => NextStage(EventStage.Road))
                        ;
                }
                else if (puzzleCounter == 36)
                {
                    Logger.Log("클릭됨8");
                    canTouch = false;
                    //bellSoundCounter = 1;
                    DOTween.Sequence()
                        .Append(gameStageBg.DOFade(0, 0.5f))
                        .JoinCallback(() =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/audio_Victory");
                            foreach (var particle in victoryParticles)
                            {
                                particle.Play();
                            }
                        })
                        .AppendInterval(0.5f)
                        .Append(blackoutRoad.transform.DOScale(Vector3.one, 2).SetEase(Ease.InQuad))
                        .Join(blackoutRoad.transform.DOLocalRotate(new Vector3(0, 0, 0), 2, RotateMode.FastBeyond360))
                        .AppendCallback(TriggerAllMoves)
                        .AppendInterval(1f)
                        .AppendCallback(() =>
                            Messenger.Default.Publish(new NarrationMessage("차는 도로로 다녀요!", "audio_9_차는_도로로_다녀요_")))
                        .AppendInterval(5f)
                        .AppendCallback(() => NextStage(EventStage.SideWalk));
                }
                else if (puzzleCounter == 48)
                {
                    canTouch = false;
                    DOTween.Sequence()
                        .Append(gameStageBg.DOFade(0, 0.5f))
                        .JoinCallback(() =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/audio_Victory");
                            foreach (var particle in victoryParticles)
                            {
                                particle.Play();
                            }
                        })
                        .AppendInterval(0.5f)
                        .Append(blackoutSideWalk.transform.DOScale(Vector3.one, 2).SetEase(Ease.InQuad))
                        .Join(blackoutSideWalk.transform.DOLocalRotate(new Vector3(0, 0, 0), 2,
                            RotateMode.FastBeyond360))
                        .AppendCallback(TriggerAllMoves)
                        .AppendInterval(1f)
                        .AppendCallback(() =>
                            Messenger.Default.Publish(new NarrationMessage("친구들은 보도로 다녀요!", "audio_12_친구들은_보도로_다녀요_")))
                        .AppendInterval(5f)
                        .AppendCallback(() => NextStage(EventStage.EndRoad))
                        ;
                }
            }
        }
    }

    private void StartGame()
    {
        OnGameStart();
    }

    private void OnGameStart()
    {
        if (_stage == EventStage.Intro)
            NextStage(EventStage.Intro);
        //NextStage(EventStage.EndRoad);
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
            case EventStage.EndRoad: EndSceneRoadStage(); break;
            case EventStage.EndSideWalk: EndSceneSideWalkStage(); break;
            case EventStage.EndScene: EndSceneStage(); break;
        }

        Logger.Log($"{next}스테이지로 변경");
    }



    private void StartIntroStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                TriggerAllMoves();
                Messenger.Default.Publish(new NarrationMessage("차도와 보도는 달라요", "audio_0_차도와_보도는_달라요"));
            })
            .AppendInterval(4.5f)
            .AppendCallback(() =>
            {
                cams[0].Priority = 9;
                cams[1].Priority = 11;
                Messenger.Default.Publish(new NarrationMessage("사람들이 다니는 길과\n차가 다니는길을 알아볼까요?",
                    "audio_1_사람들이_다니는_길과_차가_다니는길을_알아볼까요_"));
            })
            .AppendInterval(6f)
            .AppendCallback(() => NextStage(EventStage.HumanExample));

    }

    private void StartHumanExampleStage()
    {
        var startPos = childStartPosTf.position;
        var midPoint1 = childMidPoint1Tf.position;
        var midPoint2 = childMidPoint2Tf.position;
        var midPoint3 = childMidPoint3Tf.position;
        var endPos = childEndPosTf.position;

        Vector3[] pathPoints = new[]
        {
            startPos,
            midPoint1,
            midPoint2,
            midPoint3,
            endPos
        };

        DOTween.Sequence()
            .AppendCallback(() =>
            {
                eventChild.SetActive(true);
                eventCar.SetActive(true);
                eventCar.transform.DOMoveX(eventCar.transform.position.x + 5f, 2f).SetEase(Ease.OutQuad);
                eventChild.transform.DOMoveX(eventChild.transform.position.x - 4.3f, 2f).SetEase(Ease.OutQuad);
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/Car_Skid");
                Messenger.Default.Publish(new NarrationMessage("멈춰요!", "audio_2_삐__멈춰요_"));
                warningImg.gameObject.SetActive(true);
                warningImg.gameObject.transform.DOScale(Vector3.one, 0.5f);

            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                warningImg.gameObject.transform.DOScale(Vector3.zero, 0.5f);
                DOVirtual.DelayedCall(1f, () => warningImg.gameObject.SetActive(false));
                Messenger.Default.Publish(new NarrationMessage("사람은 보도로 다녀요", "audio_3_사람은_보도로_다녀요_"));

            })
            .AppendInterval(2f)

            .Append(
                eventChild.transform
                    .DOPath(
                        pathPoints,
                        2f, // 이동 시간
                        PathType.CatmullRom // 부드러운 커브
                    )
                    .SetOptions(
                        false, // 닫힌 경로 아님
                        AxisConstraint.None, // 위치 축 잠금 없음
                        AxisConstraint.X | AxisConstraint.Z // X축·Z축 회전만 잠가고, Y축 회전은 허용
                    )
                    .SetLookAt(
                        0.01f // 0.01초 뒤의 경로 진행 방향 바라보기
                    )
                    .SetEase(Ease.Linear) // 선형 속도
            )

            .Append(eventChild.transform
                .DORotateQuaternion(
                    Quaternion.Euler(eventChild.transform.eulerAngles.x, -90f, eventChild.transform.eulerAngles.z), 1f)
                .SetEase(Ease.Linear))

            .Append(eventChild.transform.DOMoveX(eventChild.transform.position.x - 10f, 1f)
                .SetEase(Ease.Linear))

            .AppendCallback(() =>
            {
                eventCar.transform.DOMoveX(transform.position.x + 10f, 3f).SetEase(Ease.OutQuad);
                Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/Car_Pass_By_1");
                // nextCarBtn.gameObject.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic)
                //     .OnStart(() => nextCarBtn.gameObject.SetActive(true));
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                if (_stage == EventStage.HumanExample)
                    NextStage(EventStage.CarExample);
            })
            ;
    }

    private void StartCarExampleStage()
    {

        var startPos = carStartPosTf.position;
        var midPoint1 = carMidPoint1Tf.position;
        var midPoint2 = carMidPoint2Tf.position;
        var midPoint3 = carMidPoint3Tf.position;
        var endPos = carEndPosTf.position;

        Vector3[] pathPoints = new[]
        {
            startPos,
            midPoint1,
            midPoint2,
            midPoint3,
            endPos
        };

        eventCar.transform.DOKill();
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                eventCar.transform.position = eventCarSideWalkTransform.transform.position;
                eventChild.transform.position = eventChildSideWalkTransform.transform.position;
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                eventCar.transform.DOMoveX(eventCar.transform.position.x + 3.5f, 2f).SetEase(Ease.OutQuad);
                eventChild.transform.DOMoveX(eventChild.transform.position.x - 5f, 2f).SetEase(Ease.OutQuad);
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/Car_Skid");
                Messenger.Default.Publish(new NarrationMessage("멈춰요!", "audio_2_삐__멈춰요_"));
                warningImg.gameObject.SetActive(true);
                warningImg.gameObject.transform.DOScale(Vector3.one, 0.5f);

            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                warningImg.gameObject.transform.DOScale(Vector3.zero, 0.5f);
                DOVirtual.DelayedCall(1f, () => warningImg.gameObject.SetActive(false));
                Messenger.Default.Publish(new NarrationMessage("차는 도로로 다녀요", "audio_4_차는_도로로_다녀요_"));

            })


            .Append(
                eventCar.transform
                    .DOPath(
                        pathPoints,
                        1.5f, // 이동 시간
                        PathType.CatmullRom // 부드러운 커브
                    )
                    .SetOptions(
                        false, // 닫힌 경로 아님
                        AxisConstraint.None, // 위치 축 잠금 없음
                        AxisConstraint.X | AxisConstraint.Z // X축·Z축 회전만 잠가고, Y축 회전은 허용
                    )
                    .SetLookAt(
                        0.01f // 0.01초 뒤의 경로 진행 방향 바라보기
                    )
                    .SetEase(Ease.Linear) // 선형 속도
            )
            .Append(eventCar.transform.DORotateQuaternion(Quaternion.Euler(
                eventCar.transform.eulerAngles.x,
                90f,
                eventCar.transform.eulerAngles.z), 0.3f).SetEase(Ease.Linear))

            .AppendCallback(() =>
            {
                eventCar.transform.DOMoveX(eventCar.transform.position.x + 10f, 2f).SetEase(Ease.InBack);
                Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/Car_Pass_By_1");
                eventChild.transform.DOMoveX(transform.position.x - 10f, 4f).SetEase(Ease.OutQuad);
                // nextRoadBtn.gameObject.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic)
                //     .OnStart(() => nextRoadBtn.gameObject.SetActive(true));
            })
            .AppendInterval(2f)
            .AppendCallback(() =>
            {
                if (_stage == EventStage.CarExample)
                    NextStage(EventStage.Road);
            })
            ;

    }

    private void StartRoadStage()
    {
        DOTween.Sequence()
            .Append(blackoutRoad.transform.DOScale(Vector3.zero, 2).SetEase(Ease.InQuad))
            .Join(blackoutRoad.transform.DOLocalRotate(new Vector3(0, 540, 0), 2, RotateMode.FastBeyond360))
            .AppendCallback(() =>
                Messenger.Default.Publish(new NarrationMessage("자동차가 다니는 길이 없어졌어요!", "audio_5_자동차가_가는_길이_없어졌어요_")))
            .AppendInterval(4f)
            .AppendCallback(() =>
                {
                    Messenger.Default.Publish(new NarrationMessage("차도 블럭을 터치해서 차도를 만들어주세요!",
                        "audio_7_차도_블럭을_터치해서_차도를_만들어주세요", 987654321));
                    OnGameStage(GameStage.Road);
                }
            );

    }

    private void StartSideWalkStage()
    {
        DOTween.Sequence()
            .Append(blackoutSideWalk.transform.DOScale(Vector3.zero, 2).SetEase(Ease.InQuad))
            .Join(blackoutSideWalk.transform.DOLocalRotate(new Vector3(0, 540, 0), 2, RotateMode.FastBeyond360))
            .AppendCallback(() =>
                Messenger.Default.Publish(new NarrationMessage("친구들이 다니는 길이 없어졌어요!", "audio_10_친구들이_다니는_길이_없어졌어요_")))
            .AppendInterval(4f)
            .AppendCallback(() =>
                {
                    Messenger.Default.Publish(new NarrationMessage("보도 블럭을 터치해서 보도를 만들어주세요!",
                        "audio_11_보도_블럭을_터치해서_보도를_만들어주세요", 987654321));
                    OnGameStage(GameStage.SideWalk);
                }
            );

    }



    private void EndSceneRoadStage()
    {
        //기존 사람이 지나다니는 1번상황에서 인도를 스케일 조정으로 눈에 띄게 표시
        //하단에 텍스트로 "차도" 표시 , 다음으로 버튼
        DOTween.Sequence()
            .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("마지막으로 오늘 배운 걸 다시 한번 볼까요?",
                "audio_16_마지막으로_오늘_배운_걸_다시_한번_볼까요_")))
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                roadTextBg.DOFade(1, 1);
                roadText.DOFade(1, 1);
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
                Messenger.Default.Publish(new NarrationMessage("자동차는 차도로 다녀요", "audio_18_자동차는_차도로_다녀요_")))
            .AppendCallback(() => HighlightObjects(roadRedLine))
            .AppendInterval(5f)
            // .AppendCallback(() =>
            //     nextEndSideWalkBtn.gameObject.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic)
            //         .OnStart(() => nextEndSideWalkBtn.gameObject.SetActive(true)))
            // .AppendInterval(1f)
            .AppendCallback(() =>
            {
                if (_stage == EventStage.EndRoad)
                    NextStage(EventStage.EndSideWalk);
            })
            ;

    }

    [SerializeField] private GameObject[] roadRedLine;
    [SerializeField] private GameObject[] sideWalkRedLine;

    private void HighlightObjects(GameObject[] objects)
    {
        foreach (var obj in objects)
        {
            // 각 오브젝트에 대해 Sequence를 생성
            DOTween.Sequence()
                .AppendCallback(() => obj.SetActive(true)) // 켜기
                .AppendInterval(0.5f)
                .AppendCallback(() => obj.SetActive(false)) // 끄기
                .AppendInterval(0.5f)
                .SetLoops(5, LoopType.Restart); // 5번 반복
        }
    }

    private void EndSceneSideWalkStage()
    {
        //기존 차가 지나다니는 1번상황에서 인도를 스케일 조정으로 눈에 띄게 표시
        //하단에 텍스트로 "인도" 표시
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                roadTextBg.DOFade(0, 1);
                roadText.DOFade(0, 1);
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                sidewalkTextBg.DOFade(1, 1);
                sidewalkText.DOFade(1, 1);
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
                Messenger.Default.Publish(new NarrationMessage("친구들은 보도로 다녀요", "audio_17_친구들은_보도로_다녀요_")))
            .AppendCallback(() => HighlightObjects(sideWalkRedLine))
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                sidewalkTextBg.DOFade(0, 1);
                sidewalkText.DOFade(0, 1);
            })
            .AppendInterval(1f)
            .AppendCallback(() =>
            {
                if (_stage == EventStage.EndSideWalk)
                    NextStage(EventStage.EndScene);
            })
            ;
    }

    private void EndSceneStage() //컨텐츠 종료 
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                eventCar.SetActive(false);
                eventChild.SetActive(false);

                AllMovesIternal();
                Messenger.Default.Publish(new NarrationMessage("친구들 보도로 다녀야하는 것을 기억해요!", "audio_0_차도와_보도에_대해_알아봤어요_",
                    654321));
                RestartScene(delay: 8);
            })
            ;
    }

    private void AllMovesIternal()
    {
        DOTween.Sequence()
            .AppendCallback(TriggerAllMoves)
            .AppendInterval(7f)
            .SetLoops(-1, LoopType.Restart);
    }


    private void OnGameStage(GameStage stage)
    {
        gameStageBg.DOFade(1, 0.5f);
        DOVirtual.DelayedCall(1.5f, () => canTouch = true);
        DOVirtual.DelayedCall(0.5f, () =>
        {
            switch (stage)
            {
                case GameStage.Road:
                    foreach (var v in roadImg)
                    {
                        v.gameObject.SetActive(true);
                        v.DOFade(1, 0.5f);
                    }

                    break;
                case GameStage.SideWalk:
                    foreach (var v in sideWalkImg)
                    {
                        v.gameObject.SetActive(true);
                        v.DOFade(1, 0.5f);
                    }

                    break;
            }
        });

    }

    public void OnNextCarExampleStage()
    {
        if (!canNext) return;

        if (_stage == EventStage.HumanExample)
            NextStage(EventStage.CarExample);

        ClickSound();
        canNext = false;
        nextCarBtn.gameObject.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic)
            .OnComplete(() => nextCarBtn.gameObject.SetActive(false));
    }

    public void OnNextRoadStage()
    {
        if (!canNext) return;

        if (_stage == EventStage.CarExample)
            NextStage(EventStage.Road);

        ClickSound();
        canNext = false;
        nextRoadBtn.gameObject.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic)
            .OnComplete(() => nextRoadBtn.gameObject.SetActive(false));
    }

    public void OnNextEndSidWalkStage()
    {
        if (!canNext) return;

        if (_stage == EventStage.EndRoad)
            NextStage(EventStage.EndSideWalk);

        ClickSound();
        canNext = false;

        nextEndSideWalkBtn.gameObject.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic)
            .OnComplete(() => nextEndSideWalkBtn.gameObject.SetActive(false));
    }

    public void OnNextEndSceneStage()
    {
        if (!canNext) return;

        if (_stage == EventStage.EndSideWalk)
            NextStage(EventStage.EndScene);

        ClickSound();
        canNext = false;

        nextEndSceneBtn.gameObject.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic)
            .OnComplete(() => nextEndSceneBtn.gameObject.SetActive(false));
    }


    private void TriggerAllMoves()
    {
        foreach (var m in _movers)
        {
            m.BeginMove();
        }
    }

    private void ClickSound()
    {
        char click = (char)('A' + Random.Range(0, 6));
        Managers.Sound.Play(SoundManager.Sound.Effect, $"SideWalk/Audio/Click_{click}", 1f);

    }
    
}

