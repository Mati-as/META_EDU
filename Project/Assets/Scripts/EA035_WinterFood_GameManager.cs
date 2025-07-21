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

        FlourBowl,
        UnCookedFishA,
        UnCookedFishB,
        FishBreadSet,
        SteamedBunSet,

        FlourInBowl_HalfA,
        FlourInBowl_HalfB,
        FlourInBowl_Whole,

        ReadBeanInKnead,
        RedBeanPool,
        BeanTargetPos,

        OriginPosParent
        // FlourBag
    }

    private SeatSelectionController _seatSelectionController;
    private ButtonClickEventController _buttonClickEventController;
    private Dictionary<int, Vector3> originalPosMap = new();

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
                    baseUIManager.PopInstructionUIFromScaleZero("먼저 호빵을 만들어 볼까요?");
                    //음성 로직 here
                    _buttonClickEventController.ChangeBtnImage("Runtime/EA035/Flour");


                    //2.5f -> sound.length
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        _buttonClickEventController.StartBtnClickSequential();
                        baseUIManager.PopInstructionUIFromScaleZero("그릇을 터치해서 밀가루를 담아주세요!");
                    });
                    break;

                case (int)MainSeq.Bread_Flour:
                    baseUIManager.PopInstructionUIFromScaleZero("이제 반죽을 할까요?");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        _buttonClickEventController.ChangeBtnImage("Runtime/EA035/Bowl");
                        _buttonClickEventController.StartBtnClickAnyOrder();
                        baseUIManager.PopInstructionUIFromScaleZero("반죽 그릇을 터치해주세요!");
                    });
                    break;

                case (int)MainSeq.Bread_Bean:
                    baseUIManager.PopInstructionUIFromScaleZero("반죽안에 맛있는 팥을 넣어요!");
                    _buttonClickEventController.ChangeBtnImage("Runtime/EA035/RedBean");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        _buttonClickEventController.StartBtnClickAnyOrder();
                        baseUIManager.PopInstructionUIFromScaleZero("그릇에 있는 팥을 터치해주세요!");
                    });
                    break;

                case (int)MainSeq.Bread_Eat:
                    baseUIManager.PopInstructionUIFromScaleZero("따끈따끈한 호빵을 먹어요!");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        baseUIManager.PopInstructionUIFromScaleZero("그릇에 있는 호빵을 터치해서 먹어요!");
                    });
                    break;

                case (int)MainSeq.Bread_Finish:
                    baseUIManager.PopInstructionUIFromScaleZero("와~! 호빵 다 먹었다!");
                    break;

                case (int)MainSeq.Fish_Intro:
                    baseUIManager.PopInstructionUIFromScaleZero("이번엔 붕어빵을 만들어 볼까요?");
                    //음성 로직 here

                    //2.5f -> sound.length
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        baseUIManager.PopInstructionUIFromScaleZero("그릇을 터치해 밀가루를 담아주세요!");
                    });
                    break;

                case (int)MainSeq.Fish_Flour:
                    baseUIManager.PopInstructionUIFromScaleZero("이제 반죽을 할까요?");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        baseUIManager.PopInstructionUIFromScaleZero("반죽 그릇을 터치해주세요!");
                    });
                    break;

                case (int)MainSeq.Fish_Bean:

                    GetObject((int)Objs.ReadBeanInKnead).transform.localScale = Vector3.zero;
                    GetObject((int)Objs.ReadBeanInKnead).SetActive(true);
                    
                    baseUIManager.PopInstructionUIFromScaleZero("반죽안에 맛있는 팥을 넣어요!");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        baseUIManager.PopInstructionUIFromScaleZero("그릇에 있는 팥을 터치해주세요!");
                    });
                    break;

                case (int)MainSeq.Fish_Eat:
                    baseUIManager.PopInstructionUIFromScaleZero("따끈따끈한 붕어빵을 먹어요!");
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        baseUIManager.PopInstructionUIFromScaleZero("그릇에 있는 호빵을 터치해서 먹어요!");
                    });
                    break;

                case (int)MainSeq.Fish_Finish:
                    baseUIManager.PopInstructionUIFromScaleZero("와~! 호빵 다 먹었다!");
                    break;


                case (int)MainSeq.OnFinish:
                    baseUIManager.PopInstructionUIFromScaleZero("붕어빵, 호빵을 모두 만들어서 먹어봤어요!");
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

        GetObject((int)Objs.ReadBeanInKnead).SetActive(false);
        GetObject((int)Objs.ReadBeanInKnead).transform.localScale = Vector3.zero;
        //GetObject((int)Objs.FlourBag).SetActive(false);

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
            baseUIManager.PopInstructionUIFromScaleZero("추운 겨울에는 어떤 음식이 있을까요?");
        });
        introSeq.AppendInterval(2.5f);
        introSeq.Append(
            GetObject((int)Objs.SteamedBunSet).transform.DOScale(_defaultSizeMap[(int)Objs.FishBreadSet], 0.7f)
                .SetEase(Ease.OutExpo));
        introSeq.AppendCallback(() =>
        {
            baseUIManager.PopInstructionUIFromScaleZero("호빵");
        });
        introSeq.AppendInterval(2.0f);
        introSeq.Append(
            GetObject((int)Objs.FishBreadSet).transform.DOScale(_defaultSizeMap[(int)Objs.FishBreadSet], 0.7f)
                .SetEase(Ease.OutExpo));
        introSeq.AppendCallback(() =>
        {
            baseUIManager.PopInstructionUIFromScaleZero("붕어빵");
        });
        introSeq.AppendInterval(2.0f);
        introSeq.AppendCallback(() =>
        {
            baseUIManager.PopInstructionUIFromScaleZero("이 두가지 음식을 만들어 볼까요?");
        });
        introSeq.AppendInterval(3.0f);
        introSeq.AppendCallback(() =>
        {
            CurrentMainMainSeq = (int)MainSeq.Bread_Intro;
        });

        introSeq.AppendInterval(2.5f);
        introSeq.Append(
            GetObject((int)Objs.SteamedBunSet).transform.DOScale(Vector3.zero, 0.7f)
                .SetEase(Ease.OutExpo));
        introSeq.Append(
            GetObject((int)Objs.FishBreadSet).transform.DOScale(Vector3.zero, 0.7f)
                .SetEase(Ease.OutExpo));
    }


    private const int CLICK_COUNT_ON_FLOUR = 7;
    private int _currentFlourClickedCount;

    public override void OnRaySynced()
    {
        base.OnRaySynced();
    }

    private Sequence _flourKneadingSeq;
    private int _kneadCount;
    private const int MAX_KNEAD_COUNT = 20;

    private Sequence _redBeanSeq;
    private int _beanCount;
    private const int MAX_BEAN_COUNT = 20;



    protected override void OnBtnClickEvent(int clickedIndex)
    {
        Logger.ContentTestLog($"OnBtnClickEvent: {clickedIndex}");
        switch (CurrentMainMainSeq)
        {
            case (int)MainSeq.Bread_Intro:

                _currentFlourClickedCount++;
                if (_currentFlourClickedCount >= CLICK_COUNT_ON_FLOUR)
                {
                    _currentFlourClickedCount = 0;
                    GetObject((int)Objs.FlourBowl).transform.localScale = Vector3.zero;
                    GetObject((int)Objs.FlourInBowl_Whole).SetActive(true);
                    GetObject((int)Objs.FlourBowl).SetActive(true);
                    GetObject((int)Objs.FlourBowl).transform.DOScale(_defaultSizeMap[(int)Objs.FlourBowl], 0.7f)
                        .SetEase(Ease.OutExpo);

                    baseUIManager.PopInstructionUIFromScaleZero("와! 밀가루를 그릇에 담았어요!");
                    _buttonClickEventController.DeactivateAllButtons();
                    DOVirtual.DelayedCall(3f, () =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.Bread_Flour;
                    });
                }
                break;

            case (int)MainSeq.Bread_Flour:
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
                        * (1 + _kneadCount * 0.04f);
                });


                if (_kneadCount >= MAX_KNEAD_COUNT)
                {
                    baseUIManager.PopInstructionUIFromScaleZero("와! 호빵 반죽이 완성되었어요!");
                    _buttonClickEventController.DeactivateAllButtons();
                    _currentFlourClickedCount = 0;

                    DOVirtual.DelayedCall(3.5f, () =>
                    {
                        CurrentMainMainSeq = (int)MainSeq.Bread_Bean;
                    });
                }
                break;


            case (int)MainSeq.Bread_Bean:
                _beanCount++;

                _flourKneadingSeq?.Kill();
                _flourKneadingSeq = DOTween.Sequence();
                LaunchBeans(clickedIndex);
                GetObject((int)Objs.ReadBeanInKnead).transform.localScale =
                    _defaultSizeMap[(int)Objs.ReadBeanInKnead] * (_beanCount / (float)MAX_BEAN_COUNT);

                if (_beanCount >= MAX_BEAN_COUNT)
                {
                 
                }

                break;

            case (int)MainSeq.Bread_Eat:
                break;


            case (int)MainSeq.Fish_Flour:

                break;

            case (int)MainSeq.Fish_Bean:

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

    [SerializeField] private int poolSize = 500;

    private readonly List<GameObject> _readBeanPrefabs = new();
    private readonly Queue<GameObject> _pool = new();

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var prefab = _readBeanPrefabs[Random.Range(0, _readBeanPrefabs.Count)];
            var instance = Instantiate(prefab, transform);
            instance.SetActive(false);
            _pool.Enqueue(instance);
        }
    }

    public GameObject GetCandyFromPool()
    {
        if (_pool.Count > 0)
        {
            var obj = _pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning("Pool empty!");
        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }

    public void LaunchBeans(int index)
    {
        int launchCount = 2;

        var originParent = originalPosMap[index];
        var target = GetObject((int)Objs.BeanTargetPos).transform;

    


        var origin = originalPosMap[index];

        for (int i = 0; i < launchCount; i++)
            LaunchCandyFrom(origin, target.position +
                                             new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f),
                                                 Random.Range(-0.2f, 0.2f)));
    }
    
    private void LaunchCandyFrom(Vector3 originPos, Vector3 targetPos)
    {
        var candy = GetCandyFromPool();
        if (candy == null) return;

        var rb = candy.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody on candy!");
            return;
        }

        candy.transform.position = originPos;
        candy.transform.rotation = Quaternion.identity;

        var dir = (targetPos - originPos).normalized;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(dir * Random.Range(1.0f, 4f), ForceMode.Impulse);
    }
}


