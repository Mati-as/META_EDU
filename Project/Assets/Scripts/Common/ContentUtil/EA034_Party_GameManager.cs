using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EA034_Party_GameManager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Default,
        Intro,
        OnCream,
        OnDecorate,
        OnCandle,
        OnCelebrate,
        OnBlowCandle,
        OnFinish,
      
    }


    public enum Objs
    {
        SeatSelection,
        Buttons,
        CakeA,
        CakeB,
        CakeC,
        CakeCream_A,
        CakeCream_B,
        CakeCream_C,
        CandySetRoot,
        OriginPosParent,
        TargetPos,
        CandleStartPos,
        Candles
    }


    public int CurrentMainMainSeq
    {
        get
        {
            return CurrentMainMainSequence;
        }
        set
        {
            CurrentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {((MainSeq)CurrentMainMainSeq).ToString()}");

            ChangeThemeSeqAnim(value);
            switch (value)
            {
                case (int)MainSeq.Default:
                    break;

                case (int)MainSeq.Intro:
                    Managers.Sound.Play(SoundManager.Sound.Narration,"SortedByScene/EA034/audio_1_케이크를_만들어볼까요_");
                    baseUIManager.PopFromZeroInstructionUI("친구들! 각자 자리에 앉아 주세요!");
                    DOVirtual.DelayedCall(1f, () =>
                    {


                        _seatSelectionController.StartSeatSelection();
                    });
                    break;

                case (int)MainSeq.OnCream:
                  
                  
                    _buttonClickEventController.ChangeBtnImage("Runtime/EA034/CreamImage");
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration,"SortedByScene/EA034/audio_2__생크림을_터치해_빵에_생크림을_발라주세요___생크림을_터치해주세요");
                        baseUIManager.PopFromZeroInstructionUI("생크림을 터치해주세요!");

                        DOVirtual.DelayedCall(10f, () =>
                        {
                            Managers.Sound.Play(SoundManager.Sound.Narration,
                                "SortedByScene/EA034/audio_3__생크림을_더_많이_발라주세요__");
                        });
                        _buttonClickEventController.StartBtnClickAnyOrder();
                    });

                    break;
                case (int)MainSeq.OnDecorate:

                    _elapsed = 0f;
                    _buttonClickEventController.ChangeBtnImage("Runtime/EA034/Deco");
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration,
                            "SortedByScene/EA034/audio_5_생크림_위에_달콤한_재료로_꾸며볼까요___달콤한_재료를_골라~");
                        baseUIManager.PopFromZeroInstructionUI("달콤한 재료를 골라 터치해주세요");
                        _buttonClickEventController.StartBtnClickAnyOrder();
                    });
                    break;

                case (int)MainSeq.OnCandle:
                    _buttonClickEventController.DeactivateAllButtons();
                    
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        baseUIManager.PopFromZeroInstructionUI("초를 터치해 꽂아주세요");
                        _buttonClickEventController.ChangeBtnImage("Runtime/EA034/Candle");
                        _buttonClickEventController.StartBtnClickAnyOrder();
                    });
                    
                    _currentClickCount = 0;
                  
                    break;

                case (int)MainSeq.OnCelebrate:
                    Managers.Sound.Play(SoundManager.Sound.Narration,
                        "SortedByScene/EA034/audio_8_케익에_초를_꽂았어요__형님이_되었으니_축하_노래를_불러볼까요_");
                    baseUIManager.PopFromZeroInstructionUI("케잌에 초를 꽂았어요!\n형님이 되었으니 축하 노래를 불러볼까요?");
                    break;
                
                case (int)MainSeq.OnBlowCandle:
                    Managers.Sound.Play(SoundManager.Sound.Narration,
                        "SortedByScene/EA034/audio_9_초에_불을_꺼볼까요__초에_있는_불을_터치해_주세요_");
                    baseUIManager.PopFromZeroInstructionUI("초를 터치해서 불을 꺼주세요");
                    break;
                  
                case (int)MainSeq.OnFinish:
                    Managers.Sound.Play(SoundManager.Sound.Narration,
                        "SortedByScene/EA034/audio_10__친구들__형님이_되었네요_축하해요_");
                    baseUIManager.PopFromZeroInstructionUI("형님이 되었네요 축하해요!");
                    break;
            }
        }
    }

    private SeatSelectionController _seatSelectionController;
    private ButtonClickEventController _buttonClickEventController;


    private Dictionary<int, Transform> _candleTransformMap = new();
    private Dictionary<int, Vector3> _candleArrivalPos = new();
    
    protected override void Init()
    {
        base.Init();
        baseUIManager = UIManagerObj.GetComponent<Base_UIManager>();
        BindObject(typeof(Objs));

        InitializeCandyPrefabs();
        InitializePool();
        _seatSelectionController = GetObject((int)Objs.SeatSelection).GetComponent<SeatSelectionController>();
        _buttonClickEventController = GetObject((int)Objs.Buttons).GetComponent<ButtonClickEventController>();

        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
        _buttonClickEventController.OnButtonClicked += OnButtonClicked;

        _buttonClickEventController.OnAllBtnClicked -= OnAllBtnClicked;
        _buttonClickEventController.OnAllBtnClicked += OnAllBtnClicked;

        _seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
        _seatSelectionController.OnAllSeatSelected += OnAllSeatSelected;

        GetObject((int)Objs.CakeA).SetActive(true);
        GetObject((int)Objs.CakeB).SetActive(true);
        GetObject((int)Objs.CakeC).SetActive(true);

        GetObject((int)Objs.CakeCream_A).SetActive(false);
        GetObject((int)Objs.CakeCream_B).SetActive(false);
        GetObject((int)Objs.CakeCream_C).SetActive(false);

        for (int i = 0; i < GetObject((int)Objs.Candles).transform.childCount; i++)
        {
            Transform candle = GetObject((int)Objs.Candles).transform.GetChild(i);
            
            _candleArrivalPos.Add(i, candle.position);
            
            candle.position = GetObject((int)Objs.CandleStartPos).transform.position;
            _candleTransformMap.Add(i, candle);
            candle.gameObject.SetActive(false);
        }
    }


