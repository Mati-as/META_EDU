using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EA035_WinterFood_GameManager : Ex_BaseGameManager
{
    // 작업중
    private enum MainSeq
    {
        Default,
        Intro,

        Bread_Intro,
        Bread_Flour,
        Bread_Bean,
        Bread_Eat,
        Bread_Finish,

        Fish_Intro,
        Fish_Flour,
        Fish_Bean,
        Fish_Eat,
        Fish_Finish,
      
        OnFinish
    }


    public enum Objs
    {
        SeatSelectionController,
        SelectionController,
        ButtonController,
        MainPlate,

        FlourBowl,

        FishBreadSet,
        SteamedBunSet,

        FlourInBowl_HalfA,
        FlourInBowl_HalfB,
        FlourInBowl_Whole,

        ReadBeanInKnead,
        RedBeanPool,
        BeanTargetPos,
        
        OriginPosParent,
        Buns,
        Fishes,
        GeneratePosOnOven,
        GeneratePosOnMould,
 
        UnCookedFishA,
        UnCookedFishB,
        RedBeanInsideFishA,
        RedBeanInsideFishB,
        Flour
        // FlourBag
    }

    private SeatSelectionController _seatSelectionController;
    private ButtonClickEventController _buttonClickEventController;
    private Dictionary<int, Vector3> originalPosMap = new();

    private Transform[] buns; 
    private Transform[] fishes; 
    public int CurrentMainMainSeq
    {
        get
        {
            return currentMainMainSequence;
        }
        set
        {
            currentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {((MainSeq)CurrentMainMainSeq).ToString()}");

            ChangeThemeSeqAnim(value);
            switch (value)
            {
                case (int)MainSeq.Default:
                    break;

                case (int)MainSeq.Intro:

                    OnMainIntro();
                    break;


                case (int)MainSeq.Bread_Intro:
                    _currentFlourClickedCount = 0;
                    
                    
                    baseUIManager.PopInstructionUIFromScaleZero("먼저 호빵을 만들어 볼까요?");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/LetsMakeBun");
                    
                    Logger.ContentTestLog("Main Plate Active -------");
                    GetObject((int)Objs.MainPlate).transform.localScale = Vector3.zero;
                    GetObject((int)Objs.MainPlate).SetActive(true);
                    GetObject((int)Objs.MainPlate).transform.DOScale(_defaultSizeMap[(int)Objs.MainPlate], 0.5f)
                        .SetEase(Ease.OutBounce);
                    
                    //음성 로직 here
                    _buttonClickEventController.ChangeBtnImage("Runtime/EA035/Flour");


                    //2.5f -> sound.length
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        _buttonClickEventController.StartBtnClickSequential();
                        baseUIManager.PopInstructionUIFromScaleZero("그릇을 터치해서 밀가루를 담아주세요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/TouchBowlForFlour");
                    });
                    break;

                case (int)MainSeq.Bread_Flour:
                    baseUIManager.PopInstructionUIFromScaleZero("이제 반죽을 할까요?");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/LetsKnead");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        _isClickableForRound = true;
                        _buttonClickEventController.ChangeBtnImage("Runtime/EA035/Bowl");
                      //  _buttonClickEventController.StartBtnClickAnyOrder();
                        baseUIManager.PopInstructionUIFromScaleZero("반죽을 터치해주세요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/KneadDough");
                    });
                    break;

                case (int)MainSeq.Bread_Bean:
                    GetObject((int)Objs.MainPlate).SetActive(true);
                   
                    
                    baseUIManager.PopInstructionUIFromScaleZero("반죽안에 맛있는 팥을 넣어요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/LetsPutRedBean");
                    
                    GetObject((int)Objs.ReadBeanInKnead).transform.localScale = Vector3.zero;
                    GetObject((int)Objs.ReadBeanInKnead).SetActive(true);
                    _buttonClickEventController.ChangeBtnImage("Runtime/EA035/RedBean");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        _isClickableForRound = true;
                        _buttonClickEventController.StartBtnClickAnyOrder();
                        baseUIManager.PopInstructionUIFromScaleZero("그릇에 있는 팥을 터치해주세요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/TouchRedBean");
                        
                    });
                    break;

                case (int)MainSeq.Bread_Eat:
                    GetObject((int)Objs.MainPlate).SetActive(true);
                    
                    baseUIManager.PopInstructionUIFromScaleZero("따끈따끈한 호빵을 먹어요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/LetsEatBun");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        baseUIManager.PopInstructionUIFromScaleZero("그릇에 있는 호빵을 터치해서 먹어요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/LetsEatBunB");
                        
                        int count = 0;
                        var centerPos = GetObject((int)Objs.BeanTargetPos).transform.position;
                        float goldenAngle = 137.5f; // 나선형 균등 배치에 사용되는 각도 (도 단위)

                        for (int i = 0; i < buns.Length; i++)
                        {
                            var bun = buns[i];

                            bun.rotation = Quaternion.Euler(0,Random.Range(0, 360), 0); // 랜덤 회전
                            // 균등 분산 각도
                            float angleInRad = Mathf.Deg2Rad * (i * goldenAngle);
                            float radius = 0.65f + Random.Range(-0.4f, 0.10f); // 반지름에 약간의 변동
                            Vector3 offset = new Vector3(Mathf.Cos(angleInRad), 0, Mathf.Sin(angleInRad)) * radius;

                            
                            var targetPos = centerPos + offset;

                            bun.transform.DOJump(targetPos, 0.5f, 1, Random.Range(0.2f, 0.25f))
                                .SetEase(Ease.OutQuad)
                                .OnStart(() =>
                                {
                                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA035/Pop");
                                })
                                .SetDelay(i * Random.Range(0.1f, 0.3f))
                                .JoinCallback(() =>
                                {
                                    bun.transform.DOShakeScale(0.15f, 0.1f, 5, 70);
                                    bun.transform.DOShakeRotation(0.15f, 0.1f, 5, 70);
                                });
                        }
                        DOVirtual.DelayedCall(2f,()=>
                        {
                            _isClickableForRound = true;
                        });
                        
                    });
                    break;

                case (int)MainSeq.Bread_Finish:
                    baseUIManager.PopInstructionUIFromScaleZero("와~! 호빵 다 먹었다!",narrationPath:"EA035/YayAteAllBun");
                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA035/Success");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/YayAteAllBun");
                    break;

                case (int)MainSeq.Fish_Intro:
                    baseUIManager.PopInstructionUIFromScaleZero("이번엔 붕어빵을 만들어 볼까요?",narrationPath:"EA035/LetsMakeFishBread");
                    _buttonClickEventController.ChangeBtnImage("Runtime/EA035/Flour");
                    _currentFlourClickedCount = 0;
                   
                    //음성 로직 here

                    //2.5f -> sound.length
                    //2.5f -> sound.length
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        baseUIManager.PopInstructionUIFromScaleZero("그릇을 터치해서 밀가루를 담아주세요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/TouchBowlForFlour");
                        
                        _buttonClickEventController.StartBtnClickSequential();
                    });
                    break;

                case (int)MainSeq.Fish_Flour:
                    GetObject((int)Objs.FlourBowl).SetActive(true);
                    GetObject((int)Objs.MainPlate).SetActive(true);
                    baseUIManager.PopInstructionUIFromScaleZero("이제 반죽을 할까요?");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/LetsKnead");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        _isClickableForRound = true;
                        _buttonClickEventController.ChangeBtnImage("Runtime/EA035/Bowl");
                       // _buttonClickEventController.StartBtnClickAnyOrder();
                        baseUIManager.PopInstructionUIFromScaleZero("반죽을 터치해주세요!");
                        Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/KneadDough");
                    });
                    break;

                case (int)MainSeq.Fish_Bean:

                    GetObject((int)Objs.FlourInBowl_Whole).transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.OutBack);

                    
                    GetObject((int)Objs.UnCookedFishA).transform.localScale = Vector3.zero;
                    GetObject((int)Objs.UnCookedFishB).transform.localScale = Vector3.zero;
                    
                    GetObject((int)Objs.UnCookedFishA).SetActive(true);
                    GetObject((int)Objs.UnCookedFishB).SetActive(true);
                    
                    GetObject((int)Objs.UnCookedFishA).transform.DOScale(_defaultSizeMap[(int)Objs.UnCookedFishA], 0.5f)
                        .SetEase(Ease.OutBounce);
                    
                    GetObject((int)Objs.UnCookedFishB).transform.DOScale(_defaultSizeMap[(int)Objs.UnCookedFishB], 0.5f)
                        .SetEase(Ease.OutBounce);
                  //  GetObject((int)Objs.ReadBeanInKnead).transform.localScale = Vector3.zero;
                 //   GetObject((int)Objs.ReadBeanInKnead).SetActive(true);
                    
                    baseUIManager.PopInstructionUIFromScaleZero("반죽안에 맛있는 팥을 넣어요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA035/KneadDough");
                    _buttonClickEventController.ChangeBtnImage("Runtime/EA035/RedBean");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        _isClickableForRound = true;
                        _buttonClickEventController.StartBtnClickAnyOrder();
                        baseUIManager.PopInstructionUIFromScaleZero("그릇에 있는 팥을 터치해주세요!",narrationPath:"EA035/TouchRedBean");
                    });
                    break;

                case (int)MainSeq.Fish_Eat:
                    baseUIManager.PopInstructionUIFromScaleZero("따끈따끈한 붕어빵을 먹어요!",narrationPath:"EA035/LetsEatFishBread");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                      
                      
                        int count = 0;
                        var centerPos = GetObject((int)Objs.BeanTargetPos).transform.position;
                        float goldenAngle = 137.5f; // 나선형 균등 배치에 사용되는 각도 (도 단위)

                        for (int i = 0; i < fishes.Length; i++)
                        {
                            var fish = fishes[i];

                            fish.rotation = Quaternion.Euler(Random.Range(85f,95f), Random.Range(0, 360), 0); // 랜덤 회전
                            // 균등 분산 각도
                            float angleInRad = Mathf.Deg2Rad * (i * goldenAngle);
                            float radius = 0.65f + Random.Range(-0.4f, 0.10f); // 반지름에 약간의 변동
                            Vector3 offset = new Vector3(Mathf.Cos(angleInRad), 0, Mathf.Sin(angleInRad)) * radius;

                            var targetPos = centerPos + offset;

                            fish.transform.DOJump(targetPos, 0.5f, 1, Random.Range(0.2f, 0.25f))
                                .SetEase(Ease.OutQuad)
                                .SetDelay(i * Random.Range(0.1f, 0.3f))
                                .OnStart(() =>
                                {
                                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA035/Pop");
                                })
                                .JoinCallback(() =>
                                {
                                    fish.transform.DOShakeScale(0.15f, 0.1f, 5, 70);
                                    fish.transform.DOShakeRotation(0.15f, 0.1f, 5, 70);
                                });
                        }
                        DOVirtual.DelayedCall(2f,()=>
                        {
                            _isClickableForRound = true;
                        });
                        
                    });
                    
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        baseUIManager.PopInstructionUIFromScaleZero("그릇에 있는 붕어빵을 터치해서 먹어요!",narrationPath:"EA035/LetsEatFishBreadB");
                    });
                    break;

                case (int)MainSeq.Fish_Finish:
                    baseUIManager.PopInstructionUIFromScaleZero("와~! 붕어빵 다 먹었다!",narrationPath:"EA035/YayAteAllFish");
                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA035/Success");
                    break;


                case (int)MainSeq.OnFinish:
                    baseUIManager.PopInstructionUIFromScaleZero("(마무리텍스트 필요)붕어빵, 호빵을 모두 만들어서 먹어봤어요!");
                    break;
            }
        }
    }


    protected override void Init()
    {
        base.Init();
        BindObject(typeof(Objs));
        
        InitializeRedBeanPrefabs();
        InitializePool();
        
        
        GetObject((int)Objs.FlourBowl).SetActive(false);
        GetObject((int)Objs.UnCookedFishA).SetActive(false);
        GetObject((int)Objs.UnCookedFishB).SetActive(false);
        GetObject((int)Objs.FishBreadSet).SetActive(false);
        GetObject((int)Objs.SteamedBunSet).SetActive(false);
        GetObject((int)Objs.FlourInBowl_Whole).SetActive(false);
        GetObject((int)Objs.FlourInBowl_HalfA).SetActive(false);
        GetObject((int)Objs.FlourInBowl_HalfB).SetActive(false);
        GetObject((int)Objs.MainPlate).SetActive(false);
        GetObject((int)Objs.ReadBeanInKnead).SetActive(false);
        GetObject((int)Objs.ReadBeanInKnead).transform.localScale = Vector3.zero;
        
        GetObject((int)Objs.Flour).SetActive(false);
        GetObject((int)Objs.Flour).transform.localScale = Vector3.zero;
        buns = new Transform[GetObject((int)Objs.Buns).transform.childCount];
        BUN_COUNT_TO_EAT = buns.Length;
        int count = 0;
        foreach (var bun in buns)
        {
            buns[count] = GetObject((int)Objs.Buns).transform.GetChild(count);
            count++;
        }
        //GetObject((int)Objs.FlourBag).SetActive(false);
        
        fishes = new Transform[GetObject((int)Objs.Fishes).transform.childCount];
        FISH_COUNT_TO_EAT = fishes.Length;
        count = 0;
        foreach (var fish in fishes)
        {
            fishes[count] = GetObject((int)Objs.Fishes).transform.GetChild(count);
            count++;
        }

        _buttonClickEventController = GetObject((int)Objs.ButtonController)
            .GetComponent<ButtonClickEventController>();

        _buttonClickEventController.OnButtonClicked -= OnBtnClickEvent;
        _buttonClickEventController.OnButtonClicked += OnBtnClickEvent;
        
        for(int i =0; i < GetObject((int)Objs.OriginPosParent).transform.childCount; i++)
        {
            originalPosMap.Add(i, GetObject((int)Objs.OriginPosParent).transform.GetChild(i).position);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _buttonClickEventController.OnButtonClicked -= OnBtnClickEvent;
    }


#if UNITY_EDITOR
    [SerializeField] private MainSeq SEQ_ON_START_BTN;
#else
    MainSeq SEQ_ON_START_BTN = MainSeq.Intro;
#endif

    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        CurrentMainMainSeq = (int)SEQ_ON_START_BTN;
    }


    private void OnMainIntro()
    {
        var introSeq = DOTween.Sequence();
        GetObject((int)Objs.FishBreadSet).transform.localScale = Vector3.zero;
        GetObject((int)Objs.SteamedBunSet).transform.localScale = Vector3.zero;

        GetObject((int)Objs.FishBreadSet).SetActive(true);
        GetObject((int)Objs.SteamedBunSet).SetActive(true);

        introSeq.AppendInterval(2.0f);
        introSeq.AppendCallback(() =>
        {
            baseUIManager.PopInstructionUIFromScaleZero("추운 겨울에는 어떤 음식이 있을까요?",narrationPath:"EA035/WhatWinterFood");
        });
        introSeq.AppendInterval(2.5f);
        introSeq.Append(
            GetObject((int)Objs.SteamedBunSet).transform.DOScale(_defaultSizeMap[(int)Objs.FishBreadSet], 0.7f)
                .SetEase(Ease.OutExpo));
        introSeq.AppendCallback(() =>
        {
            baseUIManager.PopInstructionUIFromScaleZero("호빵",narrationPath:"EA035/BunName");
        });
        introSeq.AppendInterval(1.0f);
        introSeq.Append(
            GetObject((int)Objs.FishBreadSet).transform.DOScale(_defaultSizeMap[(int)Objs.FishBreadSet], 0.7f)
                .SetEase(Ease.OutExpo));
        introSeq.AppendCallback(() =>
        {
            baseUIManager.PopInstructionUIFromScaleZero("붕어빵",narrationPath:"EA035/FishBreadName");
        });
        introSeq.AppendInterval(1.5f);
        introSeq.AppendCallback(() =>
        {
            baseUIManager.PopInstructionUIFromScaleZero("이 두가지 음식을 만들어 볼까요?",narrationPath:"EA035/LetsMakeThoseTwo");
        });
        introSeq.AppendInterval(1.0f);
        introSeq.Append(
            GetObject((int)Objs.SteamedBunSet).transform.DOScale(Vector3.zero, 0.7f)
                .SetEase(Ease.OutExpo));
        introSeq.Append(
            GetObject((int)Objs.FishBreadSet).transform.DOScale(Vector3.zero, 0.7f)
                .SetEase(Ease.OutExpo));
        introSeq.AppendInterval(1.5f);
        introSeq.AppendCallback(() =>
        {
            CurrentMainMainSeq = (int)MainSeq.Bread_Intro;
        });


    }

    private const int CLICK_COUNT_ON_FLOUR = 7;
    private int _currentFlourClickedCount;

    private int BUN_COUNT_TO_EAT;
    private int _currentBunCount;

    private int FISH_COUNT_TO_EAT;
    private int _currentFishCount;

    public override void OnRaySynced()
    {
        base.OnRaySynced();

        if ((CurrentMainMainSeq == (int)MainSeq.Bread_Flour || CurrentMainMainSeq == (int)MainSeq.Fish_Flour) 
            && _isClickableForRound)
        {
            foreach (var hit in GameManager_Hits)
            {
                if (_tfIdToEnumMap.TryGetValue(hit.transform.GetInstanceID(), out int _))
                {
                    OnKneadDough(CurrentMainMainSeq==(int)MainSeq.Bread_Flour ? MainSeq.Bread_Bean : MainSeq.Fish_Bean);
                    Managers.Sound.PlayRandomEffect("Audio/Common/Click/Click", 'D');
                }
            }
        }
        else if (CurrentMainMainSeq == (int)MainSeq.Bread_Eat)
        {
            foreach (var hit in GameManager_Hits)
            {
                RaycastHit hitCache = hit;
                int id = hitCache.transform.GetInstanceID();
                string Bread;
                if (hitCache.transform.gameObject.name.Contains(nameof(Bread)))
                {
                    _isClickableMap.TryAdd(id,true);
                    if (_isClickableMap[id] == false) continue;
                    _isClickableMap[id] = false;
                    
                    _currentBunCount++;
                    
                    Managers.Sound.PlayRandomEffect("Audio/Common/Click/Click", 'D');
                    hitCache.transform.DOScale(Vector3.zero, Random.Range(0.3f,0.4f)).SetEase(Ease.InElastic);
                    
                    if(_currentBunCount >= BUN_COUNT_TO_EAT)
                    {
                        _currentBunCount = 0;
                        _isClickableForRound = false;
                        baseUIManager.PopInstructionUIFromScaleZero("와! 호빵을 다 먹었어요!",narrationPath:"EA035/YayAteAllBun");
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA035/Success");
                        DOVirtual.DelayedCall(5f, () =>
                        {
                            CurrentMainMainSeq = (int)MainSeq.Fish_Intro;
                        });
                    }
                }

            }
        }
        else if (CurrentMainMainSeq == (int)MainSeq.Fish_Eat)
        {
            foreach (var hit in GameManager_Hits)
            {
                RaycastHit hitCache = hit;
                int id = hitCache.transform.GetInstanceID();
                string Fish;
                if (hitCache.transform.gameObject.name.Contains(nameof(Fish)))
                {
                    _isClickableMap.TryAdd(id,true);
                    if (_isClickableMap[id] == false) continue;
                    _isClickableMap[id] = false;
                    
                    _currentFishCount++;
                    
                    Managers.Sound.PlayRandomEffect("Audio/Common/Click/Click", 'D');
                    hitCache.transform.DOScale(Vector3.zero, Random.Range(0.3f,0.4f)).SetEase(Ease.InElastic);
                    
                    if(_currentFishCount>= FISH_COUNT_TO_EAT)
                    {
                        _currentFishCount = 0;
                        _isClickableForRound = false;
                        baseUIManager.PopInstructionUIFromScaleZero("와! 붕어빵을 다 먹었어요!",narrationPath:"EA035/YayAteAllFish");
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA035/Success");
                        DOVirtual.DelayedCall(5f, () =>
                        {
                            CurrentMainMainSeq = (int)MainSeq.OnFinish;
                        });
                    }
                }

            }
        }
    }
    


    private Sequence _flourKneadingSeq;
    private int _kneadCount;
    private const int MAX_KNEAD_COUNT = 20;

    private Sequence _redBeanSeq;
    private int _beanCount;
    private const int MAX_BEAN_COUNT = 20;
    private bool _isClickableForRound= false;


    private void OnKneadDough(MainSeq nextSeq)
    {
        
        _kneadCount++;
        
        _flourKneadingSeq?.Kill();
        _flourKneadingSeq = DOTween.Sequence();

        _flourKneadingSeq.Append(GetObject((int)Objs.FlourInBowl_Whole).transform
            .DOShakeScale(1f, 0.45f, 5, 70)
            .SetEase(Ease.InOutBack));
        _flourKneadingSeq.OnKill(() =>
        {
            GetObject((int)Objs.FlourInBowl_Whole).transform.localScale =
                _defaultSizeMap[(int)Objs.FlourInBowl_Whole]
                * (1 + _kneadCount / (float)MAX_KNEAD_COUNT);
        });

        if (_kneadCount >= MAX_KNEAD_COUNT && _isClickableForRound)
        {
            _isClickableForRound = false;
            baseUIManager.PopInstructionUIFromScaleZero("와! 호빵 반죽이 완성되었어요!",narrationPath:"EA035/YayBunKnead");
            Managers.Sound.Play(SoundManager.Sound.Effect, "EA035/Success");
            _buttonClickEventController.DeactivateAllButtons();
            _currentFlourClickedCount = 0;

            DOVirtual.DelayedCall(3.5f, () =>
            {
                CurrentMainMainSeq = (int)nextSeq;
            });
        }

    }

    private Sequence _flourKneadingSeqB;
    protected override void OnBtnClickEvent(int clickedIndex)
    {
        Logger.ContentTestLog($"OnBtnClickEvent: {clickedIndex}");
        switch (CurrentMainMainSeq)
        {
            case (int)MainSeq.Bread_Intro:
                OnBtnClickedOnFlour();
                break;

            case (int)MainSeq.Bread_Flour:
                OnKneadDough(nextSeq:MainSeq.Bread_Bean);
                break;


            case (int)MainSeq.Bread_Bean:
                _beanCount++;
                _flourKneadingSeq?.Kill();
                _flourKneadingSeq = DOTween.Sequence();
                LaunchBeans(clickedIndex);
                _flourKneadingSeq.Append(GetObject((int)Objs.ReadBeanInKnead).transform.
                    DOScale(_defaultSizeMap[(int)Objs.ReadBeanInKnead] * (_beanCount / (float)MAX_BEAN_COUNT),0.3f));
                _flourKneadingSeq.Join(GetObject((int)Objs.ReadBeanInKnead).transform.
                    DOShakeScale(0.10f, 0.15f, 5, 70));

                if (_beanCount >= MAX_BEAN_COUNT && _isClickableForRound)
                {
                    _isClickableForRound = true;
                    _buttonClickEventController.DeactivateAllButtons();
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA035/BunReadBean");
                    
                    
                    DOVirtual.DelayedCall(5.5f, () =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.Bread_Eat;
                    });
                }
                break;

  

            case (int)MainSeq.Fish_Intro:
                OnBtnClickedOnFlour();
                break;
            case (int)MainSeq.Fish_Flour:
                OnKneadDough(nextSeq:MainSeq.Fish_Bean);
                break;

            case (int)MainSeq.Fish_Bean:
                _beanCount++;
                _flourKneadingSeq?.Kill();
                _flourKneadingSeq = DOTween.Sequence();
                
                _flourKneadingSeqB?.Kill();
                _flourKneadingSeqB = DOTween.Sequence();
                
                LaunchBeans(clickedIndex);
                
                _flourKneadingSeq.Append(GetObject((int)Objs.RedBeanInsideFishA).transform.
                    DOScale(_defaultSizeMap[(int)Objs.RedBeanInsideFishA] * (_beanCount / (float)MAX_BEAN_COUNT),0.3f));
                _flourKneadingSeq.Join(GetObject((int)Objs.RedBeanInsideFishA).transform.
                    DOShakeScale(0.10f, 0.15f, 5, 70));
                
                _flourKneadingSeqB.Append(GetObject((int)Objs.RedBeanInsideFishB).transform.
                    DOScale(_defaultSizeMap[(int)Objs.RedBeanInsideFishB] * (_beanCount / (float)MAX_BEAN_COUNT),0.3f));
                _flourKneadingSeqB.Join(GetObject((int)Objs.RedBeanInsideFishB).transform.
                    DOShakeScale(0.10f, 0.15f, 5, 70));


                if (_beanCount >= MAX_BEAN_COUNT && _isClickableForRound)
                {
                    _isClickableForRound = true;
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA035/FishRedBean");
                    _buttonClickEventController.DeactivateAllButtons();
                    
                    
                    
                    DOVirtual.DelayedCall(5.5f, () =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.Fish_Eat;
                    });
                }
             
                break;


            case (int)MainSeq.OnFinish:

                break;
        }
    }

    private void InitializeRedBeanPrefabs()
    {
        foreach (Transform child in GetObject((int)Objs.RedBeanPool).transform)
        {
            _readBeanPrefabs.Add(child.gameObject);
        }
    }

    [SerializeField] private int poolSize = 350;

    private readonly List<GameObject> _readBeanPrefabs = new();
    private readonly Queue<GameObject> _readBeanPool = new();

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var prefab = _readBeanPrefabs[Random.Range(0, _readBeanPrefabs.Count)];
            var instance = Instantiate(prefab, transform);
            instance.SetActive(false);
            _readBeanPool.Enqueue(instance);
        }
    }

    public GameObject GetCandyFromPool()
    {
        if (_readBeanPool.Count > 0)
        {
            var obj = _readBeanPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning("Pool empty!");
        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        _readBeanPool.Enqueue(obj);
    }

    public void LaunchBeans(int index)
    {
        int launchCount = 2;

        var originParent = originalPosMap[index];
        var target = GetObject((int)Objs.BeanTargetPos).transform;

    


        var origin = originalPosMap[index];

        for (int i = 0; i < launchCount; i++)
            LaunchRedBeanFrom(origin, target.position +
                                             new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f),
                                                 Random.Range(-0.2f, 0.2f)));
    }
    
    private void LaunchRedBeanFrom(Vector3 originPos, Vector3 targetPos)
    {
        var readBean = GetCandyFromPool();
        if (readBean == null) return;

        var rb = readBean.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody on candy!");
            return;
        }

        readBean.transform.position = originPos;
        readBean.transform.rotation = Quaternion.identity;

        var dir = (targetPos - originPos).normalized;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        readBean.transform.DOJump(targetPos, Random.Range(0.1f, 0.6f), 1,
            Random.Range(0.25f, 0.8f))
            .SetEase(Ease.OutQuad).OnComplete(() =>
        {
            ReturnToPool(readBean);
        });
        //rb.AddForce(dir * Random.Range(1.0f, 4f), ForceMode.Impulse);
    }

    private void OnBtnClickedOnFlour()
    {
        _currentFlourClickedCount++;
        GetObject((int)Objs.Flour).SetActive(true);
        GetObject((int)Objs.Flour).transform.DOScale(_defaultSizeMap[(int)Objs.Flour] *
                _currentFlourClickedCount /(float) CLICK_COUNT_ON_FLOUR, 0.7f)
            .SetEase(Ease.OutExpo);
        
        if (_currentFlourClickedCount >= CLICK_COUNT_ON_FLOUR)
        {
            _currentFlourClickedCount = 0;
            
         
            
            
            GetObject((int)Objs.FlourBowl).transform.localScale = Vector3.zero;
            GetObject((int)Objs.FlourInBowl_Whole).SetActive(true);
            GetObject((int)Objs.FlourBowl).SetActive(true);
            GetObject((int)Objs.FlourBowl).transform.DOScale(_defaultSizeMap[(int)Objs.FlourBowl], 0.7f)
                .SetEase(Ease.OutExpo);

            baseUIManager.PopInstructionUIFromScaleZero("와! 밀가루를 그릇에 담았어요!",narrationPath:"EA035/YayFlour");
            Managers.Sound.Play(SoundManager.Sound.Effect, "EA035/Success");
            _buttonClickEventController.DeactivateAllButtons();
            DOVirtual.DelayedCall(3f, () =>
            {   GetObject((int)Objs.Flour).transform.DOScale(Vector3.zero, 0.7f).SetEase(Ease.OutExpo);
                CurrentMainMainSeq = CurrentMainMainSeq == (int)MainSeq.Bread_Intro ? (int)MainSeq.Bread_Flour : (int)MainSeq.Fish_Flour;
            });
        }
    } 
}


