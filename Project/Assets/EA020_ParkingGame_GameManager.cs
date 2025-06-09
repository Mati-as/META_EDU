using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;


public class EA020_ParkingGame_GameManager : Ex_BaseGameManager
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
        
        OnWinnerShow=11,
        
        
    }

    private readonly Dictionary<int, Animator> _carAnimators = new();
    private readonly Dictionary<int, GameObject> _cars = new();
     
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
    private EA020_UIManager _uiManager;
    private int currentRoundCount =0;
    private const int MAX_ROUND_COUNT = 6;

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
            Logger.ContentTestLog($"Current Sequence: {CurrentMainMainSeq.ToString()}");


            // commin Init Part.
            isRoundActive = false;
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
            
            ChangeThemeSeqAnim(value);
            switch (value)
            {
                
                case (int)MainSeq.OnCarInit:
                    currentRoundCount++;
                    if (CheckIfMaxRoundCount()) return;
                    InitCarsPerRound();
                    break;
                    
                
                
                case (int)MainSeq.SeatSelectionA:
                    ScaleBackSeats();
                    _uiManager.PopFromZeroInstructionUI("각자 자리에 서주세요!");
                    InitForSeatSelection();
                    AnimateAllSeats(Objs.Seats_TrackA);
                    
                    break;

                case (int)MainSeq.SeatSelectionB:
             
                    _uiManager.PopFromZeroInstructionUI("각자 자리에 서주세요!");
                    InitForSeatSelection();
                    ScaleBackSeats();
                    AnimateAllSeats(Objs.Seats_TrackA);
                    break;

                
                
                case (int)MainSeq.OnRaceA:
                    break;
                
                case (int)MainSeq.
                    OnRaceB:
                    break;

                case (int)MainSeq.OnFinish:
                    _uiManager.PopFromZeroInstructionUI("차를 전부 주차했어요!");
                    break;
            }
        }
    }

 

    protected override void Init()
    {
        psResourcePath = "Runtime/EA020/Fx_Click";
        subPsResourcePathMap.Add(0, "Runtime/EA020/OnArrival");
        SetSubPsPool(0);
        
        subPsResourcePathMap.Add(1, "Runtime/EA020/OnRun");
        SetSubPsPool(1);
        
        BindObject(typeof(Objs));
        InitCars();
        
        base.Init();

        _defaultSeatColor = GetObject((int)Objs.Seat_AA).transform.GetComponent<MeshRenderer>().material.color;

                          
        GetObject((int)Objs.TrackA_Set).SetActive(true);
        GetObject((int)Objs.TrackB_Set).SetActive(false);
        _uiManager = UIManagerObj.GetComponent<EA020_UIManager>();
        
        for(int i = (int)Objs.Seat_AA; i <= (int)Objs.Excavator; i++)
        {
           // _defaultSizeMap.Add(i, GetObject(i).transform.localScale);
            _sequenceMap.Add(i, DOTween.Sequence());
        }
        
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

    #region Seat Selection --------------------------------------------------------------------------------

    
    private void OnSeatSelection(MainSeq selection)
    {
        
        DOVirtual.DelayedCall(1.5f, () =>
        {
            initialMessage = "각자 표시된 자리에 서주세요!";
            _uiManagerCommonBehaviorController.ShowInitialMessage(initialMessage);
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

                Managers.Sound.Play(SoundManager.Sound.Effect, "EA020/Seat_" + (_seatClickedCount+1).ToString());
                _seatClickedCount++;

                _sequenceMap[_tfIdToEnumMap[hitTransformID]]?.Kill();
                if (_seatClickedCount >= MAX_SEAT_COUNT) isAllSeatClicked = true;
            }

            if (isAllSeatClicked)
            {
                Logger.ContentTestLog("모든 자리가 선택되었습니다--------");
                // Messenger.Default.Publish(new EA012Payload("OnSeatSelectFinished"));
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA018/Narration/OnSeatSelectFinished");
                _uiManager.PopFromZeroInstructionUI("다 앉았구나! 이제 자동차들을 보러가자!");
                DOVirtual.DelayedCall(4, () =>
                {
                    for (int i = (int)Objs.Seat_AA; i < (int)Objs.Seat_AD + 1; i++)
                    {
                        GetObject(i).transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutSine)
                            .OnComplete(() =>
                            {
                               // GetObject(i).SetActive(false);
                            });
                    }
                    
                    for (int i = (int)Objs.Seat_BA; i < (int)Objs.Seat_BD + 1; i++)
                    {
                        int cahceIndex = i;
                        GetObject(i).transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutSine)
                            .OnComplete(() =>
                            {
                               // GetObject(cahceIndex).SetActive(false);
                            });
                    }



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
    
    
    private void AnimateSeatLoop(Objs seat)
    {
        GetObject((int)seat).SetActive(true);
        
        
        var SeatTransform = GetObject((int)seat).transform;

        _sequenceMap[(int)seat]?.Kill();
        _sequenceMap[(int)seat] = DOTween.Sequence();
        _sequenceMap[(int)seat]
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 1.1f, 0.25f))
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 0.9f, 0.35f))
            .SetLoops(120, LoopType.Yoyo)
            .OnKill(() =>
            {
                SeatTransform.DOScale(_defaultSizeMap[(int)seat], 1);
            });

        _sequenceMap[(int)seat].Play();
    }

