using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;


public class EA029_ParkingGame_GameManager : Ex_BaseGameManager
{
    /// <summary>
    ///     1.
    /// </summary>
    private enum MainSeq
    {
        Default,
        OnCarInit=10,
            
        SeatSelectionA,
        OnRaceA,
        SeatSelectionB,
        OnRaceB, //OnStart, OnRoundFinish,
        OnFinish,
        
        OnWinnerShow=11,ㄹㄴ
        
        
    }

    private readonly Dictionary<int, Animator> _carAnimators = new();
    private readonly Dictionary<int, GameObject> _cars = new();
    private readonly Dictionary<int, Transform> _arrowMap = new();
     
    public Dictionary<int, string> _currentTrackNameForAnimStateMap = new();
    private Dictionary<int, string> currentTrackNameForAnimStateMap
    {
        get
        {
            return _currentTrackNameForAnimStateMap;
        }
        set
        {
            Logger.Log("--------------------Setting new currentTrackNameForAnimStateMap");
            _currentTrackNameForAnimStateMap =value;
        }
       
    }
    private Dictionary<int,GameObject> selectedCars = new();
    //구간별 초기화 필요
    private Dictionary<int, int> rankingsPerTrack = new();
    [SerializeField]
    private Color _defaultSeatColor;

    private readonly Vector3 DEFAULT_SIZE =2.0f * Vector3.one; 
    private EA029_UIManager _uiManager;
    private int currentRoundCount =0;
    
    #if UNITY_EDITOR
    [SerializeField]
    [Range(1,10)]
    private int MAX_ROUND_COUNT = 6;
    #else

    private const int MAX_ROUND_COUNT = 6;
    #endif
  

    private enum CarAnim
    {
        Default,
        
        TrackAA,
        TrackAB,
        TrackAC,
        TrackAD,
        TrackBA,
        TrackBB,
        TrackBC,
        TrackBD,
        
        OnWinner,
        
        
        
    }

   
    private enum Objs
    {
        TrackA_Set,
        Seats_TrackA,
        TrackA,

        TrackB_Set,
        Seats_TrackB,
        TrackB,

        NormalizedCar,
        
        Seat_AA,
        Seat_AB,
        Seat_AC,
        Seat_AD,
        Seat_BA,
        Seat_BB,
        Seat_BC,
        Seat_BD,
        
        Excavator,
        Bulldozer,
        RMC,
        Truck,
        Taxi,
        PoliceCar,
        FireTruck,
        Ambulance,
    }

    private Dictionary<int, string> _nameInKoreanMap;

    public int CurrentMainMainSeq
    {
        get => CurrentMainMainSequence;
        set
        {
            CurrentMainMainSequence = value;

            //  Messenger.Default.Publish(new EA012Payload(_currentMainSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {((MainSeq)CurrentMainMainSeq).ToString()}");
            // commin Init Part.
            isRoundActive = false;

            
            ChangeThemeSeqAnim(value);
            switch (value)
            {
                
                case (int)MainSeq.OnCarInit:
                    currentRoundCount++;
                    if (CheckIfMaxRoundCount()) return;
                   
                    InitCarsPerRound();
                    DeactivateSeats();
               
                    if (currentRoundCount % 2 == 1)
                    {
                        GetObject((int)Objs.TrackA_Set).SetActive(true);
                        GetObject((int)Objs.TrackB_Set).SetActive(false);
                    }
                    else
                    {
                        GetObject((int)Objs.TrackA_Set).SetActive(false);
                        GetObject((int)Objs.TrackB_Set).SetActive(true);
                
                    }
                    break;
                    
                
                
                case (int)MainSeq.SeatSelectionA:
                    ScaleBackSeats();
                    _uiManager.PopInstructionUIFromScaleZero("마음에드는 차를 골라\n네모 모양에 서주세요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA029/Narration/OnSeatSelection");
                    InitForSeatSelection();
                    AnimateAllSeats(Objs.Seats_TrackA);
                    
                    
                    break;

                case (int)MainSeq.SeatSelectionB:
             
                    _uiManager.PopInstructionUIFromScaleZero("마음에드는 차를 골라\n네모 모양에 서주세요!");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA029/Narration/OnSeatSelection");
                    InitForSeatSelection();
                    ScaleBackSeats();
                    AnimateAllSeats(Objs.Seats_TrackB);
                    break;

                
                
                case (int)MainSeq.OnRaceA:
                    break;
                
                case (int)MainSeq.
                    OnRaceB:
                    break;

                case (int)MainSeq.OnFinish:
                    Managers.Sound.Play(SoundManager.Sound.Narration, "Audio/EA029/Narration/ThankPark");
                    _uiManager.PopInstructionUIFromScaleZero("차를 전부 주차했어요!");
                    RestartScene();
                    break;
            }
        }
    }

 

