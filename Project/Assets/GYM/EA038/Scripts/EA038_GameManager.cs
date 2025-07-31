using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public enum EA038_MainSeq
{
    StartSequence,
    SelectAgeStageSequence,
    CardGameStageSequence,
    ObjectGameStageSequence,
    ChangeStageSequence,
    EndSequence
}

public class EA038_GameManager : Ex_BaseGameManager
{
    private EA038_MainSeq _currentSequence;

    private enum Cameras
    {
        CM_CardGame,
        CM_ObjectGame,
    }

    private enum Objects
    {
        CorrectObjectPositions,
        SetObjectPositions,
        CardPool,
        CarPool,
        FruitPool,
        BlockPool,
        
    }
    
    private enum Cars
    {
        Ban,
        Truck,
        Taxi,
        FireTruck,
        Ambulance,
        PoliceCar,
    }
    private enum Particle
    {
        Victory1,
        Victory2
    }

    public int gamePlayAge;
    [SerializeField] private int wrongCardClickedCount = 0;
    [SerializeField] private int correctCardClickedCount = 0;
    
    private EA038_UIManager _uiManager;
    private Vector3 clickEffectPos;
    private List<int> numbers;
    
    public List<EA038_Card> ea038_Cards;
    
    private Dictionary<Collider, EA038_Card> _cardByCollider;
    
    private Vector3 correctObjtargetPos;

    private Vector3 originalCardScale;
    private Vector3 originalCarScale;
    private Vector3 originalBlockScale;

    [SerializeField] private int totalTargetClickCount = 15;
    
    private int clickNarrationCount = 0;
    
    private readonly string[] _clickNarrationClips = {
        "EA038/Audio/audio_20_하나",
        "EA038/Audio/audio_21_둘",
        "EA038/Audio/audio_22_셋",
        "EA038/Audio/audio_23_넷",
        "EA038/Audio/audio_24_다섯"
    };
    
    
    protected override void Init()
    {
        BindObject(typeof(Objects));
        Bind<ParticleSystem>(typeof(Particle));
        Bind<CinemachineVirtualCamera>(typeof(Cameras));

        base.Init();

        _uiManager = UIManagerObj.GetComponent<EA038_UIManager>();
        _currentSequence = EA038_MainSeq.StartSequence;

        psResourcePath = "EA038/Asset/Fx_Click";
        SetPool(); //클릭 이펙트 용 풀

        gamePlayAge = 5; //컨텐츠 기본 설정 나이 (3세)

        numbers = Enumerable.Range(2, 6).ToList();

        _cardByCollider = FindObjectsOfType<EA038_Card>()
            .ToDictionary(card => card.GetComponent<Collider>(), card => card);
        

        originalCardScale = new Vector3(0.04224154f, 0.004107093f, 0.03364548f);
        originalCarScale = Vector3.one * 0.0936f;

        Get<CinemachineVirtualCamera>(0).Priority = 12; //카메라들 우선순위 초기화
        for (int i = 1; i <= 1; i++)
            Get<CinemachineVirtualCamera>(i).Priority = 10;
        
        // Managers.Sound.Play(SoundManager.Sound.Bgm, "EA033/Audio/BGM");
        //
        // var stageParents = new[]
        // {
        //     GetObject((int)Objects.BellStageTreeGroup).transform,
        //     GetObject((int)Objects.BulbStageTreeGroup).transform,
        //     GetObject((int)Objects.CandyStageTreeGroup).transform,
        //     GetObject((int)Objects.StarStageTreeGroup).transform
        // };

        // GetObject((int)Objs.Intro_Triangles).gameObject.SetActive(false);
        // GetObject((int)Objs.Intro_Stars).gameObject.SetActive(false);

    }

    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();

