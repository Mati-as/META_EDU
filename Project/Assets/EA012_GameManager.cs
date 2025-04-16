using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;
using Random = UnityEngine.Random;

public class EA012_GameManager : Ex_BaseGameManager
{

    public enum GameObj
    {
        Seats, // 자리 전체 참조용
        Cars, // Vehicle 전체 참조용
        Wheels, // Wheel 전체 참조용
        Seat_A,
        Seat_B,
        Seat_C,
        Seat_D,
        Seat_E,
        Seat_F,
        Seat_G,
        
        Car_Ambulance,
        Car_PoliceCar,
        Car_FireTruck,
        Car_Bus,
        Car_Taxi,
        
        Building_Ambulance,
        Building_PoliceCar,
        Building_FireTruck,
        Building_Taxi,
        Building_Bus,
        PosGroup,
        PosA,
        PosB,
        PosC,
        PosD,
        PosE,
        PosF,
        PosG,
        PosH,
        PosI,
        PosJ,
        PosK,
        PosL,
        PosM,
        OutA,
        OutB,
        OutC,
        OutPosGroup,
        HelpMoveBtns,
        BtnA,
        BtnB,
        BtnC,
        BtnD,
        BtnE,
        BtnF,
        BtnG,
      
    }

    public enum MainSeq
    {
        Default=-1,
        SeatSelection,
        SeqB_Ambulance,
        SeqB_Ambulance_Move,
        SeqC_PoliceCar,
        SeqC_PoliceCar_Move,
        SeqD_FireTruck,
        SeqD_FireTruck_Move,
        SeqE_Taxi,
        SeqE_Taxi_Move,
        SeqF_Bus,
        SeqF_Bus_Move,

        CarMoveHelpFinished = 100,
        Finished = -123
    }

    public enum CarAnimSeq
    {
        Ambulance_Appear=1,
        Ambulance_Leave,
        PoliceCar_Appear,
        PoliceCar_Leave,
        FireTruck_Appear,
        FireTruck_Leave,
        Taxi_Appear,
        Taxi_Leave,
        Bus_Appear,   
        Bus_Leave,
    }

    public enum TireNum
    {
        Ambulance,
        PoliceCar,
        FireTruck,
        Taxi,
        Bus,   
    }

    private bool _isClickableByAnimationOrNarration = true;//나레이션 재생 중 혹은 여러 기타요인으로 클릭을 막을경우 총괄
    public bool isClickableByAnimationOrNarration
    {
        get
        {
            return _isClickableByAnimationOrNarration;
        }
        set
        {
            _isClickableByAnimationOrNarration = value;
        }
    }//나레이션 재생 중 혹은 여러 기타요인으로 클릭을 막을경우 총괄
    private Animator _carAnimator;
    private int MainAnimFinished = -123;
    private int CAR_ANIM_ON = Animator.StringToHash("CarAnimOn");
    private int CAR_ANIM_OFF = Animator.StringToHash("CarAnimOff");
    [SerializeField]
    [Range(0,20)]
    private float TirePathDurationMin = 4f;
    [SerializeField]
    [Range(0,20)]
    private float TirePathDurationMax = 8f;
    
    private const int SEAT_COUNT = 7;
    private const int CAR_COUNT = 5;
    [SerializeField]
    [Range(0,20)]
    private int WHEEL_COUNT_TO_REMOVE = 20;
    private int currentRemovedTireCount;
    private int None_CAR_ANIM = 100;
    private int CAR_ANIM_DEFAULT = 0;

    private int _currentSelectedSeatCount;
    private int _currentCarSeqCount;
    private Dictionary<int, Collider> _helpMoveBtnColliderMap = new();
    
  //  private Dictionary<int,Material> _materialMap = new();
    private Dictionary<int,bool> isSeatClickedMap = new();
    private Dictionary<int, Dictionary<int,Transform>> tireGroupMap = new();
    private Dictionary<int, Dictionary<int,Sequence>> tireSeqMap = new();

    