    protected override void Init()
    {
        psResourcePath = "Runtime/EA029/Fx_Click";
        subPsResourcePathMap.Add(0, "Runtime/EA029/OnArrival");
        SetSubPsPool(0);
        
        subPsResourcePathMap.Add(1, "Runtime/EA029/OnRun");
        SetSubPsPool(1);
        
        BindObject(typeof(Objs));
        InitCars();
        
        base.Init();
        DeactivateSeats();
        _defaultSeatColor = GetObject((int)Objs.Seat_AA).transform.GetComponent<MeshRenderer>().material.color;
        
                          
        
        _uiManager = UIManagerObj.GetComponent<EA029_UIManager>();
        

        
        _nameInKoreanMap = new()
        {
            {GetObject((int)Objs.Excavator).transform.GetInstanceID(), "굴착기"},
            {GetObject((int)Objs.Bulldozer).transform.GetInstanceID(), "불도저"},
            {GetObject((int)Objs.RMC).transform.GetInstanceID(), "레미콘"},
            {GetObject((int)Objs.Truck).transform.GetInstanceID(), "트럭"},
            {GetObject((int)Objs.Taxi).transform.GetInstanceID(), "택시"},
            {GetObject((int)Objs.PoliceCar).transform.GetInstanceID(), "경찰차"},
            {GetObject((int)Objs.FireTruck).transform.GetInstanceID(), "소방차"},
            {GetObject((int)Objs.Ambulance).transform.GetInstanceID(), "구급차"}
        };
        
        GetObject((int)Objs.TrackA_Set).SetActive(true);
        GetObject((int)Objs.TrackB_Set).SetActive(false);
    }



    private void ScaleBackSeats()
    {
        if (CurrentMainMainSeq == (int)MainSeq.SeatSelectionA)
        {
            for (int i = (int)Objs.Seat_AA; i < (int)Objs.Seat_AD + 1; i++)
            {
                GetObject(i).SetActive(true);
                GetObject(i).transform.DOScale(_defaultSizeMap[i], 0.5f).SetEase(Ease.InOutSine);

            }

        }
        else if (CurrentMainMainSeq == (int)MainSeq.SeatSelectionB)
        {
                       
            for (int i = (int)Objs.Seat_BA; i < (int)Objs.Seat_BD + 1; i++)
            {
                GetObject(i).SetActive(true);
                GetObject(i).transform.DOScale(_defaultSizeMap[i], 0.5f).SetEase(Ease.InOutSine);
            }


        }

    }

