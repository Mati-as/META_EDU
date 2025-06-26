using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using MyGame.Messages;
using SuperMaxim.Messaging;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SideWalk_GameManager : Ex_BaseGameManager
{
    private enum EventStage
    {
        Intro, CarExample, HumanExample, Road, SideWalk, EndSideWalk, EndRoad
    }
    private EventStage _stage = EventStage.Intro;

    private enum GameStage
    {
        SideWalk, Road
    }
    
    [SerializeField] private List<CinemachineVirtualCamera> cams = new List<CinemachineVirtualCamera>(7);

    private MoveFunction[] _movers;

    [SerializeField] private GameObject eventCar;
    [SerializeField] private GameObject eventChild;

    [SerializeField] private Image warningImg;

    [SerializeField] private Button nextCarBtn;
    [SerializeField] private Button nextRoadBtn; 
    [SerializeField] private Button nextEndSideWalkBtn;

    [SerializeField] private Transform eventChildSideWalkTransform;
    [SerializeField] private Transform eventCarSideWalkTransform;

    [SerializeField] private GameObject blackoutSideWalk;
    [SerializeField] private GameObject blackoutRoad;

    [SerializeField] private SpriteRenderer gameStageBg;
    [SerializeField] private List<SpriteRenderer> sideWalkImg;
    [SerializeField] private List<SpriteRenderer> roadImg;

    [SerializeField] private int count;
    [SerializeField] private int seatClickedCount = 1;
    
    public Image roadTextBg;
    public Image sidewalkTextBg;
    public TMP_Text roadText;
    public TMP_Text sidewalkText;

    [SerializeField] private List<ParticleSystem> victoryParticles;
    private readonly Color _c = new Color(1, 1, 1, 0);

    [SerializeField] private bool canTouch;
    
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
        
        psResourcePath = "SideWalk/Asset/Fx_Click";
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

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync() || !isStartButtonClicked) return;
        
        foreach (var hit in GameManager_Hits)
        {
            if (hit.collider.CompareTag("toWork") && canTouch)
            {
                PlayParticleEffect(hit.point);
                
                count++;
                hit.collider.gameObject.SetActive(false);
                
                Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/Bell_" + seatClickedCount);
                seatClickedCount++;
                
                if (count == 6)
                {
                    canTouch = false;
                    seatClickedCount = 1;
                    DOTween.Sequence()
                        .Append(gameStageBg.DOFade(0,0.5f))
                        .AppendInterval(0.5f)
                        .Append(blackoutRoad.transform.DOScale(Vector3.one, 2).SetEase(Ease.InQuad))
                        .Join(blackoutRoad.transform.DOLocalRotate(new Vector3(0, 0, 0), 2, RotateMode.FastBeyond360))
                        .AppendCallback(() =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/audio_Victory");
                            foreach (var particle in victoryParticles)
                            {
                                particle.Play();
                            }
                        })
                        .AppendInterval(1f)
                        .AppendCallback(TriggerAllMoves)
                        .AppendInterval(1f)
                        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("차는 도로로 다녀요!", "audio_9_차는_도로로_다녀요_")))
                        .AppendInterval(5f)
                        .AppendCallback(() => NextStage(EventStage.SideWalk));
                }
                else if (count == 12)
                {
                    canTouch = false;
                    DOTween.Sequence()
                        .Append(gameStageBg.DOFade(0,0.5f))
                        .AppendInterval(0.5f)
                        .Append(blackoutSideWalk.transform.DOScale(Vector3.one, 2).SetEase(Ease.InQuad))
                        .Join(blackoutSideWalk.transform.DOLocalRotate(new Vector3(0, 0, 0), 2, RotateMode.FastBeyond360))
                        .AppendCallback(() =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Effect, "SideWalk/Audio/audio_Victory");
                            foreach (var particle in victoryParticles)
                            {
                                particle.Play();
                            }
                        })
                        .AppendInterval(1f)
                        .AppendCallback(TriggerAllMoves)
                        .AppendInterval(1f)
                        .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("친구들은 인도로 다녀요!", "audio_12_친구들은_인도로_다녀요_")))
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
        //if (_stage == EventStage.Intro)
            NextStage(EventStage.Intro);
        //NextStage(EventStage.Road);

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
        }

        Logger.Log($"{next}스테이지로 변경");
    }

   

    private void StartIntroStage()
    {
        DOTween.Sequence()
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
                Messenger.Default.Publish(new NarrationMessage("사람들이 다니는 길과\n차가 다니는길을 알아볼까요?", "audio_1_사람들이_다니는_길과_차가_다니는길을_알아볼까요_"));
            })
            .AppendInterval(6f)
            .AppendCallback(() => NextStage(EventStage.HumanExample));

    }

    private void StartHumanExampleStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                eventChild.SetActive(true);
                eventCar.SetActive(true);
                eventCar.transform.DOMoveX(eventCar.transform.position.x + 5f, 2f).SetEase(Ease.OutQuad);
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
                Messenger.Default.Publish(new NarrationMessage("사람은 인도로 다녀요", "audio_3_사람은_인도로_다녀요_"));

            })
            .AppendInterval(2f)
            .Append(eventChild.transform.DORotateQuaternion(Quaternion.Euler(eventChild.transform.eulerAngles.x, 0f, eventChild.transform.eulerAngles.z), 1f)
            .SetEase(Ease.Linear))
            .Append(eventChild.transform.DOMoveZ(eventChild.transform.position.z + 3f, 1f).SetEase(Ease.Linear))
            .Append(eventChild.transform.DORotateQuaternion(Quaternion.Euler(eventChild.transform.eulerAngles.x, -90f, eventChild.transform.eulerAngles.z), 1f)
            .SetEase(Ease.Linear))
            .Append(eventChild.transform.DOMoveX(eventChild.transform.position.x - 10f, 1f)
            .SetEase(Ease.Linear))
            .AppendCallback(() =>
            {
                eventCar.transform.DOMoveX(transform.position.x + 10f, 3f).SetEase(Ease.OutQuad);
                nextCarBtn.gameObject.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic).OnStart(() => nextCarBtn.gameObject.SetActive(true));
            })
            ;
    }

    private void StartCarExampleStage()
    {
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
                eventCar.transform.DOMoveX(eventCar.transform.position.x + 5f, 2f).SetEase(Ease.OutQuad);
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
            .Append(eventCar.transform.DORotateQuaternion(Quaternion.Euler(
                eventCar.transform.eulerAngles.x,
                140f,
                eventCar.transform.eulerAngles.z
            ), 1f).SetEase(Ease.Linear))
            .Append(eventCar.transform.DOMove(new Vector3(-269.85f, 35, 109.67f), 2f)
            .SetEase(Ease.Linear))
            .Append(eventCar.transform.DORotateQuaternion(Quaternion.Euler(
                eventCar.transform.eulerAngles.x,
                90f,
                eventCar.transform.eulerAngles.z), 1f).SetEase(Ease.Linear))
            .Append(eventCar.transform.DOMoveX(eventCar.transform.position.x + 10f, 1f).SetEase(Ease.Linear))
            .AppendCallback(() =>
            {
                eventChild.transform.DOMoveX(transform.position.x - 10f, 3f).SetEase(Ease.OutQuad);
                nextRoadBtn.gameObject.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic).OnStart(() => nextRoadBtn.gameObject.SetActive(true));
            })
            ;

    }

    private void StartRoadStage()
    {
        DOTween.Sequence()
        .Append(blackoutRoad.transform.DOScale(Vector3.zero, 2).SetEase(Ease.InQuad))
             .Join(blackoutRoad.transform.DOLocalRotate(new Vector3(0, 540, 0), 2, RotateMode.FastBeyond360))
             .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("자동차가 다니는 길이 없어졌어요!", "audio_5_자동차가_가는_길이_없어졌어요_")))
             .AppendInterval(4f)
             //2d조각 생성되면서
             .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("차도 블럭을 터치해서 차도를 만들어주세요!", "audio_7_차도_블럭을_터치해서_차도를_만들어주세요")))
             
             .AppendInterval(4f)
             .AppendCallback(() => OnGameStage(GameStage.Road));
        
    }

    private void StartSideWalkStage()
    {
        DOTween.Sequence()
             .Append(blackoutSideWalk.transform.DOScale(Vector3.zero, 2).SetEase(Ease.InQuad))
             .Join(blackoutSideWalk.transform.DOLocalRotate(new Vector3(0, 540, 0), 2, RotateMode.FastBeyond360))
             .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("친구들이 다니는 길이 없어졌어요!", "audio_10_친구들이_다니는_길이_없어졌어요_")))
             .AppendInterval(4f)
             .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("인도 블럭을 터치해서\n인도를 만들어주세요!", "audio_11_인도_블럭을_터치해서_인도를_만들어주세요")))
             .AppendInterval(4f)
             .AppendCallback(() => OnGameStage(GameStage.SideWalk));
            
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
            .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("자동차는 차도로 다녀요", "audio_18_자동차는_차도로_다녀요_")))
            .Append(blackoutRoad.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.Linear)
                .SetLoops(10, LoopType.Yoyo))
            .Join(roadTextBg.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.Linear)
                .SetLoops(10, LoopType.Yoyo))
            .AppendInterval(1f)
            .AppendCallback(() => nextEndSideWalkBtn.gameObject.transform.DOScale(1, 1f).From(0).SetEase(Ease.OutElastic).OnStart(() => nextEndSideWalkBtn.gameObject.SetActive(true)))
            
            ;

    }

    
    private void EndSceneSideWalkStage()        //컨텐츠 종료 
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
            .AppendCallback(() => Messenger.Default.Publish(new NarrationMessage("친구들은 인도로 다녀요", "audio_17_친구들은_인도로_다녀요_")))
            .Append(blackoutSideWalk.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.Linear)
                .SetLoops(10, LoopType.Yoyo))
            .Join(sidewalkTextBg.transform.DOScale(Vector3.one * 1.1f, 0.5f).SetEase(Ease.Linear)
                .SetLoops(10, LoopType.Yoyo))
            .AppendCallback(() =>
            {
                sidewalkTextBg.DOFade(0, 1);
                sidewalkText.DOFade(0, 1);
            })
            .AppendInterval(1f)
            ;
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
        if (_stage == EventStage.HumanExample)
            NextStage(EventStage.CarExample);

        ClickSound();

        nextCarBtn.gameObject.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic)
            .OnComplete(() => nextCarBtn.gameObject.SetActive(false));
    }

    public void OnNextRoadStage()
    {
        if (_stage == EventStage.CarExample)
            NextStage(EventStage.Road);
        
        ClickSound();

        nextRoadBtn.gameObject.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic)
            .OnComplete(() => nextRoadBtn.gameObject.SetActive(false));
    }

    public void OnNextEndSidWalkStage()
    {
        if (_stage == EventStage.EndRoad)
            NextStage(EventStage.EndSideWalk);

        ClickSound();
        
        nextEndSideWalkBtn.gameObject.transform.DOScale(0, 1f).From(1).SetEase(Ease.OutElastic)
            .OnComplete(() => nextEndSideWalkBtn.gameObject.SetActive(false));
        
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