    private void ActivateBuilding(int buldingindex)
    {
        DOVirtual.DelayedCall(1f,() =>
        {
            GetObject((int)GameObj.Building_Ambulance).SetActive(false);
            GetObject((int)GameObj.Building_PoliceCar).SetActive(false);
            GetObject((int)GameObj.Building_FireTruck).SetActive(false);
            GetObject((int)GameObj.Building_Bus).SetActive(false);
            GetObject((int)GameObj.Building_Taxi).SetActive(false);
            
            GetObject(buldingindex).SetActive(true);
            GetObject(buldingindex).transform.DOScale(0, 0.01f);
            GetObject(buldingindex).transform.DOScale(_defaultSizeMap[buldingindex], 1f);
        });

        
    }

    #region Main Seq (카메라 조작)---------------------------

    

   
    public  MainSeq currentMainSeq
    {
        get => _currentMainSeq;
        set
        {
           
            _currentMainSeq = value;
            
            Messenger.Default.Publish(new EA012Payload(_currentMainSeq.ToString()));
            
            Logger.ContentTestLog($"Current Sequence: {currentMainSeq.ToString()}");



            // commin Init Part.
          
            
            switch (value)
            {
                case MainSeq.Default:
                    break;
                case MainSeq.Finished:
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeatSelection:
                    AnimateAllSeats();
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqB_Ambulance:
                    StartRollingTire((int)TireNum.Ambulance);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqC_PoliceCar:
                    StartRollingTire((int)TireNum.PoliceCar);
                    ActivateBuilding((int)GameObj.Building_PoliceCar);
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqD_FireTruck:
                    StartRollingTire((int)TireNum.FireTruck);
                    ActivateBuilding((int)GameObj.Building_FireTruck);
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqE_Taxi:
                    StartRollingTire((int)TireNum.Taxi);
                    ActivateBuilding((int)GameObj.Building_Taxi);
                    ChangeThemeSeqAnim((int)value);
                    break;
                case MainSeq.SeqF_Bus:
                    StartRollingTire((int)TireNum.Bus);
                    ActivateBuilding((int)GameObj.Building_Bus);
                    ChangeThemeSeqAnim((int)value);
                    break;

                case MainSeq.CarMoveHelpFinished:
                    ChangeThemeSeqAnim((int)value);
                    break;
                
                
                case MainSeq.SeqB_Ambulance_Move:
                
                    _isCarMoveFinished = false;
                    _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.Ambulance_Appear;
                    // SetHelpCarMoveBtnStatus();
                    //  AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqC_PoliceCar_Move:
                    
                    _isCarMoveFinished = false;
                    _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.PoliceCar_Appear;
                    //                  SetHelpCarMoveBtnStatus();
//                    AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqD_FireTruck_Move:
                 
                    _isCarMoveFinished = false;
                     _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.FireTruck_Appear;

                    break;
                case MainSeq.SeqE_Taxi_Move:
                   
                    _isCarMoveFinished = false;
                     _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.Taxi_Appear;
                    //                  SetHelpCarMoveBtnStatus();
//                    AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqF_Bus_Move:
                   
                    _isCarMoveFinished = false;
                     _carAnimator.enabled = false;
                    currentCarAnimSeq = CarAnimSeq.Bus_Appear;
                    //                  SetHelpCarMoveBtnStatus();
             //      AnimateAllCarMoveHelpBtns();
                    break;
            }
        }
    }
    
    #endregion


    private Tween clickableTween;
    public void SetClickableWithDelayOfNar(float delayAmount)
    {

        isClickableByAnimationOrNarration = false;
        clickableTween?.Kill();
        
        clickableTween = DOVirtual.DelayedCall(delayAmount, () =>
        {
            Logger.ContentTestLog("[HelpBtn] 클릭 가능 상태입니다.");
            isClickableByAnimationOrNarration = true;
        });
        clickableTween.OnKill(() =>
        {
            Logger.ContentTestLog("click delay Killed..false again");
        });
         
        Logger.ContentTestLog($" {Managers.Sound.audioSources[(int)SoundManager.Sound.Narration].clip.name} : 클릭불가 상태 지속시간 : {delayAmount}");
    }

    private void OnTireSelectionFinished()
    {
        _isCarMoveFinished = false;
        SetAllHelpCarMoveBtnStatus();
        AnimateAllCarMoveHelpBtns();
   
        
    }
    private MainSeq _currentMainSeq = MainSeq.Default;