    private void DeactivateSeats()
    {
        // foreach (var key in _seatSequenceMap.Keys.ToArray())
        // {
        //     _seatSequenceMap[key]?.Kill();
        // }
        
        for (int i = (int)Objs.Seat_AA; i <= (int)Objs.Seat_AD; i++)
        {
            var iCache = i;
            GetObject(i).transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                GetObject(iCache).SetActive(false);
            });
        }

    
   
                   
        for (int i = (int)Objs.Seat_BA; i <= (int)Objs.Seat_BD; i++)
        {
            GetObject(i).SetActive(true);
            var iCache = i;
            GetObject(i).transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                GetObject(iCache).SetActive(false);
            });
        }


        

    }
    #region Seat Selection --------------------------------------------------------------------------------

    
    private void OnSeatSelection(MainSeq selection)
    {
        
        DOVirtual.DelayedCall(1.5f, () =>
        {
            initialMessage = "각자 표시된 자리에 서주세요!";
             baseUIManager.PopInstructionUIFromScaleZero(initialMessage);
            CurrentMainMainSeq = (int)selection;
            Logger.ContentTestLog("Mainseq Changed SeatSelection -------------------");
        });
    }

    private Dictionary<int, bool> isSeatClickedMap = new();
    private Dictionary<int, MeshRenderer> _seatMeshRendererMap = new();
    private int _seatClickedCount;
    [SerializeField] private Color _selectedColor;
    private static readonly int TrackNum = Animator.StringToHash("TrackNum");
    private const int MAX_SEAT_COUNT = 4;

    private void InitForSeatSelection()
    {
        isSeatClickedMap = new Dictionary<int, bool>();
        //_seatMeshRendererMap = new Dictionary<int, MeshRenderer>();
        _seatClickedCount = 0;
     
        
    }

    
    private void InitForNewRoundTrack()
    {
        _timeElapsed = 0;
        currentArrivedCarCount = 0;
        isRoundActive = false;
        _currentRankingOrder = 1; 
        transIDToIndex = new();
        _isWinnderIndexRegisted = false;
        _winnerIndex = -1;

        _lastNarratedSecond = -1;
        
        
        //car전부 enable 후 default로 옮기기 
        foreach (var key in _partProgress.Keys.ToArray())
        {
            _partProgress[key] = 0;
        }
        
        foreach (var key in isArrivedMap.Keys.ToArray())
        {
            isArrivedMap[key] = false;
        }
        foreach (var key in _carAnimators.Keys.ToArray())
        {
            _carAnimators[key].enabled = true;
            _carAnimators[key].SetTrigger(OnFinish);
            DOVirtual.DelayedCall(1f, () =>
            {
                _carAnimators[key].transform.gameObject.SetActive(false);
            });
        }
    }


    private void OnRaySyncedOnSeatSelection()
    {
        bool isAllSeatClicked = false;

        foreach (var hit in GameManager_Hits)
        {
            int hitTransformID = hit.transform.GetInstanceID();
            if (hit.transform.gameObject.name.Contains("Seat"))
            {
                int seatID = hit.transform.GetInstanceID();

                isSeatClickedMap.TryAdd(seatID, false);
                if (isSeatClickedMap[seatID]) return;
                isSeatClickedMap[seatID] = true;

                var renderer = hit.transform.GetComponent<MeshRenderer>();
                _seatMeshRendererMap.TryAdd(_tfIdToEnumMap[hitTransformID], renderer);
                _seatMeshRendererMap[_tfIdToEnumMap[hitTransformID]].material.DOColor(_selectedColor, 0.35f);

                Managers.Sound.Play(SoundManager.Sound.Effect, "EA029/Seat_" + (_seatClickedCount+1).ToString());
                _seatClickedCount++;

                _seatSequenceMap[_tfIdToEnumMap[hitTransformID]]?.Kill();
                if (_seatClickedCount >= MAX_SEAT_COUNT) isAllSeatClicked = true;
            }

            if (isAllSeatClicked)
            {
            
                Managers.Sound.Play(SoundManager.Sound.Effect, "Common/OnAllSeatSelected");
                Logger.ContentTestLog("모든 자리가 선택되었습니다--------");
                // Messenger.Default.Publish(new EA012Payload("OnSeatSelectFinished"));
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA018/Narration/OnSeatSelectFinished");
                _uiManager.PopInstructionUIFromScaleZero("다 앉았구나! 이제 자동차들을 보러가자!");
                
                DeactivateSeats();
                
                
                DOVirtual.DelayedCall(4, () =>
                {
                  



                    if (CurrentMainMainSeq == (int)MainSeq.SeatSelectionA)
                    {
                        CurrentMainMainSeq = (int)MainSeq.OnRaceA;
                        StartTrackRoundWithCount();
                    }
                    

                    else if (CurrentMainMainSeq == (int)MainSeq.SeatSelectionB)
                    {
                        CurrentMainMainSeq = (int)MainSeq.OnRaceB;
                        StartTrackRoundWithCount();
                    }
                        
                });
                break;
            }

            PlayParticleEffect(hit.point);
        }
    }

    private void AnimateAllSeats(Objs objs)
    {
        foreach (var key in _seatMeshRendererMap.Keys.ToArray())
        {
            _seatMeshRendererMap[key].material.DOColor(_defaultSeatColor, 0.2f);
        }
        
        
        if (objs == Objs.Seats_TrackA)
        {
              
            for (int i = (int)Objs.Seat_AA; i <= (int)Objs.Seat_AD; i++)
            {
                Logger.ContentTestLog($"AnimateAllSeats :Animating seat {(Objs)i}");
                AnimateSeatLoop((Objs)i);
            }

        }
        else
        {
                 
            for (int i = (int)Objs.Seat_BA; i <= (int)Objs.Seat_BD; i++)
            {
                Logger.ContentTestLog($"AnimateAllSeats :Animating seat {(Objs)i}");
                AnimateSeatLoop((Objs)i);
            }
        }
        
    }

    private Dictionary<int,Sequence> _seatSequenceMap = new Dictionary<int, Sequence>();
    
    private void AnimateSeatLoop(Objs seat)
    {
        GetObject((int)seat).SetActive(true);
        
        
        var SeatTransform = GetObject((int)seat).transform;

        _seatSequenceMap.TryAdd((int)seat, DOTween.Sequence());
        
        _seatSequenceMap[(int)seat]?.Kill();
        _seatSequenceMap[(int)seat] = DOTween.Sequence();
        _seatSequenceMap[(int)seat]
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 1.1f, 0.25f))
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 0.9f, 0.35f))
            .SetLoops(120, LoopType.Yoyo)
            .OnKill(() =>
            {
                SeatTransform.DOScale(_defaultSizeMap[(int)seat], 0.5f);
            });

       // _seatSequenceMap[(int)seat].Play();
    }

