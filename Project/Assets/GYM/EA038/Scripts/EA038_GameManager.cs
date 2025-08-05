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
        ShowCorrectObjectPositions,
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

    [SerializeField] private int totalTargetClickCount = 15;

    private int clickNarrationCount = 0;

    private readonly string[] _clickNarrationClips =
    {
        "EA038/Audio/audio_20_하나",
        "EA038/Audio/audio_21_둘",
        "EA038/Audio/audio_22_셋",
        "EA038/Audio/audio_23_넷",
        "EA038/Audio/audio_24_다섯"
    };

    private Vector3 randomEuler;

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


        originalCardScale = new Vector3(0.003750216f, 0.0006125633f, 0.003357493f);

        Get<CinemachineVirtualCamera>(0).Priority = 12; //카메라들 우선순위 초기화
        for (int i = 1; i <= 1; i++)
            Get<CinemachineVirtualCamera>(i).Priority = 10;
        
        Managers.Sound.Play(SoundManager.Sound.Bgm, "EA038/Audio/BGM");
        
        _clickClips = new AudioClip[5]; //오디오 char 캐싱 
        for (int i = 0; i < _clickClips.Length; i++)
            _clickClips[i] = Resources.Load<AudioClip>($"EA038/Audio/Click_{(char)('A' + i)}");
        
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
            case EA038_MainSeq.EndSequence: OnEndStage(); break;

        }

        Logger.Log($"{next}스테이지로 변경");
    }

    private int cardGamePlayCount = 0;
    private bool canPlayGame = false;

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (_currentSequence == EA038_MainSeq.CardGameStageSequence && canPlayGame)
            OnRayCardStage();
        else if (_currentSequence == EA038_MainSeq.ObjectGameStageSequence && canPlayGame)
        {
            OnRayBlockObjStage();
            OnRayFruitObjStage();
            OnRayCarObjStage();
        }
    }

    private List<GameObject> correctObjectList = new List<GameObject>(5);

    private void OnRayCardStage()
    {
        foreach (var hit in GameManager_Hits)
        {
            var clickedObj = hit.collider.gameObject;

            if (_cardByCollider.TryGetValue(hit.collider, out var card))
            {
                if (card.cardValue == gamePlayAge && card.canClicked)
                {
                    clickEffectPos = hit.point;
                    clickEffectPos.y += 0.2f;
                    PlayParticleEffect(clickEffectPos);
                    PlayClickSound();

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


                    correctObjectList.Add(card.gameObject);
                    card.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
                    card.transform.DOScale(targetScale, 0.5f);
                    card.transform.DORotate(new Vector3(0, -51.3f, 0), 1f);

                    if (correctCardClickedCount == gamePlayAge)
                    {
                        cardGamePlayCount++;
                        canPlayGame = false;

                        foreach (var cards in ea038_Cards)
                        {
                            cards.canClicked = false;
                        }

                        PlayVictorySoundAndEffect();

                        float delaySeq = gamePlayAge;
                        
                        DOTween.Sequence()
                            .AppendInterval(1f)
                            .AppendCallback(ShowNarrationGamePlayAge)
                            .AppendInterval(3f)
                            .AppendCallback(() =>
                            {
                                wrongCardClickedCount = 0;
                                correctCardClickedCount = 0;

                                foreach (Transform child in GetObject((int)Objects.CardPool).transform)
                                {
                                    child.gameObject.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);
                                }
                            })
                            .AppendInterval(2f)
                            .AppendCallback(() =>
                            {
                                for (int i = 0; i < correctObjectList.Count; i++)
                                {
                                    int idx = i; // 클로저 이슈 방지
                                    float delay = idx;

                                    DOVirtual.DelayedCall(delay, () =>
                                    {
                                        var obj = correctObjectList[idx];
                                        // 위치 세팅
                                        obj.transform.position =
                                            GetObject((int)Objects.ShowCorrectObjectPositions)
                                                .transform.GetChild(gamePlayAge - 3)
                                                .GetChild(idx).position;
                                        // 활성화 & 스케일 트윈
                                        obj.SetActive(true);
                                        obj.transform.DOScale(originalCardScale * 2f, 1f);
                                    });
                                }
                            })
                            .AppendInterval(delaySeq)
                            .AppendCallback(() =>
                            {
                                for (int i = 0; i < correctObjectList.Count; i++)
                                {
                                    correctObjectList[i].transform.DOScale(0, 1f);
                                }
                            })
                            .AppendInterval(1f)
                            .AppendCallback(() =>
                            {
                                if (cardGamePlayCount == 2)
                                {
                                    ChangeStage(EA038_MainSeq.ObjectGameStageSequence);
                                }
                                else
                                {
                                    DOTween.Sequence()
                                        .AppendCallback(() => Managers.Sound.Play(SoundManager.Sound.Narration,
                                            "EA038/Audio/audio_9_친구들_다시_한_번_해볼까요_"))
                                        .AppendInterval(3f)
                                        .AppendCallback(() =>
                                        {
                                            for (int i = 0;
                                                 i < GetObject((int)Objects.CardPool).transform.childCount;
                                                 i++)
                                            {
                                                GetObject((int)Objects.CardPool).transform.GetChild(i).gameObject
                                                        .transform.localPosition
                                                    = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i)
                                                        .gameObject
                                                        .transform.localPosition;

                                            }

                                            _uiManager.PopInstructionUIFromScaleZero("내 나이 숫자에 맞는 카드를 뒤집어주세요", 12345f,
                                                narrationPath: "EA038/Audio/audio_8_내_나이_숫자에_맞는_카드를_뒤집어주세요_");

                                            SettingCardGame();

                                        })
                                        .AppendInterval(4f)
                                        .AppendCallback(() =>
                                        {
                                            _uiManager.PlayReadyAndStart();
                                        })
                                        .AppendInterval(5f)
                                        .AppendCallback(() =>
                                        {
                                            canPlayGame = true;
                                        })
                                        ;
                                }
                            })
                            ;
                    }
                }
                else if (card.cardValue != gamePlayAge && card.canClicked)
                {
                    PlayClickSound();

                    wrongCardClickedCount++;
                    if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_25_아니야__잘생각해봐_");
                        //_uiManager.PopInstructionUIFromScaleZero("아니야! 잘 생각해봐!", 3f);
                    }

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

    private int objGameStageCount = 0;
    
    private void OnRayCarObjStage()
    {
        foreach (var hit in GameManager_Hits)
        {
            var clickedObj = hit.collider.gameObject;
            
            var obj = clickedObj.GetComponent<EA038_Car>();
            if (obj == null)
            {
                Logger.Log("자동차 클래스가 null 입니다");
                return;
            }

            if (obj.carValue == gamePlayAge && obj.canClicked)
            {
                clickEffectPos = hit.point;
                clickEffectPos.y += 0.2f;
                PlayParticleEffect(clickEffectPos);
                PlayClickSound();

                correctCardClickedCount++;
                Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");

                obj.canClicked = false;
                obj.KillShake();

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

                Vector3 targetScale = obj.originalScale * 0.9f;

                Vector3 originalCarScale = obj.originalScale;
                correctObjectList.Add(obj.gameObject);
                obj.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
                obj.transform.DOScale(targetScale, 0.5f);
                obj.transform.DORotate(new Vector3(0, -15.675f, 0), 1f);

                if (correctCardClickedCount == gamePlayAge) //게임 종료 
                {
                    PlayVictorySoundAndEffect();
                    canPlayGame = false;

                    DOVirtual.DelayedCall(1f, ShowNarrationGamePlayAge);

                    objGameStageCount++;

                    Transform carPool = GetObject((int)Objects.CarPool).transform;

                    for (int i = 0; i < carPool.childCount; i++)
                    {
                        var typeParent = carPool.GetChild(i);

                        for (int j = 0; j < typeParent.childCount; j++)
                        {
                            typeParent.GetChild(j).gameObject.GetComponent<EA038_Car>().canClicked = false;
                        }
                    }

                    //자동차 게임 초기화
                    DOVirtual.DelayedCall(4f, () =>
                    {
                        wrongCardClickedCount = 0;
                        correctCardClickedCount = 0;

                        for (int i = 0; i < GetObject((int)Objects.CarPool).transform.childCount; i++)
                        {
                            var typeParent = GetObject((int)Objects.CarPool).transform.GetChild(i);
                            for (int j = 0; j < typeParent.childCount; j++)
                            {
                                Transform child = typeParent.GetChild(j);

                                child.DOScale(Vector3.zero, 1f)
                                    .SetEase(Ease.OutCubic)
                                    .OnComplete(() =>
                                    {
                                        child.gameObject.SetActive(false);
                                    });
                            }
                        }

                        DOVirtual.DelayedCall(1f, () =>
                        {
                            for (int i = 0; i < correctObjectList.Count; i++)
                            {
                                int idx = i; // 클로저 이슈 방지
                                float delay = idx;

                                DOVirtual.DelayedCall(delay, () =>
                                {
                                    var obj = correctObjectList[idx];
                                    // 위치 세팅
                                    obj.transform.position =
                                        GetObject((int)Objects.ShowCorrectObjectPositions)
                                            .transform.GetChild(gamePlayAge - 3)
                                            .GetChild(idx).position;
                                    // 활성화 & 스케일 트윈
                                    obj.SetActive(true);
                                    obj.transform.DOScale(originalCarScale * 2f, 1f);
                                });
                            }
                        });

                        DOVirtual.DelayedCall(gamePlayAge + 1f, () =>
                        {
                            for (int i = 0; i < correctObjectList.Count; i++)
                            {
                                correctObjectList[i].transform.DOScale(0, 1f);
                            }

                            DOVirtual.DelayedCall(1, () =>
                            {
                                if (objGameStageCount == 2)
                                {
                                    DOVirtual.DelayedCall(2f, StartSecondObjectGame);
                                }
                                else if (objGameStageCount == 4)
                                {
                                    ChangeStage(EA038_MainSeq.EndSequence);
                                }
                                else
                                {
                                    DOTween.Sequence()
                                        .AppendCallback(() => Managers.Sound.Play(SoundManager.Sound.Narration,
                                            "EA038/Audio/audio_9_친구들_다시_한_번_해볼까요_"))
                                        .AppendInterval(3f)
                                        .AppendCallback(() =>
                                        {
                                            _uiManager.PopInstructionUIFromScaleZero("내 나이 숫자가 써진 자동차를 터치해주세요!", 3f,
                                                narrationPath: "EA038/Audio/audio_13_내_나이_숫자가_써진_자동차를_터치해주세요_");
                                            SettingCarObject();
                                        })
                                        .AppendInterval(4f)
                                        .AppendCallback(() =>
                                        {
                                            _uiManager.PlayReadyAndStart();
                                        })
                                        .AppendInterval(5f)
                                        .AppendCallback(() => canPlayGame = true)
                                        ;
                                }
                            });

                        });

                    });
                }
            }
            else if (obj.carValue != gamePlayAge && obj.canClicked)
            {
                PlayClickSound();
                
                wrongCardClickedCount++;
                if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_25_아니야__잘생각해봐_");
                    //_uiManager.PopInstructionUIFromScaleZero("아니야! 잘 생각해봐!", 3f);
                }

                obj.canClicked = false;
                clickedObj.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);

            }
        }
    }

    private void OnRayFruitObjStage()
    {
        foreach (var hit in GameManager_Hits)
        {
            var clickedObj = hit.collider.gameObject;
            
            var _objEA038 = clickedObj.GetComponent<EA038_Fruit>();
            if (_objEA038 == null)
            {
                Logger.Log("과일 클래스가 null 입니다");
                return;
            }

            if (_objEA038.Value == gamePlayAge && _objEA038.canClicked)
            {
                clickEffectPos = hit.point;
                clickEffectPos.y += 0.2f;
                PlayParticleEffect(clickEffectPos);
                PlayClickSound();

                correctCardClickedCount++;
                Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");

                _objEA038.canClicked = false;
                _objEA038.KillShake();

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

                Vector3 targetScale = _objEA038.originalScale * 0.9f;

                Vector3 originalFruitScale = _objEA038.originalScale;
                correctObjectList.Add(_objEA038.gameObject);

                _objEA038.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
                _objEA038.transform.DOScale(targetScale, 0.5f);
                //obj.transform.DORotate(new Vector3(0, 0, 0), 1f);

                if (correctCardClickedCount == gamePlayAge) //게임 종료 
                {
                    PlayVictorySoundAndEffect();
                    canPlayGame = false;

                    DOVirtual.DelayedCall(1f, ShowNarrationGamePlayAge);

                    objGameStageCount++;

                    Transform fruitPool = GetObject((int)Objects.FruitPool).transform;

                    for (int i = 0; i < fruitPool.childCount; i++)
                    {
                        var typeParent = fruitPool.GetChild(i);

                        for (int j = 0; j < typeParent.childCount; j++)
                        {
                            typeParent.GetChild(j).gameObject.GetComponent<EA038_Fruit>().canClicked = false;
                        }
                    }

                    //과일 게임 초기화
                    DOVirtual.DelayedCall(4f, () =>
                    {
                        wrongCardClickedCount = 0;
                        correctCardClickedCount = 0;

                        for (int i = 0; i < GetObject((int)Objects.FruitPool).transform.childCount; i++)
                        {
                            var typeParent = GetObject((int)Objects.FruitPool).transform.GetChild(i);
                            for (int j = 0; j < typeParent.childCount; j++)
                            {
                                typeParent.GetChild(j).gameObject.transform.DOScale(Vector3.zero, 1f)
                                    .SetEase(Ease.OutCubic).OnComplete(() =>
                                        typeParent.GetChild(j).gameObject.SetActive(false));
                            }
                        }


                        DOVirtual.DelayedCall(1f, () =>
                        {
                            for (int i = 0; i < correctObjectList.Count; i++)
                            {
                                int idx = i; // 클로저 이슈 방지
                                float delay = idx;

                                DOVirtual.DelayedCall(delay, () =>
                                {
                                    var obj = correctObjectList[idx];
                                    // 위치 세팅
                                    obj.transform.position =
                                        GetObject((int)Objects.ShowCorrectObjectPositions)
                                            .transform.GetChild(gamePlayAge - 3)
                                            .GetChild(idx).position;
                                    // 활성화 & 스케일 트윈
                                    obj.SetActive(true);
                                    obj.transform.DOScale(originalFruitScale * 2f, 1f);
                                });
                            }
                        });

                        DOVirtual.DelayedCall(gamePlayAge + 1f, () =>
                        {
                            for (int i = 0; i < correctObjectList.Count; i++)
                            {
                                correctObjectList[i].transform.DOScale(0, 1f);
                            }

                            if (objGameStageCount == 2)
                            {
                                DOVirtual.DelayedCall(2f, StartSecondObjectGame);
                            }
                            else if (objGameStageCount == 4)
                            {
                                ChangeStage(EA038_MainSeq.EndSequence);
                            }
                            else
                            {
                                DOTween.Sequence()
                                    .AppendCallback(() => Managers.Sound.Play(SoundManager.Sound.Narration,
                                        "EA038/Audio/audio_9_친구들_다시_한_번_해볼까요_"))
                                    .AppendInterval(3f)
                                    .AppendCallback(() =>
                                    {
                                        _uiManager.PopInstructionUIFromScaleZero("내 나이 숫자가 써진 과일을 터치해주세요!", 3f,
                                            narrationPath: "EA038/Audio/audio_12_내_나이_숫자가_써진_과일을_터치해주세요_");
                                        SettingFruitGame();
                                    })
                                    .AppendInterval(4f)
                                    .AppendCallback(() =>
                                    {
                                        _uiManager.PlayReadyAndStart();
                                    })
                                    .AppendInterval(5f)
                                    .AppendCallback(() => canPlayGame = true)
                                    ;
                            }
                        });


                    });
                }
            }
            else if (_objEA038.Value != gamePlayAge && _objEA038.canClicked)
            {
                PlayClickSound();
                
                wrongCardClickedCount++;
                if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_25_아니야__잘생각해봐_");
                    //_uiManager.PopInstructionUIFromScaleZero("아니야! 잘 생각해봐!", 3f);
                }

                _objEA038.canClicked = false;
                clickedObj.transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutCubic);

            }
        }
    }

    private void OnRayBlockObjStage()
    {
        foreach (var hit in GameManager_Hits)
        {
            var clickedObj = hit.collider.gameObject;
            
            var _objEA038 = clickedObj.GetComponent<EA038_Block>();
            if (_objEA038 == null)
            {
                Logger.Log("블럭 클래스가 null 입니다");
                return;
            }

            if (_objEA038.Value == gamePlayAge && _objEA038.canClicked)
            {
                clickEffectPos = hit.point;
                clickEffectPos.y += 0.2f;
                PlayParticleEffect(clickEffectPos);
                PlayClickSound();
                
                correctCardClickedCount++;
                Logger.Log($"정답 클릭 됨 : ${correctCardClickedCount}개");

                _objEA038.canClicked = false;
                _objEA038.KillShake();
                
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

                Vector3 targetScale = _objEA038.originalScale * 0.9f;

                Vector3 originalBlockScale = _objEA038.originalScale;
                correctObjectList.Add(_objEA038.gameObject);
                
                _objEA038.transform.DOJump(correctObjtargetPos, 0.5f, 1, 1f);
                _objEA038.transform.DOScale(targetScale, 0.5f);
                _objEA038.transform.DORotate(new Vector3(0, 38.623f, 0), 1f);

                if (correctCardClickedCount == gamePlayAge) //게임 종료 
                {
                    PlayVictorySoundAndEffect();
                    canPlayGame = false;
                    
                    DOVirtual.DelayedCall(1f, ShowNarrationGamePlayAge);

                    objGameStageCount++;

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
                        
                        DOVirtual.DelayedCall(1f, () =>
                        {
                            for (int i = 0; i < correctObjectList.Count; i++)
                            {
                                int idx = i; // 클로저 이슈 방지
                                float delay = idx;

                                DOVirtual.DelayedCall(delay, () =>
                                {
                                    var obj = correctObjectList[idx];
                                    // 위치 세팅
                                    obj.transform.position =
                                        GetObject((int)Objects.ShowCorrectObjectPositions)
                                            .transform.GetChild(gamePlayAge - 3)
                                            .GetChild(idx).position;
                                    // 활성화 & 스케일 트윈
                                    obj.SetActive(true);
                                    obj.transform.DOScale(originalBlockScale * 2f, 1f);
                                });
                            }
                        });
                        

                        DOVirtual.DelayedCall(gamePlayAge + 1f, () =>
                        {
                            for (int i = 0; i < correctObjectList.Count; i++)
                            {
                                correctObjectList[i].transform.DOScale(0, 1f);
                            }

                            if (objGameStageCount == 2)
                            {
                                DOVirtual.DelayedCall(2f, StartSecondObjectGame);
                            }
                            else if (objGameStageCount == 4)
                            {
                                ChangeStage(EA038_MainSeq.EndSequence);
                            }
                            else
                            {
                                DOTween.Sequence()
                                    .AppendCallback(() => Managers.Sound.Play(SoundManager.Sound.Narration,
                                        "EA038/Audio/audio_9_친구들_다시_한_번_해볼까요_"))
                                    .AppendInterval(3f)
                                    .AppendCallback(() =>
                                    {
                                        _uiManager.PopInstructionUIFromScaleZero("내 나이 숫자가 써진 블럭을 터치해주세요!", 3f,
                                            narrationPath: "EA038/Audio/audio_14_내_나이_숫자가_써진_블럭을_터치해주세요_");
                                        SettingBlockGame();
                                    })
                                    .AppendInterval(4f)
                                    .AppendCallback(() =>
                                    {
                                        _uiManager.PlayReadyAndStart();
                                    })
                                    .AppendInterval(5f)
                                    .AppendCallback(() => canPlayGame = true)
                                    ;
                            }

                        });

                    });
                }
            }
            else if (_objEA038.Value != gamePlayAge && _objEA038.canClicked)
            {
                PlayClickSound();
                
                wrongCardClickedCount++;
                if (wrongCardClickedCount % 2 == 1) //2회에 한번 오답 안내 나레이션
                {
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_25_아니야__잘생각해봐_");
                    //_uiManager.PopInstructionUIFromScaleZero("아니야! 잘 생각해봐!", 3f);
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
                _uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 3살!", 3f);
                break;
            case 4:
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_28_다_찾았어요__네살_");
                _uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 4살!", 3f);
                break;
            case 5:
                _uiManager.PopInstructionUIFromScaleZero("다 찾았어요! 5살!", 3f, narrationPath: "EA038/Audio/audio_26_다_찾았어요__다섯살_");
                break;
        }
    }


    private void OnStartStage()
    {
        DecidePlayObjGame();
        
        DOTween.Sequence()
            // .AppendCallback(() =>
            // {
            //     Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_0_형님이_된_나이만큼_숫자를_알아봐요_"); //이건 씬 시작했을때 인트로 
            // })
            // .AppendInterval(4f)
            .AppendCallback(() =>
            {
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_1_내_나이를_찾아볼까요_");
                _uiManager.PopInstructionUIFromScaleZero("내 나이를 찾아볼까요?", 4f);
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
                _uiManager.PopInstructionUIFromScaleZero("먼저 나이를 설정해주세요!", 12345);

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
                _uiManager.PopInstructionUIFromScaleZero("내 나이 숫자에 맞는 카드를 뒤집어주세요", 12345f);

                SettingCardGame();
            })
            .AppendInterval(4.5f)
            .AppendCallback(() =>
            {
                _uiManager.PlayReadyAndStart();
            })
            .AppendInterval(5f)
            .AppendCallback(() => canPlayGame = true)
            ;

    }



    private void OnObjectGameStage()
    {
        Get<CinemachineVirtualCamera>((int)Cameras.CM_CardGame).Priority = 10;
        Get<CinemachineVirtualCamera>((int)Cameras.CM_ObjectGame).Priority = 12;

        DOVirtual.DelayedCall(0.5f, StartFirstObjectGame); 

    }


    private void OnEndStage()
    {
        DOTween.Sequence()
            .AppendCallback(() =>
            {
                _uiManager.ShowEndSelectAgeBtn(); //버튼 화면 송출
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_15_내_나이를_알아봤어요_");
            })
            .AppendInterval(2.5f)
            .AppendCallback(() =>
            {
                _uiManager.PopInstructionUIFromScaleZero("친구들 나이를 잘 기억해요~!", 12345f,
                    narrationPath: "EA038/Audio/audio_16_친구들_나이를_잘_기억해요_");

                RestartScene();
            })
            ;
    }

    
    #region 카드게임 기능

    private void SettingCardGame()
    {
        correctObjectList.Clear();
        
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

    private Dictionary<int, Cars> _CarMapping = new Dictionary<int, Cars>();

    private void SettingCarObject()
    {
        correctObjectList.Clear();
        _uiManager.PopInstructionUIFromScaleZero($"내 나이 {gamePlayAge}살이 써진 자동차를 터치해주세요!", 12345f);
        
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
            int j = Random.Range(0, i + 1); // 0 <= j <= i
            Cars tmp = carArray[i];
            carArray[i] = carArray[j];
            carArray[j] = tmp;
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

        Dictionary<Cars, int> reverseMap = _CarMapping
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

        for (int i = 0; i < total; i++) //values[] 대로 딕셔너리에서 뽑아와서 해당 자동차 생성하는 로직
        {
            GetDeactiveChild(GetObject((int)Objects.CarPool).transform.GetChild((int)_CarMapping[values[i]]).transform)
                    .transform.position
                = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i).transform.position;

            EA038_Car car =
                GetDeactiveChild(GetObject((int)Objects.CarPool).transform.GetChild((int)_CarMapping[values[i]])
                        .transform)
                    .transform.gameObject.GetComponent<EA038_Car>();

            GameObject carObj = GetDeactiveChild(GetObject((int)Objects.CarPool).transform
                .GetChild((int)_CarMapping[values[i]]).transform);

            float randomValue = Random.Range(-45f, 45f);
            randomEuler = new Vector3(0f, randomValue, 0f);
            
            carObj.SetActive(true);
            carObj.transform.localEulerAngles  = randomEuler;
            carObj.transform.DOScale(car.originalScale, 1f).SetEase(Ease.OutBack)
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
        correctObjectList.Clear();
        _uiManager.PopInstructionUIFromScaleZero($"내 나이 {gamePlayAge}살이 써진 과일을 터치해주세요!", 12345f);
        
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
            GetDeactiveChild(GetObject((int)Objects.FruitPool).transform.GetChild((int)reverseMap[Pos[i]]).transform)
                    .transform.position
                = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i).transform.position;


            EA038_Fruit fruit = GetDeactiveChild(GetObject((int)Objects.FruitPool).transform
                .GetChild((int)reverseMap[Pos[i]]).transform).transform.gameObject.GetComponent<EA038_Fruit>();

            GameObject fruitObj = GetDeactiveChild(GetObject((int)Objects.FruitPool).transform
                .GetChild((int)reverseMap[Pos[i]]).transform);

            fruitObj.SetActive(true);
            fruitObj.transform.DOScale(fruit.originalScale, 1f).SetEase(Ease.OutBack)
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
            var tmp = codePool[i];
            codePool[i] = codePool[j];
            codePool[j] = tmp;
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
        correctObjectList.Clear();
        _uiManager.PopInstructionUIFromScaleZero($"내 나이 {gamePlayAge}살이 써진 블럭을 터치해주세요!", 12345f);        
        
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
            GetDeactiveChild(GetObject((int)Objects.BlockPool).transform.GetChild((int)reverseMap[Pos[i]]).transform)
                    .transform.position
                = GetObject((int)Objects.SetObjectPositions).transform.GetChild(i).transform.position;


            EA038_Block block = GetDeactiveChild(GetObject((int)Objects.BlockPool).transform
                .GetChild((int)reverseMap[Pos[i]]).transform).transform.gameObject.GetComponent<EA038_Block>();

            GameObject blockObj = GetDeactiveChild(GetObject((int)Objects.BlockPool).transform
                .GetChild((int)reverseMap[Pos[i]]).transform);

            float randomValue = Random.Range(-45f, 45f);
            randomEuler = new Vector3(0f, randomValue, 0f);
            
            blockObj.SetActive(true);
            blockObj.transform.localEulerAngles = randomEuler;
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
            var tmp = blockRandomList[i];
            blockRandomList[i] = blockRandomList[j];
            blockRandomList[j] = tmp;
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


    private GameObject GetDeactiveChild(Transform parent)
    {
        foreach (Transform child in parent)
            if (!child.gameObject.activeSelf)
                return child.gameObject;
        return null;
    }


    #region 사물 게임 순서 정하는 기능

    private enum ObjGameType
    {
        Car,
        Fruit,
        Block
    }

    private List<ObjGameType> objGameTypes = new List<ObjGameType>
    {
        ObjGameType.Car,
        ObjGameType.Fruit,
        ObjGameType.Block
    };

    private ObjGameType firstStage;
    private ObjGameType secondStage;

    private void DecidePlayObjGame()
    {
        int i = Random.Range(0, objGameTypes.Count);
        firstStage = objGameTypes[i];

        objGameTypes.RemoveAt(i);

        int j = Random.Range(0, objGameTypes.Count);
        secondStage = objGameTypes[j];

    }
    
    #endregion
    
    private void StartFirstObjectGame()
    {
        string promptText;
        string narrationKey;
        TweenCallback spawnAction;

        switch (firstStage)
        {
            case ObjGameType.Car:
                promptText = $"내 나이 {gamePlayAge}살이 써진 자동차를 터치해주세요!";
                narrationKey = "EA038/Audio/audio_13_내_나이_숫자가_써진_자동차를_터치해주세요_";
                spawnAction = SettingCarObject;
                break;
            case ObjGameType.Fruit:
                promptText = $"내 나이 {gamePlayAge}살이 써진 과일을 터치해주세요!";
                narrationKey = "EA038/Audio/audio_12_내_나이_숫자가_써진_과일을_터치해주세요_";
                spawnAction = SettingFruitGame;
                break;
            case ObjGameType.Block:
                promptText = $"내 나이 {gamePlayAge}살이 써진 블럭을 터치해주세요!";
                narrationKey = "EA038/Audio/audio_14_내_나이_숫자가_써진_블럭을_터치해주세요_";
                spawnAction = SettingBlockGame;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        DOTween.Sequence()
            .AppendCallback(() =>
                _uiManager.PopInstructionUIFromScaleZero("이번엔 다른 놀이를 해볼까요?", 4f,
                    narrationPath: "EA038/Audio/audio_10_이번엔_다른_놀이를_해볼까요_"))
            .AppendInterval(3f)
            .AppendCallback(() =>
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_11_내_나이_개수만큼_제시된_사물을_모아주세요_"))
            .AppendInterval(4.8f)
            .AppendCallback(() =>
            {
                spawnAction();
                _uiManager.PopInstructionUIFromScaleZero(promptText, 12345f, narrationPath: narrationKey);
            })
            .AppendInterval(5f)
            .AppendCallback(() => _uiManager.PlayReadyAndStart())
            .AppendInterval(5f)
            .AppendCallback(() => canPlayGame = true)
            ;
    }

    private void StartSecondObjectGame()
    {
        string promptText;
        string narrationKey;
        TweenCallback spawnAction;

        switch (secondStage)
        {
            case ObjGameType.Car:
                promptText = $"내 나이 {gamePlayAge}살이 써진 자동차를 터치해주세요!";
                narrationKey = "EA038/Audio/audio_13_내_나이_숫자가_써진_자동차를_터치해주세요_";
                spawnAction = SettingCarObject;
                break;
            case ObjGameType.Fruit:
                promptText = $"내 나이 {gamePlayAge}살이 써진 과일을 터치해주세요!";
                narrationKey = "EA038/Audio/audio_12_내_나이_숫자가_써진_과일을_터치해주세요_";
                spawnAction = SettingFruitGame;
                break;
            case ObjGameType.Block:
                promptText = $"내 나이 {gamePlayAge}살이 써진 블럭을 터치해주세요!";
                narrationKey = "EA038/Audio/audio_14_내_나이_숫자가_써진_블럭을_터치해주세요_";
                spawnAction = SettingBlockGame;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        DOTween.Sequence()
            .AppendCallback(() => _uiManager.PopInstructionUIFromScaleZero("이번엔 다른 놀이를 해볼까요?", 4f,
                narrationPath: "EA038/Audio/audio_10_이번엔_다른_놀이를_해볼까요_"))
            .AppendInterval(3f)
            .AppendCallback(() =>
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA038/Audio/audio_11_내_나이_개수만큼_제시된_사물을_모아주세요_"))
            .AppendInterval(4.8f)
            .AppendCallback(() =>
            {
                spawnAction();
                _uiManager.PopInstructionUIFromScaleZero(promptText, 12345f, narrationPath: narrationKey);
            })
            .AppendInterval(5f)
            .AppendCallback(() => _uiManager.PlayReadyAndStart())
            .AppendInterval(5f)
            .AppendCallback(() => canPlayGame = true)
            ;
    }

    

    


}