    protected override void Init()
    {
        psResourcePath = "Runtime/SortedByScene/EA012/Fx_Click";
        DOTween.SetTweensCapacity(500,1000);
        base.Init();
        BindObject(typeof(GameObj));
        Messenger.Default.Publish(new EA012Payload(nameof(MainSeq.Default)));
        
        for (int i = (int)GameObj.Seat_A; i < (int)GameObj.Seat_A + SEAT_COUNT; i++)
        {
            isSeatClickedMap.Add(i, false);
        }

        _carAnimator = GetObject((int)GameObj.Cars).GetComponent<Animator>();
        SetTireGroupDictionary();
        for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
        {
            Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
            GetObject(i).transform.localScale = Vector3.zero;
            _helpMoveBtnColliderMap.Add(i,GetObject(i).GetComponentInChildren<Collider>());
        }
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        currentMainSeq = MainSeq.SeatSelection;
    }

    public override void OnRaySynced()
    {
        if (currentMainSeq == MainSeq.SeatSelection)
        {
            OnRaySyncedOnSeatSelection();
        }
        else if (currentMainSeq == MainSeq.SeqB_Ambulance)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Ambulance);
        }
        else if (currentMainSeq == MainSeq.SeqC_PoliceCar)
        {
            OnRaySyncedOnTireSelection((int)TireNum.PoliceCar);
        }
        else if (currentMainSeq == MainSeq.SeqD_FireTruck)
        {
            OnRaySyncedOnTireSelection((int)TireNum.FireTruck);
        }
        else if (currentMainSeq == MainSeq.SeqE_Taxi)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Taxi);
        }
        else if (currentMainSeq == MainSeq.SeqF_Bus)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Bus);
        }
        
        
        
        else if (currentMainSeq == MainSeq.SeqB_Ambulance_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_Ambulance);
        }
        else if (currentMainSeq == MainSeq.SeqC_PoliceCar_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_PoliceCar);
        }
        else if (currentMainSeq == MainSeq.SeqD_FireTruck_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_FireTruck);
        }
        else if (currentMainSeq == MainSeq.SeqE_Taxi_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_Taxi);
        }
        else if (currentMainSeq == MainSeq.SeqF_Bus_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_Bus);
        }
    }

    #region Seat Selection Part
    
    int TIRE_GROUPCOUNT = 5;
    private static readonly int CAR_NUM = Animator.StringToHash("CarNum");

    private void SetTireGroupDictionary()
    {
        char tireGroupName = 'A'; //ABCD순으로 증가하여 폴더 prefab 순회
        
        for (int i = 0; i < TIRE_GROUPCOUNT; i++)
        {
            var newDict = new Dictionary<int, Transform>();
            var newSeqMap = new Dictionary<int, Sequence>();
            
            tireGroupMap.Add(i, newDict);
            tireSeqMap.Add(i, newSeqMap);
            var prefab = Resources.Load<GameObject>("Runtime/SortedByScene/EA012/Wheel_" + tireGroupName);

          
            for (int tire = 0; tire < WHEEL_COUNT_TO_REMOVE; tire++)
            {
                var tirePrefab = Instantiate(prefab).transform;
                
                _isClickedMap.Add(tirePrefab.transform.GetInstanceID(), false);
                tireGroupMap[i].Add(tire, tirePrefab);
                tireSeqMap[i].Add(tirePrefab.gameObject.transform.GetInstanceID(),DOTween.Sequence());
            }

            tireGroupName++;
        }
    }
    
    private void StartRollingTire(int currentTireGroup)
    {
        if (!tireSeqMap.ContainsKey(currentTireGroup))
            tireSeqMap[currentTireGroup] = new Dictionary<int, Sequence>();

        _isTireRemovalFinished = false;
        Managers.Sound.Play(SoundManager.Sound.Effect,"EA012/TireRoll");
        for (int i = 0; i < WHEEL_COUNT_TO_REMOVE; i++)
        {
            var tire = tireGroupMap[currentTireGroup][i];
            var tireID = tire.GetInstanceID();
            
            // 기존 시퀀스가 있다면 정리
            if (tireSeqMap[currentTireGroup].ContainsKey(tireID))
            {
                tireSeqMap[currentTireGroup][tireID]?.Kill();
                tireSeqMap[currentTireGroup].Remove(tireID);
            }

            Sequence seq = DOTween.Sequence();
            tireSeqMap[currentTireGroup][tireID] = seq;
            
            // 위치 초기화
            int startIndex = Random.Range((int)GameObj.OutA, (int)GameObj.OutC + 1);
            Transform startTransform = GetObject(startIndex).transform;
            tire.position = startTransform.position;
            tire.rotation = startTransform.rotation;

            int destinationIndex = Random.Range((int)GameObj.PosA, (int)GameObj.PosM + 1);
            Transform destinationTransform = GetObject(destinationIndex).transform;

            List<Vector3> loopPoints = new() { destinationTransform.position };
            if (destinationIndex > (int)GameObj.PosA)
                loopPoints.Add(GetObject(destinationIndex - 1).transform.position);
            if (destinationIndex < (int)GameObj.PosM)
                loopPoints.Add(GetObject(destinationIndex + 1).transform.position);
            loopPoints = loopPoints.OrderBy(_ => Random.value).ToList();

            // 시퀀스 생성

            seq.Append(tire.DOMove(destinationTransform.position, Random.Range(1.2f,1.8f)).SetEase(Ease.InOutSine));
            seq.Append(tire.DOPath(loopPoints.ToArray(), Random.Range(TirePathDurationMin,TirePathDurationMax), PathType.CatmullRom)
                .SetDelay(Random.Range(0.2f, 0.5f))
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.05f)
                .SetLoops(Int32.MaxValue, LoopType.Yoyo));

            tire.DOLocalRotate(new Vector3(0, 0, 360f), Random.Range(1.2f,2f), RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(Int32.MaxValue, LoopType.Incremental);

            // 시퀀스 저장
            
        }
    }

    #endregion

    private int _seatClickedCount =1;

    private void OnRaySyncedOnSeatSelection()
    {
        bool isAllSeatClicked = true;
        foreach (var hit in GameManager_Hits)
        {
            int hitTransformID = hit.transform.GetInstanceID();
            if (hit.transform.gameObject.name.Contains("Seat"))
            {
                if (isSeatClickedMap[_tfIdToEnumMap[hitTransformID]]) return;
                isSeatClickedMap[_tfIdToEnumMap[hitTransformID]] = true;
                _seatClickedCount++;


                Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Seat_" + _seatClickedCount);


                _sequenceMap[_tfIdToEnumMap[hitTransformID]]?.Kill();

                foreach (int key in isSeatClickedMap.Keys)
                    if (!isSeatClickedMap[key])
                        isAllSeatClicked = false;

                if (isAllSeatClicked)
                {
                    Logger.ContentTestLog("모든 자리가 선택되었습니다--------");
                    
                    Messenger.Default.Publish(new EA012Payload("OnSeatSelectFinished"));

                    DOVirtual.DelayedCall(4, () =>
                    {
                        currentMainSeq = MainSeq.SeqB_Ambulance;
                    });
                    break;
                }


                PlayParticleEffect(hit.transform.position);
            }
        }
    }
    
    
    private bool _isTireRemovalFinished;
    #region 타이어 등장파트

    private void OnRaySyncedOnTireSelection(int currentTireGroup)
    {
        //Logger.ContentTestLog($"{(TireNum)currentTireGroup} : 현재 타이어 그룹 ");
        foreach (var hit in GameManager_Hits)
        {
            
            if (!hit.transform.gameObject.name.Contains("Wheel")) continue;

            PlayParticleEffect(hit.transform.position);
            Logger.ContentTestLog($"Tire clicked: {hit.transform.name}");

            var clickedTire = hit.transform;
            int clickedTransformID = hit.transform.GetInstanceID();

            foreach (var _ in tireGroupMap[currentTireGroup])
            {
                if(_isClickedMap.ContainsKey(clickedTransformID) && _isClickedMap[clickedTransformID])continue;
            
                //중복실행방지
                if (!_isClickedMap[clickedTransformID])
                {
                    _isClickedMap[clickedTransformID] = true;
                    currentRemovedTireCount++;
                }
                
                
                if (tireSeqMap[currentTireGroup].ContainsKey(clickedTransformID))
                {
                    tireSeqMap[currentTireGroup][clickedTransformID]?.Kill();
                    tireSeqMap[currentTireGroup].Remove(clickedTransformID);
                }
                  
                else
                    Logger.ContentTestLog($"tire관련 key 없음 현재 키 :{currentTireGroup} 클릭한 키 : {clickedTransformID}");


                char randomChar = (char)Random.Range('A', 'D' + 1);
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Click" + randomChar.ToString());

                // 사라지는 애니메이션
                
                clickedTire.DOScale(Vector3.zero, 0.35f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        clickedTire.gameObject.SetActive(false);
                    });

//                Logger.ContentTestLog($"타이어 제거됨: 그룹 {(TireNum)currentTireGroup}, 인덱스 {clickedTransformID}");
                return; // 하나만 처리하고 종료
                // }
            }
            
        }

        if (!_isTireRemovalFinished && currentRemovedTireCount >= WHEEL_COUNT_TO_REMOVE)
        {
            _isTireRemovalFinished = true;
            currentRemovedTireCount = 0;
           // CurrentSeq = Seq.WheelSelectFinished;
           //group*2는단순 수적관계임 주의 ---------------------------------
            Logger.ContentTestLog($"모든 타이어 제거됨--------------------Caranim:{(currentTireGroup*2)+1}{(CarAnimSeq)(currentTireGroup*2)+1}------");
            Messenger.Default.Publish(new EA012Payload("AllTiresRemoved"));
            _carAnimator.SetInteger(CAR_NUM ,currentTireGroup*2 + 1);
            Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/OnCarAppear");
            
            DOVirtual.DelayedCall(3.5f, () =>
            {
                currentMainSeq++;
                Logger.ContentTestLog($"current seq -> {currentMainSeq} -------------------");
                OnTireSelectionFinished();
               
            });
        }
        else
        {
            Logger.ContentTestLog($"남은타이어 개수 {currentRemovedTireCount}/ {WHEEL_COUNT_TO_REMOVE}");
        }
    }

    #endregion
    
    private void AnimateAllSeats()
    {
        for (int i = (int)GameObj.Seat_A; i <= (int)GameObj.Seat_G; i++)
        {
            Logger.ContentTestLog($"AnimateAllSeats :Animating seat {(GameObj)i}");
            AnimateSeatLoop((GameObj)i);
        }
    }

    private void SetAllHelpCarMoveBtnStatus(bool isActive=true)
    {
        
        // helpbtnmove 클릭시 초기화 로직 한번에 진행 ----------------------------------------------
        if (isActive)
        {
            foreach (var key in _helpMoveBtnColliderMap.Keys.ToArray())
            {
                _helpMoveBtnColliderMap[key].enabled =false;
            }
            
            for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
            {

                if (i == (int)GameObj.BtnA)
                {
                    _helpMoveBtnColliderMap[(int)GameObj.BtnA].enabled =true;
                    AnimateSeatLoopSelectively(GameObj.BtnA);
                }
                
                Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
                GetObject(i).transform.DOScale(_defaultSizeMap[i], 0.25f);
            }
        }
        else
        {
            for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
            {
                
//                Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
                var i1 = i;
                GetObject(i).transform.DOScale(0, 0.25f).OnComplete(() =>
                {
                    GetObject(i1).SetActive(false);
                });
            }     
         
        }
           
        
    }
    
    private void KillHelpBtns()
    {
        for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
        {
            Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
            
            _sequenceMap[(int)i]?.Kill();
            _sequenceMap[(int)i] = DOTween.Sequence();
        }
        SetAllHelpCarMoveBtnStatus(false);
    }
    private void AnimateAllCarMoveHelpBtns()
    {
        //차가 버튼으로움직일땐 비활성화 -----------------

       
        //GetObject((int)GameObj.Car_Ambulance).
        for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
        {
            GetObject((int)i).SetActive(true);
//            Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
            //AnimateSeatLoop((GameObj)i);
        }
    }

    private void AnimateSeatLoopSelectively(GameObj seat)
    {
        if (!_sequenceMap.ContainsKey((int)seat)) return; 
        
        
        var SeatTransform = GetObject((int)seat).transform;
        _sequenceMap[(int)seat]?.Kill();
        _sequenceMap[(int)seat] = DOTween.Sequence();
        _sequenceMap[(int)seat]
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat]*1.1f, 0.25f))
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat]*0.9f, 0.35f))
            .SetLoops(-1,LoopType.Yoyo)
            .OnKill(()=>
            {
                SeatTransform.DOScale(_defaultSizeMap[(int)seat], 1);
            });

        _sequenceMap[(int)seat].Play();
    }
    private void AnimateSeatLoop(GameObj seat)
    {
        var SeatTransform = GetObject((int)seat).transform;
        
        _sequenceMap[(int)seat]?.Kill();
        _sequenceMap[(int)seat] = DOTween.Sequence();
        _sequenceMap[(int)seat]
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat]*1.1f, 0.25f))
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat]*0.9f, 0.35f))
            .SetLoops(-1,LoopType.Yoyo)
            .OnKill(()=>
            {
                SeatTransform.DOScale(_defaultSizeMap[(int)seat], 1);
            });
        
        _sequenceMap[(int)seat].Play();
    }

    private CarAnimSeq currentCarAnimSeq;
   // private MainSeq currentMainSeq;
    private bool _isCarMoveFinished;
    
    #region 자동차 옮겨주기 파트
    
    private void OnRaySyncOnHelpCarMovePart(GameObj currentCar)
    {
        if (!_isClickableByAnimationOrNarration)
        {
            Logger.ContentTestLog("[HelpBtn] 클릭 불가 상태입니다. ----- 대기 필요 ");
            return;
        }
        
        foreach (var hit in GameManager_Hits)
            if (hit.transform.name.Contains("BtnA")) OnHelpBtnClicked(GameObj.BtnA     ,currentCar);
            else if (hit.transform.name.Contains("BtnB")) OnHelpBtnClicked(GameObj.BtnB,currentCar);
            else if (hit.transform.name.Contains("BtnC")) OnHelpBtnClicked(GameObj.BtnC,currentCar);
            else if (hit.transform.name.Contains("BtnD")) OnHelpBtnClicked(GameObj.BtnD,currentCar);
            else if (hit.transform.name.Contains("BtnE")) OnHelpBtnClicked(GameObj.BtnE,currentCar);
            else if (hit.transform.name.Contains("BtnF")) OnHelpBtnClicked(GameObj.BtnF,currentCar);
            else if (!_isCarMoveFinished&&hit.transform.name.Contains("BtnG"))
            {
                
                if(_isCarMoveFinished) return;
                {
                    //중복실행방지
                }
                _isCarMoveFinished = true;
                OnHelpBtnClicked(GameObj.BtnG, currentCar);
                
                Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/OnCarAppear");
               
                DOVirtual.DelayedCall(1.5f,()=>
                {
                    OnHelpMoveFinisehd();


                    currentMainSeq = MainSeq.CarMoveHelpFinished;
                    currentCarAnimSeq++;
                    // SetInteger first? 
                    _carAnimator.enabled = true;
                    _carAnimator.SetInteger(CAR_NUM,(int)currentCarAnimSeq);
                    Logger.ContentTestLog($"{(int)currentCarAnimSeq} : {currentCarAnimSeq} : 현재 차량 애니메이션 시퀀스----------------");
                 
                    
                    switch (currentCarAnimSeq)
                    {
                        case CarAnimSeq.Ambulance_Leave :
                             Messenger.Default.Publish(new EA012Payload("Arrival","구급차"));
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/Ambulance_Arrival");
                            break;
                        
                        case CarAnimSeq.PoliceCar_Leave:
                             Messenger.Default.Publish(new EA012Payload("Arrival","경찰차"));
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/Ambulance_Arrival");
                            break;
                        
                        case CarAnimSeq.FireTruck_Leave:
                             Messenger.Default.Publish(new EA012Payload("Arrival","소방차"));
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/Ambulance_Arrival");
                             break;
                      
                        case CarAnimSeq.Taxi_Leave:
                             Messenger.Default.Publish(new EA012Payload("Arrival","택시"));
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/Ambulance_Arrival");
                             break;
                        
                        case CarAnimSeq.Bus_Leave:
                             Messenger.Default.Publish(new EA012Payload("Arrival","버스"));
                             Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/Ambulance_Arrival");
                             break;
                    }

                });
               

                // 차량 옮겨주는 애니메이션 종료 후 다음 시퀀스로 이동 및 초기화 진행 ----------------------------------------------
                DOVirtual.DelayedCall(8f, () =>
                {
                      
                   
                        
                    switch (currentCarAnimSeq)
                    {
                        case CarAnimSeq.Ambulance_Leave :
                            currentMainSeq = MainSeq.SeqC_PoliceCar;
                            break;
                        
                        case CarAnimSeq.PoliceCar_Leave:
                            _isCarMoveFinished = false;
                            currentMainSeq = MainSeq.SeqD_FireTruck;
                            break;
                        
                        case CarAnimSeq.FireTruck_Leave:
                            currentMainSeq = MainSeq.SeqE_Taxi;
                            break;
                      
                        case CarAnimSeq.Taxi_Leave:
                            _isCarMoveFinished = false;
                            currentMainSeq = MainSeq.SeqF_Bus;
                            break;
                        
                        case CarAnimSeq.Bus_Leave:
                            _isCarMoveFinished = false;
                            currentMainSeq = MainSeq.Finished;
                            break;
                    }
              
                    _carAnimator.SetInteger(CAR_NUM, CAR_ANIM_DEFAULT);
                    Logger.ContentTestLog($"car anim seq {currentCarAnimSeq} 다음 로직으로 이동 ----------currentMainSeq : {currentMainSeq}");
         
                });
            }
    }