        GameStart();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        UI_InScene_StartBtn.onGameStartBtnShut -= GameStart;
    }
    
    private void GameStart()
    {
        //if (_stage == MainSeq.OnStart)
            ChangeStage(EA038_MainSeq.StartSequence);
        //ChangeStage(EA038_MainSeq.ObjectGameStageSequence);
    }

    public void ChangeStage(EA038_MainSeq next)
    {
        _currentSequence = next;
        switch (next)
        {
            case EA038_MainSeq.StartSequence: OnStartStage(); break;
            case EA038_MainSeq.SelectAgeStageSequence: OnSelectAgeStage(); break;
            case EA038_MainSeq.CardGameStageSequence: OnCardGameStage(); break;
            case EA038_MainSeq.ObjectGameStageSequence: OnObjectGameStage(); break;
            case EA038_MainSeq.ChangeStageSequence: OnChangeStage(); break;
            case EA038_MainSeq.EndSequence: OnEndStage(); break;
            
        }
        
        Logger.Log($"{next}스테이지로 변경");
    }
    
    private int cardGamePlayCount = 0;
    private int currentObjectGameStage = 0;
    
    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (_currentSequence == EA038_MainSeq.CardGameStageSequence)
            OnRayCardStage();
        // else if (_currentSequence == EA038_MainSeq.ObjectGameStageSequence && currentObjectGameStage < 2)
        //     OnRayCarObjStage();
        // else if (_currentSequence == EA038_MainSeq.ObjectGameStageSequence && currentObjectGameStage >= 2) 
        //     OnRayFruitObjStage();
        else if (_currentSequence == EA038_MainSeq.ObjectGameStageSequence) 
            OnRayBlockObjStage();
    }

    private void OnRayCardStage()
    {
        foreach (var hit in GameManager_Hits)
        {
            var clickedObj = hit.collider.gameObject;
            
            //clickedObj.transform.DOKill();

            // clickEffectPos = hit.point;
            // //clickEffectPos.y += 0.2f;
            // PlayParticleEffect(clickEffectPos);
            // PlayClickSound();

            if (_cardByCollider.TryGetValue(hit.collider, out var card))
            {
                if (card.cardValue == gamePlayAge && card.canClicked)
                {
                    correctCardClickedCount++;
                    Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");

                    card.canClicked = false;
                    card.KillShake();

                    PlayClickCountNarration();

                    switch (gamePlayAge)
                    {
                        case 3:
                            correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                                .GetChild(0).GetChild(correctCardClickedCount - 1).transform.position;
                            break;
                        case 4:
                            correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                                .GetChild(1).GetChild(correctCardClickedCount - 1).transform.position;
                            break;
                        case 5:
                            correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                                .GetChild(2).GetChild(correctCardClickedCount - 1).transform.position;
                            break;
                    }

                    Vector3 targetScale = originalCardScale * 0.8f;

                    card.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
                    card.transform.DOScale(targetScale, 0.5f);
                    card.transform.DORotate(new Vector3(0, 38, 0), 1f);

                    if (correctCardClickedCount == gamePlayAge)
                    {
                        cardGamePlayCount++;

                        foreach (var cards in ea038_Cards)
                        {
                            cards.canClicked = false;
                        }

                        DOVirtual.DelayedCall(1f, ShowNarrationGamePlayAge);

                        //카드게임 초기화
                        DOVirtual.DelayedCall(4f, () =>
                        {
                            wrongCardClickedCount = 0;
                            correctCardClickedCount = 0;

                            foreach (Transform child in GetObject((int)Objects.CardPool).transform)
                            {
                                child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);
                            }

                            DOVirtual.DelayedCall(2f, () =>
                            {
                                if (cardGamePlayCount == 2)
                                {
                                    ChangeStage(EA038_MainSeq.ObjectGameStageSequence);
                                }
                                else
                                {
                                    SettingCardGame();

                                    for (int i = 0; i < GetObject((int)Objects.CardPool).transform.childCount; i++)
                                    {
                                        GetObject((int)Objects.CardPool).transform.GetChild(i).gameObject
                                                .transform.localPosition
                                            = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i)
                                                .gameObject
                                                .transform.localPosition;

                                    }
                                }
                            });
                        });
                    }
                }
                else if (card.cardValue != gamePlayAge && card.canClicked)
                {
                    wrongCardClickedCount++;
                    if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_25_아니야__잘생각해봐_");

                    card.canClicked = false;
                    clickedObj.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);

                }
            }
        }
    }

    private void PlayClickCountNarration()
    {
        Managers.Sound.Play(SoundManager.Sound.Narration, _clickNarrationClips[clickNarrationCount]);
        clickNarrationCount++;

        if (clickNarrationCount == gamePlayAge)
            clickNarrationCount = 0;
    }

    // private void OnRayCarObjStage()
    // {
    //     foreach (var hit in GameManager_Hits)
    //     {
    //         var clickedObj = hit.collider.gameObject;
    //         //clickedObj.transform.DOKill();
    //     
    //         // clickEffectPos = hit.point;
    //         // //clickEffectPos.y += 0.2f;
    //         // PlayParticleEffect(clickEffectPos);
    //         // PlayClickSound();
    //     
    //         var obj = clickedObj.GetComponent<EA038_Car>();
    //         if (obj == null)
    //         {
    //             Logger.Log("자동차 클래스가 null 입니다");
    //             return;
    //         }
    //
    //         if (obj.carValue == gamePlayAge && obj.canClicked)
    //         {
    //             correctCardClickedCount++;
    //             Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");
    //
    //             obj.canClicked = false;
    //             obj.KillShake();
    //
    //             PlayClickCountNarration();
    
    //             switch (gamePlayAge)
    //             {
    //                 case 3:
    //                     correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
    //                         .GetChild(0).GetChild(correctCardClickedCount - 1).transform.position;
    //                     break;
    //                 case 4:
    //                     correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
    //                         .GetChild(1).GetChild(correctCardClickedCount - 1).transform.position;
    //                     break;
    //                 case 5:
    //                     correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
    //                         .GetChild(2).GetChild(correctCardClickedCount - 1).transform.position;
    //                     break;
    //             }
    //
    //             Vector3 targetScale = originalCarScale * 0.9f;
    //
    //             obj.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
    //             obj.transform.DOScale(targetScale, 0.5f);
    //             obj.transform.DORotate(new Vector3(0, 0, 0), 1f);
    //
    //             if (correctCardClickedCount == gamePlayAge) //게임 종료 
    //             {
    //                 currentObjectGameStage++;
    //
    //                 ShowNarrationGamePlayAge();
    //
    //                 Transform carPool = GetObject((int)Objects.CarPool).transform;
    //
    //                 for (int i = 0; i < carPool.childCount; i++)
    //                 {
    //                     var typeParent = carPool.GetChild(i);
    //
    //                     for (int j = 0; j < typeParent.childCount; j++)
    //                     {
    //                         typeParent.GetChild(j).gameObject.GetComponent<EA038_Car>().canClicked = false;
    //                     }
    //                 }
    //
    //                 //자동차 게임 초기화
    //                 DOVirtual.DelayedCall(4f, () =>
    //                 {
    //                     wrongCardClickedCount = 0;
    //                     correctCardClickedCount = 0;
    //
    //                     for (int i = 0; i < GetObject((int)Objects.CarPool).transform.childCount; i++)
    //                     {
    //                         var typeParent = GetObject((int)Objects.CarPool).transform.GetChild(i);
    //                         for (int j = 0; j < typeParent.childCount; j++)
    //                         {
    //                             typeParent.GetChild(j).gameObject.transform.DOScale(Vector3.zero, 1f)
    //                                 .SetEase(Ease.OutCubic).OnComplete(() =>
    //                                     typeParent.GetChild(j).gameObject.SetActive(false));
    //                         }
    //                     }
    //
    //                     if (currentObjectGameStage == 2)
    //                     {
    //                         DOVirtual.DelayedCall(2f, () =>
    //                         {
    //                             SettingFruitGame();
    //                         });
    //                     }
    //                     else
    //                     {
    //                         DOVirtual.DelayedCall(2f, () =>
    //                         {
    //                             SettingCarObject();
    //                         });
    //                     }
    //
    //                 });
    //             }
    //         }
    //         else if (obj.carValue != gamePlayAge && obj.canClicked)
    //         {
    //             wrongCardClickedCount++;
    //             if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
    //             {
    //                 Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_25_아니야__잘생각해봐_");
    //                 _uiManager.PopAndChangeUI("아니야! 잘 생각해봐!", 2f);
    //             }
    //     
    //             obj.canClicked = false;
    //             clickedObj.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);
    //     
    //         }
    //     }
    // }
    //
    // private void OnRayFruitObjStage()
    // {
    //     foreach (var hit in GameManager_Hits)
    //     {
    //         var clickedObj = hit.collider.gameObject;
    //         //clickedObj.transform.DOKill();
    //
    //         // clickEffectPos = hit.point;
    //         // //clickEffectPos.y += 0.2f;
    //         // PlayParticleEffect(clickEffectPos);
    //         // PlayClickSound();
    //
    //         var _objEA038 = clickedObj.GetComponent<EA038_Fruit>();
    //         if (_objEA038 == null)
    //         {
    //             Logger.Log("과일 클래스가 null 입니다");
    //             return;
    //         }
    //             
    //         if (_objEA038.Value == gamePlayAge && _objEA038.canClicked)
    //         {
    //             correctCardClickedCount++;
    //             Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");
    //
    //             _objEA038.canClicked = false;
    //             _objEA038.KillShake();
    //
    //             switch (gamePlayAge)
    //             {
    //                 case 3:
    //                     correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
    //                         .GetChild(0).GetChild(correctCardClickedCount - 1).transform.position;
    //                     break;
    //                 case 4:
    //                     correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
    //                         .GetChild(1).GetChild(correctCardClickedCount - 1).transform.position;
    //                     break;
    //                 case 5:
    //                     correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
    //                         .GetChild(2).GetChild(correctCardClickedCount - 1).transform.position;
    //                     break;
    //             }
    //
    //             Vector3 targetScale = _objEA038.originalScale * 0.9f;
    //
    //             _objEA038.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
    //             _objEA038.transform.DOScale(targetScale, 0.5f);
    //             //obj.transform.DORotate(new Vector3(0, 0, 0), 1f);
    //
    //             if (correctCardClickedCount == gamePlayAge) //게임 종료 
    //             {
    //                 cardGamePlayCount++;
    //
    //                 ShowNarrationGamePlayAge();
    //
    //                 Transform fruitPool = GetObject((int)Objects.FruitPool).transform;
    //
    //                 for (int i = 0; i < fruitPool.childCount; i++)
    //                 {
    //                     var typeParent = fruitPool.GetChild(i);
    //
    //                     for (int j = 0; j < typeParent.childCount; j++)
    //                     {
    //                         typeParent.GetChild(j).gameObject.GetComponent<EA038_Fruit>().canClicked = false;
    //                     }
    //                 }
    //
    //                 //과일 게임 초기화
    //                 DOVirtual.DelayedCall(4f, () =>
    //                 {
    //                     wrongCardClickedCount = 0;
    //                     correctCardClickedCount = 0;
    //
    //                     for (int i = 0; i < GetObject((int)Objects.FruitPool).transform.childCount; i++)
    //                     {
    //                         var typeParent = GetObject((int)Objects.FruitPool).transform.GetChild(i);
    //                         for (int j = 0; j < typeParent.childCount; j++)
    //                         {
    //                             typeParent.GetChild(j).gameObject.transform.DOScale(Vector3.zero, 1f)
    //                                 .SetEase(Ease.OutCubic).OnComplete(() =>
    //                                     typeParent.GetChild(j).gameObject.SetActive(false));
    //                         }
    //                     }
    //
    //                     DOVirtual.DelayedCall(2f, () =>
    //                     {
    //                         SettingFruitGame();
    //                     });
    //                 });
    //             }
    //         }
    //         else if (_objEA038.Value != gamePlayAge && _objEA038.canClicked)
    //         {
    //             wrongCardClickedCount++;
    //             if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
    //             {
    //                 Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_25_아니야__잘생각해봐_");
    //                 _uiManager.PopAndChangeUI("아니야! 잘 생각해봐!", 2f);
    //             }
    //
    //             _objEA038.canClicked = false;
    //             clickedObj.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);
    //
    //         }
    //     }
    // }
    
    private void OnRayBlockObjStage()
    {
        foreach (var hit in GameManager_Hits)
        {
            var clickedObj = hit.collider.gameObject;
            //clickedObj.transform.DOKill();

            // clickEffectPos = hit.point;
            // //clickEffectPos.y += 0.2f;
            // PlayParticleEffect(clickEffectPos);
            // PlayClickSound();

            var _objEA038 = clickedObj.GetComponent<EA038_Block>();
            if (_objEA038 == null)
            {
                Logger.Log("블럭 클래스가 null 입니다");
                return;
            }
                
            if (_objEA038.Value == gamePlayAge && _objEA038.canClicked)
            {
                correctCardClickedCount++;
                Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");

                _objEA038.canClicked = false;
                _objEA038.KillShake();

                switch (gamePlayAge)
                {
                    case 3:
                        correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                            .GetChild(0).GetChild(correctCardClickedCount - 1).transform.position;
                        break;
                    case 4:
                        correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                            .GetChild(1).GetChild(correctCardClickedCount - 1).transform.position;
                        break;
                    case 5:
                        correctObjtargetPos = GetObject((int)Objects.CorrectObjectPositions).transform
                            .GetChild(2).GetChild(correctCardClickedCount - 1).transform.position;
                        break;
                }

                Vector3 targetScale = _objEA038.originalScale * 0.9f;

                _objEA038.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
                _objEA038.transform.DOScale(targetScale, 0.5f);
                //obj.transform.DORotate(new Vector3(0, 0, 0), 1f);

                if (correctCardClickedCount == gamePlayAge) //게임 종료 
                {
                    ShowNarrationGamePlayAge();
                    
                    Transform blockPool = GetObject((int)Objects.BlockPool).transform;

                    for (int i = 0; i < blockPool.childCount; i++)
                    {
                        var typeParent = blockPool.GetChild(i);

                        for (int j = 0; j < typeParent.childCount; j++)
                        {
                            typeParent.GetChild(j).gameObject.GetComponent<EA038_Block>().canClicked = false;
                        }
                    }

                    //과일 게임 초기화
                    DOVirtual.DelayedCall(4f, () =>
                    {
                        wrongCardClickedCount = 0;
                        correctCardClickedCount = 0;

                        for (int i = 0; i < GetObject((int)Objects.BlockPool).transform.childCount; i++)
                        {
                            var typeParent = GetObject((int)Objects.BlockPool).transform.GetChild(i);
                            for (int j = 0; j < typeParent.childCount; j++)
                            {
                                typeParent.GetChild(j).gameObject.transform.DOScale(Vector3.zero, 1f)
                                    .SetEase(Ease.OutCubic).OnComplete(() =>
                                        typeParent.GetChild(j).gameObject.SetActive(false));
                            }
                        }

                        DOVirtual.DelayedCall(2f, SettingBlockGame);
                    });
                }
            }
            else if (_objEA038.Value != gamePlayAge && _objEA038.canClicked)
            {
                wrongCardClickedCount++;
                if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_25_아니야__잘생각해봐_");
                    _uiManager.PopAndChangeUI("아니야! 잘 생각해봐!", 2f);
                }

                _objEA038.canClicked = false;
                clickedObj.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);

            }
        }
    }

    private void ShowNarrationGamePlayAge()
    {
        switch (gamePlayAge)
        {
            case 3:
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_27_다_찾았어요__세살_");
                _uiManager.PopAndChangeUI("다 찾았어요! 3살!", 3f);
                break;
            case 4:
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_28_다_찾았어요__네살_");
                _uiManager.PopAndChangeUI("다 찾았어요! 4살!", 3f);
                break;
            case 5:
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_26_다_찾았어요__다섯살_");
                _uiManager.PopAndChangeUI("다 찾았어요! 5살!", 3f);
                break;
        }
    }


    private void OnStartStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_0_형님이_된_나이만큼_숫자를_알아봐요_");
            })
            .AppendInterval(4f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_1_내_나이를_찾아볼까요_");
                _uiManager.PopAndChangeUI("내 나이를 찾아볼까요?", 4f);
            })
            .AppendInterval(3.5f)
            .AppendCallback(() =>
            {
                ChangeStage(EA038_MainSeq.SelectAgeStageSequence);
            })
            ;
        
    }
    
    private void OnSelectAgeStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_2_먼저_나이를_설정해주세요_");
                _uiManager.PopAndChangeUI("먼저 나이를 설정해주세요!");
                
                _uiManager.ShowSelectAgeBtn();
            });

    }

    private void OnCardGameStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_7_내_나이_카드를_뒤집어요_");
            })
            .AppendInterval(3f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_8_내_나이_숫자에_맞는_카드를_뒤집어주세요_");
                
                SettingCardGame();
            })
            ;
        
    }
    
    
    
    private void OnObjectGameStage()
    {
        Get<CinemachineVirtualCamera>((int)Cameras.CM_CardGame).Priority = 10;
        Get<CinemachineVirtualCamera>((int)Cameras.CM_ObjectGame).Priority = 12;

        DOVirtual.DelayedCall(2f, SettingBlockGame);
        
        //SettingCarObject();
        //SettingFruitGame();

    }
    
    private void OnChangeStage()
    {
        
    }

    private void OnEndStage()
    {
        
    }

    

    #region 카드게임 기능
    private void SettingCardGame()
    {
        _uiManager.PopAndChangeUI("내 나이 숫자에 맞는 카드를 뒤집어주세요!");
        
        int total = totalTargetClickCount;

        List<int> values = new List<int>(total);

        numbers.Remove(gamePlayAge);

        for (int i = 0; i < gamePlayAge; i++)
            values.Add(gamePlayAge);

        values.AddRange(numbers);

        int leftValueCount = gamePlayAge + numbers.Count;
        for (int i = leftValueCount; i < total; i++)
            values.Add(numbers[Random.Range(0, numbers.Count)]);

        // 셔플
        for (int i = 0; i < values.Count; i++)
        {
            int j = Random.Range(0, total);
            int tmp = values[i];
            values[i] = values[j];
            values[j] = tmp;
        }

        for (int i = 0; i < total; i++)
        {
            ea038_Cards[i].SetValue(values[i]);
            ea038_Cards[i].ChangeValueTMP(values[i]);
        }

        foreach (var card in ea038_Cards)
            card.gameObject.transform.DOScale(originalCardScale, 1f).SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    card.canClicked = true;
                    if (card.cardValue == gamePlayAge)
                        card.Shake();
                });

    }
    
    
    #endregion
    
    #region 자동차 게임 기능
    
    Dictionary<int, Cars> _CarMapping = new Dictionary<int, Cars>();
    private void SettingCarObject()
    {
        for (int i = 0; i < GetObject((int)Objects.CarPool).transform.childCount; i++)
        {
            var typeParent = GetObject((int)Objects.CarPool).transform.GetChild(i);
            for (int j = 0; j < typeParent.childCount; j++)
            {
                typeParent.GetChild(j).gameObject.SetActive(false);
            }
        }
        
        Cars[] carArray = (Cars[])Enum.GetValues(typeof(Cars));

        // 셔플
        for (int i = carArray.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);  // 0 <= j <= i
            Cars tmp       = carArray[i];
            carArray[i]    = carArray[j];
            carArray[j]    = tmp;
        }

        _CarMapping.Clear();

        for (int i = 0; i < carArray.Length; i++)
        {
            int key = i + 2;
            _CarMapping.Add(key, carArray[i]);
        }
        

        int total = totalTargetClickCount;

        List<int> values = new List<int>(total);

        numbers.Remove(gamePlayAge);

        for (int i = 0; i < gamePlayAge; i++)
            values.Add(gamePlayAge);

        values.AddRange(numbers);

        int leftValueCount = gamePlayAge + numbers.Count;
        for (int i = leftValueCount; i < total; i++)
            values.Add(numbers[Random.Range(0, numbers.Count)]);

        // 셔플
        for (int i = 0; i < values.Count; i++)
        {
            int j = Random.Range(0, total);
            int tmp = values[i];
            values[i] = values[j];
            values[j] = tmp;
        }

        Dictionary<Cars,int> reverseMap = _CarMapping
            .ToDictionary(pair => pair.Value, pair => pair.Key);

        Transform carPool = GetObject((int)Objects.CarPool).transform;

        for (int i = 0; i < carPool.childCount; i++)
        {
            var typeParent = carPool.GetChild(i);
            Cars carType = (Cars)i; 

            int key = reverseMap[carType];

            for (int j = 0; j < typeParent.childCount; j++)
            {
                var car = typeParent.GetChild(j)
                    .GetComponent<EA038_Car>();
                car.SetValue(key);
                car.ChangeValueTMP(key);
            }
        }
        
        for (int i = 0; i < total; i++)             //values[] 대로 딕셔너리에서 뽑아와서 해당 자동차 생성하는 로직
        {
            GetDeactiveChild(GetObject((int)Objects.CarPool).transform.GetChild((int)_CarMapping[values[i]]).transform).transform.position
                = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i).transform.position;

            EA038_Car car = GetDeactiveChild(GetObject((int)Objects.CarPool).transform.GetChild((int)_CarMapping[values[i]]).transform)
                .transform.gameObject.GetComponent<EA038_Car>();

            GameObject carObj = GetDeactiveChild(GetObject((int)Objects.CarPool).transform
                .GetChild((int)_CarMapping[values[i]]).transform);

            carObj.SetActive(true);
            carObj.transform.DOScale(originalCarScale, 1f).SetEase(Ease.OutBack)
                .From(Vector3.zero)
                .OnComplete(() =>
                {
                    car.canClicked = true;
                    if (car.carValue == gamePlayAge)
                        car.Shake();
                });
        }
    }

    #endregion

    #region 과일 게임 기능
    
    private List<int> codePool = Enumerable.Range(2, 6).ToList();
    public Dictionary<FruitType, int> fruitCodeMap;
    
    private void SettingFruitGame()
    {
        shuffleFruit();
    
        for (int i = 0; i < GetObject((int)Objects.FruitPool).transform.childCount; i++)
        {
            var typeParent = GetObject((int)Objects.FruitPool).transform.GetChild(i);
            for (int j = 0; j < typeParent.childCount; j++)
            {
                typeParent.GetChild(j).gameObject.SetActive(false);
            }
        }
        
        for (int i = 0; i < GetObject((int)Objects.FruitPool).transform.childCount; i++)
        for (int j = 0; j < GetObject((int)Objects.FruitPool).transform.GetChild(i).childCount; j++)
        {
            var fruit = GetObject((int)Objects.FruitPool).transform.GetChild(i).GetChild(j).GetComponent<EA038_Fruit>();
            fruit.settingFruit();
            //fruit.gameObject.transform.localScale = fruit.originalScale;
        }
    
        List<int> Pos = ShuffleSpawnOne();
    
        Dictionary<int, FruitType> reverseMap = fruitCodeMap
            .ToDictionary(pair => pair.Value, pair => pair.Key);
    
        for (int i = 0; i < 15; i++)
        {
            GetDeactiveChild(GetObject((int)Objects.FruitPool).transform.GetChild((int)reverseMap[Pos[i]]).transform).transform.position
                = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i).transform.position;
            
                
            EA038_Fruit fruit = GetDeactiveChild(GetObject((int)Objects.FruitPool).transform
                    .GetChild((int)reverseMap[Pos[i]]).transform).transform.gameObject.GetComponent<EA038_Fruit>();
            
            GameObject fruitObj = GetDeactiveChild(GetObject((int)Objects.FruitPool).transform
                .GetChild((int)reverseMap[Pos[i]]).transform);
    
            fruitObj.SetActive(true);
            fruitObj.transform.DOScale(Vector3.one * 0.6f, 1f).SetEase(Ease.OutBack)
                .From(Vector3.zero)
                .OnComplete(() =>
                {
                    fruit.canClicked = true;
                    if (fruit.Value == gamePlayAge)
                        fruit.Shake();
                });
        }
    
    }
    
    public void shuffleFruit()
    {
        for (int i = codePool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp      = codePool[i];
            codePool[i]   = codePool[j];
            codePool[j]   = tmp;
        }
        
        FruitType[] types = Enum.GetValues(typeof(FruitType))
            .Cast<FruitType>()
            .ToArray();
    
        fruitCodeMap = new Dictionary<FruitType, int>();
        for (int i = 0; i < types.Length; i++)
            fruitCodeMap[types[i]] = codePool[i];
        
    }
    
    #endregion
    
    
    private List<int> ShuffleSpawnOne()
    {
        int total = totalTargetClickCount;

        List<int> values = new List<int>(total);

        numbers.Clear();
        numbers = Enumerable.Range(2, 6).ToList();
        
        numbers.Remove(gamePlayAge);

        for (int i = 0; i < gamePlayAge; i++)
            values.Add(gamePlayAge);

        values.AddRange(numbers);

        int leftValueCount = gamePlayAge + numbers.Count;
        for (int i = leftValueCount; i < total; i++)
            values.Add(numbers[Random.Range(0, numbers.Count)]);

        // 셔플
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            
            int tmp = values[i];
            values[i] = values[j];
            values[j] = tmp;
        }
        
        return values;
    }
    
    
    
    private List<int> blockRandomList = Enumerable.Range(2, 6).ToList();
    public Dictionary<BlockType, int> blockCodeMap;

    private void SettingBlockGame()
    {
        shuffleBlock();

        for (int i = 0; i < GetObject((int)Objects.BlockPool).transform.childCount; i++)
        {
            var typeParent = GetObject((int)Objects.BlockPool).transform.GetChild(i);
            for (int j = 0; j < typeParent.childCount; j++)
            {
                typeParent.GetChild(j).gameObject.SetActive(false);
            }
        }
        
        for (int i = 0; i < GetObject((int)Objects.BlockPool).transform.childCount; i++)
        for (int j = 0; j < GetObject((int)Objects.BlockPool).transform.GetChild(i).childCount; j++)
        {
            var block = GetObject((int)Objects.BlockPool).transform.GetChild(i).GetChild(j).GetComponent<EA038_Block>();
            block.settingBlock();
            //fruit.gameObject.transform.localScale = fruit.originalScale;
        }

        List<int> Pos = ShuffleSpawnOne();

        Dictionary<int, BlockType> reverseMap = blockCodeMap
            .ToDictionary(pair => pair.Value, pair => pair.Key);

        for (int i = 0; i < 15; i++)
        {
            GetDeactiveChild(GetObject((int)Objects.BlockPool).transform.GetChild((int)reverseMap[Pos[i]]).transform).transform.position
                = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i).transform.position;
            
                
            EA038_Block block = GetDeactiveChild(GetObject((int)Objects.BlockPool).transform
                    .GetChild((int)reverseMap[Pos[i]]).transform).transform.gameObject.GetComponent<EA038_Block>();
            
            GameObject blockObj = GetDeactiveChild(GetObject((int)Objects.BlockPool).transform
                .GetChild((int)reverseMap[Pos[i]]).transform);

            blockObj.SetActive(true);
            blockObj.transform.DOScale(block.originalScale, 1f).SetEase(Ease.OutBack)
                .From(Vector3.zero)
                .OnComplete(() =>
                {
                    block.canClicked = true;
                    if (block.Value == gamePlayAge)
                        block.Shake();
                });
        }

    }
    
    public void shuffleBlock()
    {
        for (int i = blockRandomList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp      = blockRandomList[i];
            blockRandomList[i]   = blockRandomList[j];
            blockRandomList[j]   = tmp;
        }
        
        BlockType[] types = Enum.GetValues(typeof(BlockType))
            .Cast<BlockType>()
            .ToArray();

        blockCodeMap = new Dictionary<BlockType, int>();
        for (int i = 0; i < types.Length; i++)
            blockCodeMap[types[i]] = blockRandomList[i];
        
    }
    
    
    private AudioClip[] _clickClips;
    
    private void PlayClickSound() 
    {
        int idx = Random.Range(0, _clickClips.Length);
        
        if (_clickClips[idx] == null)
            Logger.Log("사운드 경로에 없음");
        
        Managers.Sound.Play(SoundManager.Sound.Effect, _clickClips[idx]);
        
    }
    
    private void PlayVictorySoundAndEffect()
    {
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA038/Audio/audio_Victory");

        Get<ParticleSystem>((int)Particle.Victory1).Play();
        Get<ParticleSystem>((int)Particle.Victory2).Play();
    }
    
    
    GameObject GetDeactiveChild(Transform parent)
    {
        foreach (Transform child in parent)
            if (!child.gameObject.activeSelf)
                return child.gameObject;
        return null;
    }

}