#endregion ---------------------------------------------------------------------------------

    public override void OnRaySynced()
    {
        if (!PreCheckOnRaySync()) return;

        if (CurrentMainMainSeq == (int)MainSeq.SeatSelectionA || CurrentMainMainSeq == (int)MainSeq.SeatSelectionB)
        {
            OnRaySyncedOnSeatSelection();
        }
        else if (CurrentMainMainSeq == (int)MainSeq.OnRaceA ||CurrentMainMainSeq == (int)MainSeq.OnRaceB)
        {
            MoveCarTowardOnTrack();
        }

    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        
        CurrentMainMainSeq = (int)MainSeq.OnCarInit;
    }

    private bool CheckIfMaxRoundCount()
    {
        if (currentRoundCount > MAX_ROUND_COUNT)
        {
            Logger.ContentTestLog("최대 라운드 수를 초과했습니다. ----------OnFinish");
            CurrentMainMainSeq = (int)MainSeq.OnFinish;
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
    
    #region Track Setting --------------------------------------------------------------------------------
    

        private const int TRACK_COUNT = 4;
    
        private void InitCars()
        {
            for (var i = 0; i < GetObject((int)Objs.NormalizedCar).transform.childCount; i++)
            {
                var car = GetObject((int)Objs.NormalizedCar).transform.GetChild(i).gameObject;
                _cars.Add(i, car);
                Animator Caranimator = car.GetComponent<Animator>();
                _carAnimators.Add(i, Caranimator);
                car.SetActive(false);
                car.transform.localScale = DEFAULT_SIZE;
                tfIDToAnimatorMap.Add( car.gameObject.transform.GetInstanceID(), Caranimator);
            }
        }

        private Sequence _CarIntroduceSequence;
        
        private void InitCarsPerRound()
        {

            _CarIntroduceSequence?.Kill();
            _CarIntroduceSequence = DOTween.Sequence();
            
            currentTrackNameForAnimStateMap = new();
            
            _uiManager.PopFromZeroInstructionUI("차를 도와주세요! 누가 빨리가는지 볼까요?"); 
            selectedCars.Clear();

            List<int> carIndices = new List<int>(_cars.Keys);

            if (carIndices.Count < 4)
            {
                Logger.ContentTestLog("차량이 4개 미만입니다. InitOnRack 실패.");
                return;
            }

            // 키 셔플 후 앞에서 4개 선택
            
            Logger.ContentTestLog($"Car Indice Count : {carIndices.Count}----------");
            System.Random rng = new System.Random();
            carIndices.Sort((a, b) => rng.Next(-1, 2)); // 셔플

            for (int i = 0; i < 4; i++)
            {
                int carIndex = carIndices[i];
                GameObject car = _cars[carIndex];
                Animator animator = _carAnimators[carIndex];
                selectedCars.Add(i, car);
           
                car.SetActive(true);


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
                
           

                animator.SetInteger(TrackNum, animInt); // TrackNum이 string이면 확인 필요
                animator.Play(currentTrackNameForAnimStateMap[selectedCarID], 0, 0);
                
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    animator.enabled = false;
                });

                Logger.ContentTestLog($"Car {carIndex} ID{selectedCarID}-> {(CarAnim)animInt}");

                //Doshake
                //transform.DOShakeScale(1.0f, 1, 10, 90, true).SetEase(Ease.OutQuad);
            }

            
            
            
            for (int i = 0; i < selectedCars.Count; i++)
            {
                _nameInKoreanMap.TryGetValue(selectedCars[0].transform.GetInstanceID(), out string carName);
                _CarIntroduceSequence.AppendCallback(() =>
                {
                    _uiManager.PopFromZeroInstructionUI($"차량 {carName})");
                });

            }
            

           if ((int)currentRoundCount % 2 == 1)
            {
                CurrentMainMainSeq = (int)MainSeq.SeatSelectionA;
            }
            else
            {
                CurrentMainMainSeq = (int)MainSeq.SeatSelectionB;
            }
            
        }

        private void StartTrackRoundWithCount()
        {
            Managers.Sound.Play(SoundManager.Sound.Effect, "EA020/CarStart");

            DOVirtual.DelayedCall(3f, () =>
            {
                _uiManager.PopFromZeroInstructionUI("차를 도와주세요! 누가 빨리가는지 볼까요?");
                DOVirtual.DelayedCall(3f, () =>
                {
                    _uiManager.PopFromZeroInstructionUI("준비!");
                    Managers.Sound.Play(SoundManager.Sound.Effect, "EA020/RaceCount");

                    Managers.Sound.Play(SoundManager.Sound.Narration, "EA020/Ready");

                    DOVirtual.DelayedCall(3f, () =>
                    {
                        isRoundActive = true;
                        _uiManager.PopFromZeroInstructionUI("시작!", 1f);
                        DOVirtual.DelayedCall(0.7f, () =>
                        {
                            _uiManager.ClosePopupUI();
                        });

                        Managers.Sound.Play(SoundManager.Sound.Narration, "EA020/Start");
                    });
                });
            });
        }
        private readonly Dictionary<int, int> _partProgress = new(); // 각 파트의 클릭 진행 상태
        private readonly Dictionary<int, bool> isArrivedMap = new();
        private readonly Dictionary<int, Animator> tfIDToAnimatorMap = new();
        private bool isRoundActive = false;
        private const int COUNT_TO_ARRIVE = 20;
        private int currentArrivedCarCount = 0; 
        private Vector3 _particlePosOffset = new Vector3(0, 0, -1.5f);
     
        private int _winnerIndex = -1;
        private bool _isWinnderIndexRegisted = false;
        
        private void MoveCarTowardOnTrack()
        {
         if (!isRoundActive) return;


        foreach (var hit in GameManager_Hits)
        {
            int ID = hit.transform.GetInstanceID();


            _isClickableMap.TryAdd(ID, true);
            if (_isClickableMap[ID] == false) continue;
            _isClickableMap[ID] = false;
            
            

            // 현재 진행도 저장 및 증가
            isArrivedMap.TryAdd(ID, false);
            if (isArrivedMap[ID]) return;

            if (tfIDToAnimatorMap.TryGetValue(ID, out var animator))
            {
               
                var tf = animator.transform;
                // OnCompletion 클립 가져오기
                _defaultSizeMap.TryAdd(ID, tf.localScale);
                _defaultRotationQuatMap.TryAdd(ID, tf.localRotation);


                var clip = animator.runtimeAnimatorController.animationClips
                    .FirstOrDefault(c => c.name.Contains(currentTrackNameForAnimStateMap[ID]));
                
                Logger.Log($"ID: {currentTrackNameForAnimStateMap[ID]}");

                if (clip != null)
                {
                    _partProgress.TryAdd(ID, 0);
                    _partProgress[ID] = Mathf.Min(_partProgress[ID] + 1, COUNT_TO_ARRIVE);
                    

                    float progressNormalized = _partProgress[ID] / (float)COUNT_TO_ARRIVE;

                    //도착시 로직 
                    if (!isArrivedMap[ID] && _partProgress[ID] >= COUNT_TO_ARRIVE)
                    {
                        isArrivedMap[ID] = true;
                        
                        char randoChar = (char)Random.Range('A', 'B' + 1);
                        Managers.Sound.Play(SoundManager.Sound.Effect, "EA018/OnPartArrive_" + randoChar);
                  
                       
                        currentArrivedCarCount++;

                        _sequenceMap[ID]?.Kill();
                        _sequenceMap[ID] = DOTween.Sequence();
           
                        _sequenceMap[ID].Append(tf.transform.DOScale(DEFAULT_SIZE * 1.2f, 0.25f)
                            .SetEase(Ease.OutBounce));
                      //  _sequenceMap[ID].Join(tf.transform.DOLocalRotate(Vector3.zero, 0.35f));
                        _sequenceMap[ID].Append(tf.transform.DOScale(DEFAULT_SIZE * 0.8f, 0.2f)
                            .SetEase(Ease.OutBounce));
                        _sequenceMap[ID].Append(tf.transform.DOScale(DEFAULT_SIZE * 1.2f, 0.25f)
                            .SetEase(Ease.OutBounce));
                        _sequenceMap[ID].Append(tf.transform.DOScale(DEFAULT_SIZE, 0.5f));
                       
                        _sequenceMap[ID].AppendCallback(() =>
                        {
                            _isClickableMap[ID] = false;
                            animator.enabled = true;
                            animator.SetInteger(TrackNum, _currentRankingOrder + 10);
                            _currentRankingOrder++;
                        });

                        if (!_isWinnderIndexRegisted)
                        {
                            _winnerIndex = transIDToIndex[ID];
                            _isWinnderIndexRegisted = true;
                        }
                  
                   
                        
                        Logger.ContentTestLog($"sub part Arrived {currentArrivedCarCount}");
                        PlaySubParticleEffect(0,hit.transform.position);
                      
                       
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
                        _sequenceMap.TryAdd(ID, DOTween.Sequence());
                        _sequenceMap[ID]?.Kill();
                        _sequenceMap[ID] = DOTween.Sequence();
                        _sequenceMap[ID].Append(tf.DOScale(DEFAULT_SIZE* 1.45f, 0.15f).SetEase(Ease.OutBounce));
      
                        _sequenceMap[ID].Append(tf.DOScale(DEFAULT_SIZE, 0.06f).SetEase(Ease.OutBounce));
                        _sequenceMap[ID].AppendCallback(() =>
                        {
                            animator.enabled = true;
                     //       Managers.Sound.Play(SoundManager.Sound.Effect, "EA020/CarMove",0.1f);
                            animator.Play(currentTrackNameForAnimStateMap[ID], 0, progressNormalized);
                        });

                        float time = 0;
                        _sequenceMap[ID].AppendCallback(() =>
                        {
                            DOVirtual.DelayedCall(0.1f, () =>
                            {
                                _isClickableMap[ID] = true;
                                animator.enabled = false;
                            }).OnUpdate(() =>
                            {
                                var pos = hit.transform.position +_particlePosOffset;
                                DOVirtual.DelayedCall(0.05f, () =>
                                {
                                    time += Time.deltaTime;
                                    if (time > 0.05f)
                                    {
                                        time = 0;
                                        PlaySubParticleEffect(1,pos);
                                    }
                                });
                            });
                        });
                        
                      

                        // 재생 잠깐 유지 후 정지
                        DOVirtual.DelayedCall(0.1f, () =>
                        {
                          //  animator.enabled = false;
                            
                            // int colorSeqID = ID + 1234;
                            // _sequenceMap.TryAdd(colorSeqID, DOTween.Sequence());
                            // _sequenceMap[colorSeqID]?.Kill();
                            // _sequenceMap[colorSeqID] = DOTween.Sequence();
                            
                            // int effectType = Random.Range(0, 3); // 0~2 랜덤
                            //
                            // switch (effectType)
                            // {
                            //     case 0: // Shake
                            //         _sequenceMap[ID]
                            //             .Append(tf.DOShakePosition(0.5f, new Vector3(Random.Range(0.1f,0.5f), Random.Range(0.1f,0.5f), Random.Range(0.1f,0.5f))).OnKill(() =>
                            //             {
                            //                 tf.localRotation = _defaultRotationQuatMap[ID];
                            //             }));
                            //         break;
                            //     case 1: // Scale Up & Down
                            //         _sequenceMap[ID].Append(tf.DOScale(_defaultSizeMap[ID] * 1.2f, 0.30f)
                            //                 .SetLoops(2, LoopType.Yoyo))
                            //             .OnKill(() =>
                            //             {
                            //                 tf.localRotation = _defaultRotationQuatMap[ID];
                            //             });
                            //         break;
                            //     case 2: // Rotate
                            //         float angle = Random.Range(-20f, 20f);
                            //
                            //         _sequenceMap[ID].Append(tf
                            //             .DORotateQuaternion(_defaultRotationQuatMap[ID]* new Quaternion(0, 0, Random.Range(-15,15), 0), 0.25f)
                            //             .SetLoops(2, LoopType.Yoyo)
                            //             .SetEase(Ease.InOutSine));
                            //         break;
                            // }
                            
                        });
                    }
                }
                else
                    Logger.LogWarning("Clip is null");
            }
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
    

    private void Update()
    {
        if (isRoundActive)
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed >= TIME_LIMIT)
            {
                isRoundActive = false;
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA020/Stop");
                _uiManager.PopFromZeroInstructionUI("그만!");
                OnTrackRoundFinished();
            }
            int remainTime = TIME_LIMIT - (int)_timeElapsed;

            if (remainTime <= 10 && remainTime != _lastNarratedSecond)
            {
                _lastNarratedSecond = remainTime;
                Managers.Sound.Play(SoundManager.Sound.Narration, "EA020/Narration/Count_" + remainTime);
                _uiManager.PopFromZeroInstructionUI($"{remainTime}");
            }
        }
    }

    private void OnTrackRoundFinished()
    {
        DOVirtual.DelayedCall(3f, () =>
        {

            if (_winnerIndex != -1)
            {
                _uiManager.PopFromZeroInstructionUI($"{_winnerIndex + 1}번 친구가 가장 먼저 들어왔어!");
            }
            else
            {
                _uiManager.PopFromZeroInstructionUI("열심히 터치해서 차를 주차해요!"); 
            }
            Logger.ContentTestLog("");
            DOVirtual.DelayedCall(3f, () =>
            {  
                InitForNewRoundTrack();
                DOVirtual.DelayedCall(2f, () =>
                {
                    if (CurrentMainMainSeq == (int)MainSeq.OnRaceA)
                    {
                        CurrentMainMainSeq = (int)MainSeq.SeatSelectionB;
                    }
                    else if (CurrentMainMainSeq == (int)MainSeq.OnRaceB)
                    {
                        CurrentMainMainSeq = (int)MainSeq.SeatSelectionA;
                    }
                   
                });
          
            });
        });
        
        }

    #endregion


}