private void OnHelpMoveFinisehd()
{
    KillHelpBtns();
 //   currentMainSeq++;
}


private void OnHelpBtnClicked(GameObj clickedBtn,GameObj currentCar)
    {
        // BtnG일 경우 종료
        if (clickedBtn > GameObj.BtnG)
        {
          
           // Logger.ContentTestLog("[HelpBtn] BtnG 도달 → CarAnim Seq 2 설정됨");
          
           
            return;
        }
        
        //C인경우 재생안되도록 설계됨. NextHelpMOveBtnC 파일 넣지 않은상태가 정상동작
        char randomCharAB = (char)Random.Range('A', 'F' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Narration/NextHelpMoveBtn" + randomCharAB);
        
        char randomChar = (char)Random.Range('A', 'D' + 1);
        Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Click" + randomChar.ToString());

        Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/CarMove");
        
        _sequenceMap[(int)clickedBtn]?.Kill();
        _sequenceMap[(int)clickedBtn] = DOTween.Sequence();
        AnimateSeatLoopSelectively(clickedBtn+1);
        
        
   //     _carAnimator.SetTrigger(CAR_ANIM_OFF);
        if(_helpMoveBtnColliderMap.ContainsKey((int)clickedBtn))_helpMoveBtnColliderMap[(int)clickedBtn].enabled = false;
        if(_helpMoveBtnColliderMap.ContainsKey((int)clickedBtn+1))_helpMoveBtnColliderMap[(int)clickedBtn + 1].enabled = true;

        int currentIdx = (int)clickedBtn-1;
        int nextIdx = currentIdx + 1;


        var car = GetObject((int)currentCar).transform;
        var start = clickedBtn == GameObj.BtnA
            ? GetObject((int)currentCar).transform.position
            : GetObject(currentIdx).transform.position;
        var end = clickedBtn == GameObj.BtnA
            ? GetObject((int)GameObj.BtnA).transform.position
            : GetObject(nextIdx).transform.position;
        var mid = Vector3.Lerp(start, end, 0.5f) + Vector3.up * 0.5f;

        Vector3[] path = { start, mid, end };

        car.DOPath(path, 1.5f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .SetLookAt(0.1f, Vector3.forward)
            .OnComplete(() =>
            {
                Logger.ContentTestLog($"[HelpBtn] 차량 이동 완료: {clickedBtn} → {(GameObj)nextIdx}");

            });
    }
    #endregion
}




public interface IPayload{};
public class EA012Payload : IPayload
{
    public string Narration { get; }
    public string CurrentCarName { get; }

    public EA012Payload(string narration,string carname ="car")
    {
        Narration = narration;
        CurrentCarName = carname;
    }
}