#endregion ---------------------------------------------------------------------------------

    public override void OnRaySynced()
    {
     
        base.OnRaySynced();
        
        if (CurrentMainMainSeq == (int)MainSeq.SeatSelectionA | CurrentMainMainSeq == (int)MainSeq.SeatSelectionB)
        {
            OnRaySyncedOnSeatSelection();
        }
        else if (CurrentMainMainSeq == (int)MainSeq.OnRaceA | CurrentMainMainSeq == (int)MainSeq.OnRaceB)
        {
           
            MoveCarTowardOnTrack();
        }

    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        
        _uiManager.PopInstructionUIFromScaleZero("차를 도와주세요! 누가 빨리가는지 볼까요?");
        Managers.Sound.Play(SoundManager.Sound.Narration, "EA029/Narration/Story/CarLookingFor");
        CurrentMainMainSeq =0;
        DOVirtual.DelayedCall(3.25f, () =>
        {
        CurrentMainMainSeq = (int)MainSeq.OnCarInit;
        });
    }

    private bool CheckIfMaxRoundCount()
    {
        if (currentRoundCount > MAX_ROUND_COUNT)
        {
            Logger.ContentTestLog("최대 라운드 수를 초과했습니다. ----------OnFinish");
            CurrentMainMainSeq = (int)MainSeq.OnFinish;
            mainAnimator.SetTrigger(Finish);
            return true;
        }
        else
        {
            Logger.ContentTestLog($"현재 라운드 수 : {currentRoundCount}");
            return false;
        }
    }

    private const int CAR_SEQUENCE_OFFSET = 100; // sequenceMap Key 충돌 방지용
    private Dictionary<int,int> transIDToIndex = new(); // 각 차량의 애니메이션 시퀀스 관리
    private int _currentRankingOrder = 1; //OnArrivalAnimation컨트롤위한 변수
    private Vector3 _arrowDefaultSize;
    #region Track Setting --------------------------------------------------------------------------------
    

        private const int TRACK_COUNT = 4;
    
        private void InitCars()
        {
            for (var i = (int)Objs.Excavator; i <= (int)Objs.Ambulance; i++)
            {
                var car = GetObject(i);
                _cars.Add(i, car);
                Animator Caranimator = car.GetComponent<Animator>();
                _carAnimators.Add(i, Caranimator);
                car.SetActive(false);
                car.transform.localScale = DEFAULT_SIZE;
                tfIDToAnimatorMap.Add(car.transform.GetInstanceID(), Caranimator);

                var Arrow = car.transform.Find("Arrow");
                _arrowMap.Add(car.transform.GetInstanceID(), Arrow);
                _arrowDefaultSize = Arrow.localScale;
                Arrow.localScale = Vector3.zero;
                Arrow.gameObject.SetActive(false);
                
                Logger.Log("Car :" + (Objs)i + $" added to _cars dictionary.{car.transform.GetInstanceID()}");
            }
        }

        private Sequence _CarIntroduceSequence;
        
        private void InitCarsPerRound()
        {

            foreach (var key in _isClickableMap.Keys.ToArray())
            {
                _isClickableMap[key] = true; // 클릭 가능 상태로 초기화
            }
            _CarIntroduceSequence?.Kill();
            _CarIntroduceSequence = DOTween.Sequence();
            
            currentTrackNameForAnimStateMap = new();
            selectedCars.Clear();
            _selectedCarAnimators.Clear();
            
            
           // _uiManager.PopFromZeroInstructionUI("차를 도와주세요! 누가 빨리가는지 볼까요?"); 
            
        

            List<int> carIndices = new List<int>(_cars.Keys);

            if (carIndices.Count < 4)
            {
                Logger.ContentTestLog("차량이 4개 미만입니다. InitOnRack 실패.");
                return;
            }

            // 키 셔플 후 앞에서 4개 선택
            
            Logger.ContentTestLog($"Car Indice Count : {carIndices.Count}----------");
            System.Random rng = new System.Random();

            for (int i = carIndices.Count - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (carIndices[i], carIndices[j]) = (carIndices[j], carIndices[i]);
            }

            foreach (var key in _carAnimators.Keys.ToArray())
            {
                _carAnimators[key].transform.gameObject.SetActive(false);
            }

            for (int i = 0; i < 4; i++)
            {
                int carIndex = carIndices[i];
                GameObject car = _cars[carIndex];
                Animator animator = _carAnimators[carIndex];
                _selectedCarAnimators.Add(i, animator);
                selectedCars.Add(i, car);
           
               


                int selectedCarID = car.transform.GetInstanceID();

                transIDToIndex.TryAdd(selectedCarID, i);
                
                
              
                
                
                if (currentRoundCount % 2 == 1)
                {
                    switch (i)
                    {
                        case 0:
                            currentTrackNameForAnimStateMap.Add(selectedCarID, "TrackAA");
                            break;
                        case 1:
                            currentTrackNameForAnimStateMap.Add(selectedCarID, "TrackAB");
                            break;
                        case 2:
                            currentTrackNameForAnimStateMap.Add(selectedCarID, "TrackAC");
                            break;
                        case 3:
                            currentTrackNameForAnimStateMap.Add(selectedCarID, "TrackAD");
                            break;
                    }
                    
     
                }
                else if(currentRoundCount % 2 == 0)
                {
                    switch (i)
                    {
                        case 0:
                            currentTrackNameForAnimStateMap.Add(selectedCarID, "TrackBA");
                            break;
                        case 1:
                            currentTrackNameForAnimStateMap.Add(selectedCarID, "TrackBB");
                            break;
                        case 2:
                            currentTrackNameForAnimStateMap.Add(selectedCarID, "TrackBC");
                            break;
                        case 3:
                            currentTrackNameForAnimStateMap.Add(selectedCarID, "TrackBD");
                            break;
                    }
                }
                
                
                int animInt = currentRoundCount % 2 == 1
                    ? ((int)CarAnim.TrackAA + i)
                    : ((int)CarAnim.TrackBA + i);
                
           

                //animator.SetInteger(TrackNum, animInt); // TrackNum이 string이면 확인 필요
               // animator.Play(currentTrackNameForAnimStateMap[selectedCarID], 0, 0);
                
                animator.enabled = false; // 애니메이션 비활성화
                animator.SetInteger(TrackNum, 0);

                Logger.ContentTestLog($"Selected Car:  {(Objs)carIndex} ID{selectedCarID}-> {(CarAnim)animInt} Activate");
                animator.transform.gameObject.SetActive(true);

                //Doshake
                //transform.DOShakeScale(1.0f, 1, 10, 90, true).SetEase(Ease.OutQuad);
            }



            DOVirtual.DelayedCall(4.5f, () =>
            {
                IntroduceCars(() =>
                {
                    if ((int)currentRoundCount % 2 == 1)
                    {
                        CurrentMainMainSeq = (int)MainSeq.SeatSelectionA;
                    }
                    else
                    {
                        CurrentMainMainSeq = (int)MainSeq.SeatSelectionB;
                    }

                });
            });
        }

        private readonly Dictionary<int, Animator> _selectedCarAnimators = new();
        private const int CAR_ANIM_INTRO = 101;

        private void IntroduceCars(Action onComplete)
        {
            float delay = 4.5f;
            int count = 0;
            int total = _selectedCarAnimators.Count;
            foreach (var key in _selectedCarAnimators.Keys.ToArray())
            {
                _selectedCarAnimators[key].transform.gameObject.SetActive(true); // 애니메이션 비활성화
            }

            foreach (int key in _selectedCarAnimators.Keys.ToArray())
            {
                //Logger.ContentTestLog($"Intro Car {count} ----------------------------");
                bool isLast = count == total - 1; // 마지막 차 판별

                int countCache = count;
                DOVirtual.DelayedCall(delay * countCache, () =>
                {
                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA029/CarIntro");
                    _selectedCarAnimators[key].enabled = true;
                    _selectedCarAnimators[key].SetInteger(TrackNum, CAR_ANIM_INTRO);
                    _uiManager.PopInstructionUIFromScaleZero(
                        $"{countCache + 1}번차, {_nameInKoreanMap[_selectedCarAnimators[key].transform.GetInstanceID()]}!");
                    
                    Managers.Sound.Play(SoundManager.Sound.Narration,"EA029/Narration/Number_" + (countCache + 1).ToString());
                    DOVirtual.DelayedCall(2.5f, () =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Narration,"EA029/Narration/CarName/"+_selectedCarAnimators[key].transform.gameObject.name);
                      
                    });
                    _selectedCarAnimators[key].transform.DOScale(DEFAULT_SIZE * 2.25f, 1f);
                    
                    //  _selectedCarAnimators[key].enabled = true;
                }).OnComplete(() =>
                {
                 
                    DOVirtual.DelayedCall(1.55f, () =>
                    {
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA029/CarStop");
                    });
                    DOVirtual.DelayedCall(5f, () =>
                    {
                        int animInt = currentRoundCount % 2 == 1
                            ? (int)CarAnim.TrackAA + countCache
                            : (int)CarAnim.TrackBA + countCache;
                        _selectedCarAnimators[key]
                            .Play(currentTrackNameForAnimStateMap[_selectedCarAnimators[key].transform.GetInstanceID()],
                                0,
                                0);

                        _selectedCarAnimators[key].SetInteger(TrackNum, animInt); // TrackNum이 string이면 확인 필요
                        DOVirtual.DelayedCall(0.1f, () =>
                        {
                            if (isLast) onComplete?.Invoke();
                            _selectedCarAnimators[key].enabled = false;
                            _selectedCarAnimators[key].transform.DOScale(DEFAULT_SIZE, 0.5f);
                        });
                    });
                });

                count++;
            }
        }

        private void StartTrackRoundWithCount()
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "EA029/CarStart");

            DOVirtual.DelayedCall(3f, () =>
            {
                _uiManager.PopInstructionUIFromScaleZero("친구들의 차를 터치해서 주차해주세요!");
                Managers.Sound.Play(SoundManager.Sound.Narration,"EA029/Narration/Story/TouchAndPark");
                DOVirtual.DelayedCall(4.5f, () =>
                {
                  //  _uiManager.PopFromZeroInstructionUI("준비!");
                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA029/RaceCount");
                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA029/Ready");

                    _uiManager.PlayReadyAndStart(() =>
                    {
                        //_uiManager.PopFromZeroInstructionUI("시작!", 0f);
                        // Managers.Sound.Play(SoundManager.Sound.Narration, "EA029/Start");

                        DOVirtual.DelayedCall(1.5f, () =>
                        {
                            char randomChar = (char)Random.Range('A', 'B' + 1);
                            Managers.Sound.Play(SoundManager.Sound.Narration, "EA029/Narration/Hurry" + randomChar);
                        });
                     
                     
                        isRoundActive = true;

                        foreach (var key in _arrowMap.Keys.ToArray())
                        {
                            _arrowMap[key].gameObject.SetActive(true);
                            _arrowMap[key].DOScale(_arrowDefaultSize, 0.75f).SetEase(Ease.InOutBounce);
                        }
                    
                    },0.75f);
             
                });
            });
        }
        private readonly Dictionary<int, int> _partProgress = new(); // 각 파트의 클릭 진행 상태
        private readonly Dictionary<int, bool> isArrivedMap = new();
        private readonly Dictionary<int, Animator> tfIDToAnimatorMap = new();
        private bool isRoundActive = false;
        private const int COUNT_TO_ARRIVE = 25;
        private int currentArrivedCarCount = 0; 
        private Vector3 _particlePosOffset = new Vector3(0, 0, -1.5f);
     
        private int _winnerIndex = -1;
        private bool _isWinnderIndexRegisted = false;
        private readonly Dictionary<int, int> _arrivalTriggerHashCache = new();
        private void MoveCarTowardOnTrack()
        {
            if (!isRoundActive) return;

        

            Logger.ContentTestLog($"gamemanager hit count :{GameManager_Hits.Length}");
            foreach (var hit in GameManager_Hits)
            {
         
                
                int hitID = hit.transform.GetInstanceID();
                
                Logger.ContentTestLog("hit : " + hit.transform.name + $":{hitID}");
                _isClickableMap.TryAdd(hitID, true);
                if (!_isClickableMap[hitID])
                {
                    Logger.ContentTestLog("클릭 불가.. 리턴");
                    return;
                }
             
                _isClickableMap[hitID] = false;
       

                

                // 현재 진행도 저장 및 증가
                isArrivedMap.TryAdd(hitID, false);
                if (isArrivedMap[hitID])
                {
                    Logger.ContentTestLog("hit : " + hit.transform.name + " is already arrived.");
                    return;
                }

                if (tfIDToAnimatorMap.TryGetValue(hitID, out var animator))
                {
                    var tf = animator.transform;
                    // OnCompletion 클립 가져오기
                    _defaultSizeMap.TryAdd(hitID, tf.localScale);
                    _defaultRotationQuatMap.TryAdd(hitID, tf.localRotation);


                    var clip = animator.runtimeAnimatorController.animationClips
                        .FirstOrDefault(c => c.name.Contains(currentTrackNameForAnimStateMap[hitID]));

                    Logger.Log($"ID: {currentTrackNameForAnimStateMap[hitID]}");

                    if (clip != null)
                    {
                        _partProgress.TryAdd(hitID, 0);
                        _partProgress[hitID] = Mathf.Min(_partProgress[hitID] + 1, COUNT_TO_ARRIVE);


                        float progressNormalized = _partProgress[hitID] / (float)COUNT_TO_ARRIVE;

                        //도착시 로직 
                        if (!isArrivedMap[hitID] && _partProgress[hitID] >= COUNT_TO_ARRIVE -3)
                        {
                            isArrivedMap[hitID] = true;
                            currentArrivedCarCount++;
                            
                            char randoChar = (char)Random.Range('A', 'B' + 1);
                            Managers.Sound.Play(SoundManager.Sound.Effect, "EA018/OnPartArrive_" + randoChar);

                            _arrowMap[hitID].gameObject.SetActive(false);
                     

                            _sequenceMap[hitID]?.Kill();
                            _sequenceMap[hitID] = DOTween.Sequence();

                            _sequenceMap[hitID].Append(tf.transform.DOScale(DEFAULT_SIZE * 1.2f, 0.25f)
                                .SetEase(Ease.OutBounce));
                            //  _sequenceMap[ID].Join(tf.transform.DOLocalRotate(Vector3.zero, 0.35f));
                            _sequenceMap[hitID].Append(tf.transform.DOScale(DEFAULT_SIZE * 0.8f, 0.2f)
                                .SetEase(Ease.OutBounce));
                            _sequenceMap[hitID].Append(tf.transform.DOScale(DEFAULT_SIZE * 1.2f, 0.25f)
                                .SetEase(Ease.OutBounce));
                            _sequenceMap[hitID].Append(tf.transform.DOScale(DEFAULT_SIZE, 0.5f));

                            _sequenceMap[hitID].AppendCallback(() =>
                            {
                                _isClickableMap[hitID] = false;
                                animator.enabled = true;
                                
                                int arrivalHash;
                                if (!_arrivalTriggerHashCache.TryGetValue(_currentRankingOrder, out arrivalHash))
                                {
                                    string triggerName = "Arrival_" + _currentRankingOrder.ToString();
                                    arrivalHash = Animator.StringToHash(triggerName);
                                    _arrivalTriggerHashCache[_currentRankingOrder] = arrivalHash;
                                }

                                animator.SetTrigger(arrivalHash);
                                
                                _currentRankingOrder++;
                            });

                            if (!_isWinnderIndexRegisted)
                            {
                                _winnerIndex = transIDToIndex[hitID];
                                _isWinnderIndexRegisted = true;
                            }


                            Logger.ContentTestLog($"a car Arrived {currentArrivedCarCount}");
                            PlaySubParticleEffect(0, hit.transform.position);


                            if (currentArrivedCarCount >= TRACK_COUNT && isRoundActive)
                            {
                                Logger.ContentTestLog("All Cars Arrived -------------------");
                                OnTrackRoundFinished();
                            }
                        }
                        else //도착 아닐 시
                        {
                
                            PlayParticleEffect(hit.point);

                            char randoChar = (char)Random.Range('A', 'E' + 1);
                            Managers.Sound.Play(SoundManager.Sound.Effect, "EA018/Click_" + randoChar);
                            // Animator 재생
                            animator.enabled = false;
                            _sequenceMap.TryAdd(hitID, DOTween.Sequence());
                            _sequenceMap[hitID]?.Kill();
                            _sequenceMap[hitID] = DOTween.Sequence();
                            _sequenceMap[hitID].Append(tf.DOScale(DEFAULT_SIZE * 1.45f, 0.15f).SetEase(Ease.OutBounce));

                            _sequenceMap[hitID].Append(tf.DOScale(DEFAULT_SIZE, 0.06f).SetEase(Ease.OutBounce));
                            _sequenceMap[hitID].AppendCallback(() =>
                            {
                                var randoPoss = Random.Range(0, 100f);
                                if (randoPoss > 60)
                                {
                                    char randomChar = (char)Random.Range('A', 'B' + 1);
                                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA029/OnCarMove"+ randomChar.ToString());
                                }
                                animator.enabled = true;
                                //Managers.Sound.Play(SoundManager.Sound.Effect, "EA029/CarMove",0.1f);
                                animator.Play(currentTrackNameForAnimStateMap[hitID], 0, progressNormalized);
                            });

                            float time = 0;
                            _sequenceMap[hitID].AppendCallback(() =>
                            {
                                DOVirtual.DelayedCall(0.1f, () =>
                                {
                                    _isClickableMap[hitID] = true;
                                    animator.enabled = false;
                                }).OnUpdate(() =>
                                {
                                    var pos = hit.transform.position + _particlePosOffset;
                                    DOVirtual.DelayedCall(0.05f, () =>
                                    {
                                        time += Time.deltaTime;
                                        if (time > 0.05f)
                                        {
                                            time = 0;
                                            PlaySubParticleEffect(1, pos);
                                        }
                                    });
                                });
                            });


                    
                            Logger.Log($"ID: progress: ->{currentTrackNameForAnimStateMap[hitID]}");
                        }
                    }
                    else
                        Logger.LogWarning("Clip is null");
                }
                else
                    Logger.LogWarning($"Animator not found for ID: {hitID}");
            }

           
        }
        
        
    #if UNITY_EDITOR
    [SerializeField] [Range(0, 60)] private int TIME_LIMIT;
    #else
    private int TIME_LIMIT = 60;
    #endif
    private float _timeElapsed;
    private static readonly int OnFinish = Animator.StringToHash("OnFinish");
    private int _lastNarratedSecond = -1;
    private bool isCountNarPlayed  = false;
    private static readonly int Finish = Animator.StringToHash("Finish");


    private float _timeElapsedForNarration = 0f;
    private const float NARRATION_INTERVAL = 12f; // 1초마다 내레이션 재생
    
    private void Update()
    {
        if (isRoundActive)
        {
            _timeElapsed += Time.deltaTime;
            _timeElapsedForNarration += Time.deltaTime;
            
            if (_timeElapsedForNarration >= Random.Range(NARRATION_INTERVAL-3,NARRATION_INTERVAL+3) && (TIME_LIMIT-_timeElapsed)>16)
            {
                isCountNarPlayed = true;
                _timeElapsedForNarration = 0;
                char randomChar = (char)Random.Range('A', 'B' + 1);
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA029/Narration/Hurry" + randomChar);
               
            }
            if (_timeElapsed >= TIME_LIMIT)
            {
             
                OnTrackRoundFinished();
            }
            int remainTime = TIME_LIMIT - (int)_timeElapsed;

            if (remainTime <= 10 && remainTime != _lastNarratedSecond)
            {
                _lastNarratedSecond = remainTime;
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA029/Narration/Count_" + remainTime);
                _uiManager.PopInstructionUIFromScaleZero($"{remainTime}");
            }
        }
    }

    private void OnTrackRoundFinished()
    {  
        
        _timeElapsedForNarration = 0;
        isRoundActive = false;
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA029/Stop");
        //_uiManager.PopFromZeroInstructionUI("그만!");
        _uiManager.PlayStopAnimation();
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA029/CarHonk");
        foreach (var key in _arrowMap.Keys.ToArray())
        {
            _arrowMap[key].DOScale(Vector3.zero, 0.75f).SetEase(Ease.InOutBounce);
            _arrowMap[key].gameObject.SetActive(false);
        }
        
        
        foreach (var key in _selectedCarAnimators.Keys.ToArray())
        {
            _selectedCarAnimators[key].enabled = true;
        }
        
        DOVirtual.DelayedCall(3f, () =>
        {

            if (_winnerIndex != -1)
            {
                _uiManager.PopInstructionUIFromScaleZero($"{_winnerIndex + 1}번 친구가 가장 먼저 들어왔어! 주차 성공!");
              
                Managers.Sound.Play(SoundManager.Sound.Narration, $"EA029/Narration/Story/Winner_{_winnerIndex+1}");
            }
            else
            {
                _uiManager.PopInstructionUIFromScaleZero("열심히 터치해서 차를 주차해요!"); 
            }
            Logger.ContentTestLog("");
            DOVirtual.DelayedCall(3f, () =>
            {  
                InitForNewRoundTrack();
                DOVirtual.DelayedCall(2f, () =>
                {
                    CurrentMainMainSeq = (int)MainSeq.OnCarInit;
                    // if (CurrentMainMainSeq == (int)MainSeq.OnRaceA)
                    // {
                    //     CurrentMainMainSeq = (int)MainSeq.SeatSelectionB;
                    // }
                    // else if (CurrentMainMainSeq == (int)MainSeq.OnRaceB)
                    // {
                    //     CurrentMainMainSeq = (int)MainSeq.SeatSelectionA;
                    // }

                });
          
            });
        });
        
        }

    #endregion


}