#if UNITY_EDITOR
    [SerializeField] private MainSeq _startSeq;
    [SerializeField]
    [Range(0, 60)]
    private float DECO_TIME = 20; 
#else
     [SerializeField] private const float DECO_TIME = 15; 
    [SerializeField] private MainSeq _startSeq = MainSeq.Intro;
#endif

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();

   
        CurrentMainMainSeq = (int)_startSeq;
    }

    private void OnAllSeatSelected()
    {
        Logger.ContentTestLog("전체 자리 선택 완료 -----------------------EA_Party");
        baseUIManager.PopFromZeroInstructionUI("잘했어! 모두 자리에 앉았구나!");


        DOVirtual.DelayedCall(2.5f, () =>
        {
            CurrentMainMainSeq = (int)MainSeq.OnCream;
        });


        //_seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
    }

    private void OnAllBtnClicked()
    {

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
        _seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
        _seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
    }


    private const int COUNT_TO_CLICK_ONCREAM = 21;
    private int _currentClickCount;
    private bool isCakeRotatable = true;
    private bool _isCakeRoundFinish;

    private int _cakeCurrentRound;
    private readonly int ROUND_COUNT_ONCREAM = 2; // 생크림 올리기 라운드 수 (0부터시작이라 3개 라운드) 


    private const int COUNT_TO_CLICK_ONCANDLE = 15;
    private readonly int CANDLE_ROUND_COUNT = 6; // 양초 올리기 라운드 수(갯수) (0부터시작이라 3개 라운드) 
    private int _candleCurrentRound;

    private void OnButtonClicked(int clickedButtonIndex)
    {
        switch (CurrentMainMainSeq)
        {
            case (int)MainSeq.Default:
                break;

            case (int)MainSeq.Intro:
                break;

            case (int)MainSeq.OnCream:
                OnBtnClickedOnCream();
                break;
            case (int)MainSeq.OnDecorate:
                
                OnBtnClickedOnDeco(clickedButtonIndex);
                break;

            case (int)MainSeq.OnCandle:
                OnBtnClickedOnCandle();
                break;

            case (int)MainSeq.OnCelebrate:
                break;

            case (int)MainSeq.OnFinish:
                break;
        }
    }

    private bool isCandleRoundFinished = false;

    private void OnBtnClickedOnCandle()
    {
        if(isCandleRoundFinished) return;
        _currentClickCount++;
        if (_currentClickCount > COUNT_TO_CLICK_ONCANDLE && _candleCurrentRound >= CANDLE_ROUND_COUNT)
        {
            isCandleRoundFinished = true;
            _currentClickCount = 0;
            PutCandleOnCake(_candleCurrentRound);
            _buttonClickEventController.DeactivateAllButtons();
         
          

            DOVirtual.DelayedCall(2.5f, () =>
            {
                CurrentMainMainSeq = (int)MainSeq.OnCelebrate;
            });
        }
        else if (_currentClickCount > COUNT_TO_CLICK_ONCANDLE)
        {
            _currentClickCount = 0;
            PutCandleOnCake(_candleCurrentRound);
            _candleCurrentRound++;
           
        }
      
    }

    private void PutCandleOnCake(int candleIndex)
    {
        _candleTransformMap[candleIndex].DOMove(_candleArrivalPos[candleIndex], 1.2f).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            _candleTransformMap[candleIndex].DOScale(Vector3.one*1.2f, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _candleTransformMap[candleIndex].DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine);
            });
        });
        _candleTransformMap[candleIndex].gameObject.SetActive(true);
    }

    private void OnBtnClickedOnCream()
    {
        _currentClickCount++;
        if (_currentClickCount > COUNT_TO_CLICK_ONCREAM && _cakeCurrentRound >= ROUND_COUNT_ONCREAM)
        {
            ChangeCakeToCreamOne(_cakeCurrentRound); //마지막 케이크
            _isCakeRoundFinish = true;
            _buttonClickEventController.DeactivateAllButtons();
            _currentClickCount = 0;

            Managers.Sound.Play(SoundManager.Sound.Narration,"SortedByScene/EA034/audio_4__와____달콤한__생크림_케이크가_만들어지고_있어요_");
            baseUIManager.PopFromZeroInstructionUI("생크림 케이크가 만들어지고 있어요!");

            DOVirtual.DelayedCall(2.5f, () =>
            {
                CurrentMainMainSeq = (int)MainSeq.OnDecorate;
            });
        }
        else if (_currentClickCount > COUNT_TO_CLICK_ONCREAM)
        {
            _currentClickCount = 0;
            ChangeCakeToCreamOne(_cakeCurrentRound);
            _cakeCurrentRound++;
        }
        else
        {
            if (_isCakeRoundFinish) return;
            _sequenceMap.TryAdd((int)Objs.CakeA + _cakeCurrentRound, DOTween.Sequence());


            if (!isCakeRotatable) return;
            isCakeRotatable = false;
            DOVirtual.DelayedCall(0.3f, () =>
            {
                isCakeRotatable = true;
            });

            var t = GetObject((int)Objs.CakeA + _cakeCurrentRound).transform;

            var targetRot = t.localRotation * Quaternion.Euler(0, 15, 0);

            _sequenceMap[(int)Objs.CakeA + _cakeCurrentRound]?.Kill();
            _sequenceMap[(int)Objs.CakeA + _cakeCurrentRound] = DOTween.Sequence();

            _sequenceMap[(int)Objs.CakeA + _cakeCurrentRound]
                .Append(t.DOLocalRotateQuaternion(targetRot, 0.1f).SetEase(Ease.OutBack));
        }
    }

    private void ChangeCakeToCreamOne(int currentCake)
    {
        GetObject((int)Objs.CakeCream_A + currentCake).transform.localScale = Vector3.zero;
        GetObject((int)Objs.CakeCream_A + currentCake).SetActive(true);

        GetObject((int)Objs.CakeA + currentCake)
            .transform.DOScale(Vector3.zero, 0.55f).SetEase(Ease.InOutCirc).OnComplete(() =>
            {
                GetObject((int)Objs.CakeCream_A + currentCake).transform
                    .DOScale(_defaultSizeMap[(int)Objs.CakeCream_A + currentCake], 0.25f).SetEase(Ease.InOutCirc);
            });
    }

    //  [SerializeField] private Transform candySetRoot; // CandySet 오브젝트
    [SerializeField] private int poolSize = 500;

    private readonly List<GameObject> _candyPrefabs = new();
    private readonly Queue<GameObject> _pool = new();


    private void OnBtnClickedOnDeco(int index)
    {
        LaunchCandies(index);
    }

    private void InitializeCandyPrefabs()
    {
        foreach (Transform child in GetObject((int)Objs.CandySetRoot).transform) _candyPrefabs.Add(child.gameObject);
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var prefab = _candyPrefabs[Random.Range(0, _candyPrefabs.Count)];
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

    [SerializeField] private float launchPower = 10f; // 발사 힘 조절 변수

    public void LaunchCandies(int index = -1)
    {
        int launchCount = 3;

        var originParent = GetObject((int)Objs.OriginPosParent).transform;
        var target = GetObject((int)Objs.TargetPos).transform;

        if (originParent.childCount == 0)
        {
            Debug.LogWarning("No origin points available!");
            return;
        }


        var origin = originParent.GetChild(index);

        for (int i = 0; i < launchCount; i++) LaunchCandyFrom(origin.position, target.position +
            new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)));
    }

    private float _elapsed;

    private void Update()
    {
        if (CurrentMainMainSeq == (int)MainSeq.OnDecorate)
        {
            
            _elapsed += Time.deltaTime;
            
            
            if (_elapsed >= DECO_TIME)
            {
                _elapsed = 0f;
                
                Managers.Sound.Play(SoundManager.Sound.Narration,"SortedByScene/EA034/audio_6_달콤한_케이크가_완성되었어요_");
                baseUIManager.PopFromZeroInstructionUI("달콤한 케이크가 완성되었어요!");
                DOVirtual.DelayedCall(4f,()=>
                {
                    CurrentMainMainSeq = (int)MainSeq.OnCandle;
                });
            }
       
        }

   
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
        rb.AddForce(dir * Random.Range(1.8f,3.7f), ForceMode.Impulse);
    }
}