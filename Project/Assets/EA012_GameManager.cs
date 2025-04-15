using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SuperMaxim.Messaging;
using UnityEngine;

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
        
        CarMoveHelpFinished=100
    }

    public enum CarAnimSeq
    {
        Ambulance=1,
        Ambulance_Leave,
        PoliceCar,
        PoliceCar_Leave,
        FireTruck,
        FireTruck_Leave,
        Bus,   
        Bus_Leave, 
        Taxi_Leave
    }

    public enum TireNum
    {
        Ambulance,
        PoliceCar,
        FireTruck,
        Bus,   
        Taxi
    }

    private Animator _carAnimator;
    [SerializeField]
    [Range(0,20)]
    private float TirePathDurationMin = 4f;
    [SerializeField]
    [Range(0,20)]
    private float TirePathDurationMax = 8f;
    
    private const int SEAT_COUNT = 7;
    private const int CAR_COUNT = 5;
    private const int WHEEL_COUNT_TO_REMOVE = 20;
    private int currentRemovedTireCount;

    private int _currentSelectedSeatCount;
    private int _currentCarSeqCount;
    
    private Dictionary<int,Material> _materialMap = new();
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
        });
;
        
        GetObject(buldingindex).SetActive(true);
        GetObject(buldingindex).transform.DOScale(0, 0.1f);
        GetObject(buldingindex).transform.DOScale(0, 0.1f);

    }
    public  MainSeq CurrentMainSeq
    {
        get => _currentMainSeq;
        set
        {
            ChangeThemeSeqAnim((int)value);
            _currentMainSeq = value;
            Messenger.Default.Publish(new EA012Payload(_currentMainSeq.ToString()));
            Logger.ContentTestLog($"Current Sequence: {CurrentMainSeq.ToString()}");



            // commin Init Part.
            currentRemovedTireCount = 0;
            
            switch (value)
            {
                case MainSeq.Default:
                    break;
                case MainSeq.SeatSelection:
                    AnimateAllSeats();
                    break;
                case MainSeq.SeqB_Ambulance:
                    RollTire((int)TireNum.Ambulance);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
                case MainSeq.SeqC_PoliceCar:
                    RollTire((int)TireNum.PoliceCar);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
                case MainSeq.SeqD_FireTruck:
                    RollTire((int)TireNum.FireTruck);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
                case MainSeq.SeqE_Taxi:
                    RollTire((int)TireNum.Bus);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
                case MainSeq.SeqF_Bus:
                    RollTire((int)TireNum.Taxi);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
                
                
                case MainSeq.SeqB_Ambulance_Move:
                    
                    _isCarMoveFinished = false;
                    currentCarAnimSeq = CarAnimSeq.Ambulance;
                    ShowHelpBtns();
                    AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqC_PoliceCar_Move:
                    _isCarMoveFinished = false;
                    currentCarAnimSeq = CarAnimSeq.PoliceCar;
                    ShowHelpBtns();
                    AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqD_FireTruck_Move:
                    _isCarMoveFinished = false;
                    currentCarAnimSeq = CarAnimSeq.FireTruck;
                    ShowHelpBtns();
                    AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqE_Taxi_Move:
                    _isCarMoveFinished = false;
                    currentCarAnimSeq = CarAnimSeq.Taxi_Leave;
                    ShowHelpBtns();
                    AnimateAllCarMoveHelpBtns();
                    break;
                case MainSeq.SeqF_Bus_Move:
                    _isCarMoveFinished = false;
                    currentCarAnimSeq = CarAnimSeq.Bus;
                    ShowHelpBtns();
                    AnimateAllCarMoveHelpBtns();
                    break;
            }
        }
    }
    private MainSeq _currentMainSeq = MainSeq.Default;

    protected override void Init()
    {
        psResourcePath = "Runtime/SortedByScene/EA012/Fx_Click";
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
        }
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        CurrentMainSeq = MainSeq.SeatSelection;
    }

    public override void OnRaySynced()
    {
        if (CurrentMainSeq == MainSeq.SeatSelection)
        {
            OnRaySyncedOnSeatSelection();
        }
        else if (CurrentMainSeq == MainSeq.SeqB_Ambulance)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Ambulance);
        }
        else if (CurrentMainSeq == MainSeq.SeqC_PoliceCar)
        {
            OnRaySyncedOnTireSelection((int)TireNum.PoliceCar);
        }
        else if (CurrentMainSeq == MainSeq.SeqD_FireTruck)
        {
            OnRaySyncedOnTireSelection((int)TireNum.FireTruck);
        }
        else if (CurrentMainSeq == MainSeq.SeqE_Taxi)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Taxi);
        }
        else if (CurrentMainSeq == MainSeq.SeqF_Bus)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Bus);
        }
        
        
        
        else if (CurrentMainSeq == MainSeq.SeqB_Ambulance_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_Ambulance);
        }
        else if (CurrentMainSeq == MainSeq.SeqC_PoliceCar_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_PoliceCar);
        }
        else if (CurrentMainSeq == MainSeq.SeqD_FireTruck_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_FireTruck);
        }
        else if (CurrentMainSeq == MainSeq.SeqE_Taxi_Move)
        {
            OnRaySyncOnHelpCarMovePart(GameObj.Car_Taxi);
        }
        else if (CurrentMainSeq == MainSeq.SeqF_Bus_Move)
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
    
    private void RollTire(int currentTireGroup)
    {
        if (!tireSeqMap.ContainsKey(currentTireGroup))
            tireSeqMap[currentTireGroup] = new Dictionary<int, Sequence>();

        for (int i = 0; i < tireGroupMap[currentTireGroup].Count; i++)
        {
            var tire = tireGroupMap[currentTireGroup][i];

            // 기존 시퀀스가 있다면 정리
            if (tireSeqMap[currentTireGroup].ContainsKey(i))
            {
                tireSeqMap[currentTireGroup][i]?.Kill();
                tireSeqMap[currentTireGroup].Remove(i);
            }

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
            Sequence seq = DOTween.Sequence();
            seq.Append(tire.DOMove(destinationTransform.position, Random.Range(1.2f,1.8f)).SetEase(Ease.InOutSine));
            seq.Append(tire.DOPath(loopPoints.ToArray(), Random.Range(TirePathDurationMin,TirePathDurationMax), PathType.CatmullRom)
                .SetDelay(Random.Range(0.2f, 0.5f))
                .SetEase(Ease.InOutSine)
                .SetLookAt(0.05f)
                .SetLoops(-1, LoopType.Yoyo));

            tire.DOLocalRotate(new Vector3(0, 0, 360f), Random.Range(1.2f,2f), RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);

            // 시퀀스 저장
            tireSeqMap[currentTireGroup][i] = seq;
        }
    }

    #endregion

    private void OnRaySyncedOnSeatSelection()
    { 
        
        bool isAllSeatClicked = true;
        foreach (var hit in GameManager_Hits)
        {
            int hitTransformID = hit.transform.GetInstanceID();
            if (hit.transform.gameObject.name.Contains("Seat"))
            {
                isSeatClickedMap[_tfIdToEnumMap[hitTransformID]] = true;

                _sequenceMap[_tfIdToEnumMap[hitTransformID]]?.Kill();
                
               
                foreach (var key in isSeatClickedMap.Keys)
                {
                    if (!isSeatClickedMap[key]) isAllSeatClicked = false;
                }
                
                if(isAllSeatClicked) 
                {
                    Logger.ContentTestLog("모든 자리가 선택되었습니다--------");
                    CurrentMainSeq = MainSeq.SeqB_Ambulance;
                    break;
                }
             
             
                PlayParticleEffect(hit.transform.position);
                
            }
        }
    }
    
    #region 타이어 등장파트

    private void OnRaySyncedOnTireSelection(int currentTireGroup)
    {
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
                
                if (tireSeqMap[currentTireGroup].ContainsKey(clickedTransformID))
                {
                    tireSeqMap[currentTireGroup][clickedTransformID]?.Kill();
                    tireSeqMap[currentTireGroup].Remove(clickedTransformID);
                }
                  
                else
                    Logger.ContentTestLog($"tire관련 key 없음 현재 키 :{currentTireGroup} 클릭한 키 : {clickedTransformID}");


                //중복실행방지
                if (!_isClickedMap[clickedTransformID])
                {
                    currentRemovedTireCount++;
                    _isClickedMap[clickedTransformID] = true;
                }

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

        if (currentRemovedTireCount >= WHEEL_COUNT_TO_REMOVE)
        {
          
            currentRemovedTireCount = 0;
           // CurrentSeq = Seq.WheelSelectFinished;
           //group*2는단순 수적관계임 주의 ---------------------------------
            _carAnimator.SetInteger(CAR_NUM ,currentTireGroup*2 + 1);
            Logger.ContentTestLog($"모든 타이어 제거됨-----------------------{(CarAnimSeq)(currentTireGroup*2)+1}------");
            DOVirtual.DelayedCall(6f, () =>
            {
                CurrentMainSeq++;
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

    private void ShowHelpBtns()
    {
        for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
        {
            Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
            GetObject(i).transform.DOScale(_defaultSizeMap[i], 0.25f);
        }

        _carAnimator.enabled=false;
    }
    
    private void KillHelpBtns()
    {
        
        for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
        {
            Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
            
            _sequenceMap[(int)i]?.Kill();
            _sequenceMap[(int)i] = DOTween.Sequence();
            GetObject(i).transform.DOScale(_defaultSizeMap[i], 0.25f);
        }

        _carAnimator.enabled=false;
    }
    private void AnimateAllCarMoveHelpBtns()
    {
        for (int i = (int)GameObj.BtnA; i <= (int)GameObj.BtnG; i++)
        {
            Logger.ContentTestLog($"AnimateAllBtns :Animating seat {(GameObj)i}");
            AnimateSeatLoop((GameObj)i);
        }
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
    private MainSeq currentMainSeq;
    private bool _isCarMoveFinished;
    
    #region 자동차 옮겨주기 파트
    
    private void OnRaySyncOnHelpCarMovePart(GameObj currentCar)
    {
        foreach (var hit in GameManager_Hits)
        {
            if (hit.transform.name.Contains("BtnA")) OnHelpBtnClicked(GameObj.BtnA     ,currentCar);
            else if (hit.transform.name.Contains("BtnB")) OnHelpBtnClicked(GameObj.BtnB,currentCar);
            else if (hit.transform.name.Contains("BtnC")) OnHelpBtnClicked(GameObj.BtnC,currentCar);
            else if (hit.transform.name.Contains("BtnD")) OnHelpBtnClicked(GameObj.BtnD,currentCar);
            else if (hit.transform.name.Contains("BtnE")) OnHelpBtnClicked(GameObj.BtnE,currentCar);
            else if (hit.transform.name.Contains("BtnF")) OnHelpBtnClicked(GameObj.BtnF,currentCar);
            else if (!_isCarMoveFinished&&hit.transform.name.Contains("BtnG"))
            {
                _isCarMoveFinished = true;
                OnHelpBtnClicked(GameObj.BtnG,currentCar);
                _carAnimator.enabled = true;
                DOVirtual.DelayedCall(2.5f, () =>
                {
                    CurrentMainSeq = MainSeq.CarMoveHelpFinished;
                    currentCarAnimSeq++;
                    Logger.ContentTestLog($"[HelpBtn] BtnG 도달 → CarAnim Seq {currentCarAnimSeq}설정됨");
                    _carAnimator.SetInteger(CAR_NUM,(int)currentCarAnimSeq);
                    KillHelpBtns();
                    
                    DOVirtual.DelayedCall(5f, () =>
                    {
                      
                        switch (currentCarAnimSeq)
                        {
                            case CarAnimSeq.Ambulance_Leave:
                                CurrentMainSeq = MainSeq.SeqC_PoliceCar;
                                break;
                            case CarAnimSeq.FireTruck_Leave:
                                CurrentMainSeq = MainSeq.SeqD_FireTruck;
                                break;
                            case CarAnimSeq.PoliceCar_Leave:
                                _isCarMoveFinished = false;
                                CurrentMainSeq = MainSeq.SeqE_Taxi;
                                break;
                            case CarAnimSeq.Taxi_Leave:
                                _isCarMoveFinished = false;
                                CurrentMainSeq = MainSeq.SeqF_Bus;
                                break;
                            case CarAnimSeq.Bus_Leave:
                                _isCarMoveFinished = false;
                              //  CurrentMainSeq = Finish-------------------
                                break;
                        }
                        
                        Logger.ContentTestLog($"currentMainSeq : {(MainSeq)CurrentMainSeq}");
                    });
                });
            }
        }
    }
    private void OnHelpBtnClicked(GameObj clickedBtn,GameObj currentCar)
    {
        // BtnG일 경우 종료
        if (clickedBtn >= GameObj.BtnG)
        {
            _carAnimator.SetInteger(CAR_NUM, 2);
            Logger.ContentTestLog("[HelpBtn] BtnG 도달 → CarAnim Seq 2 설정됨");
            return;
        }

        int currentIdx = (int)clickedBtn;
        int nextIdx = currentIdx + 1;

        Transform car = GetObject((int)currentCar).transform;
        Vector3 start = GetObject(currentIdx).transform.position;
        Vector3 end = GetObject(nextIdx).transform.position;
        Vector3 mid = Vector3.Lerp(start, end, 0.5f) + Vector3.up * 0.5f;

        Vector3[] path = { start, mid, end };

        car.DOPath(path, 1.5f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .SetLookAt(0.1f, Vector3.forward)
            .OnComplete(() =>
            {
                Logger.ContentTestLog($"[HelpBtn] 차량 이동 완료: {clickedBtn} → {(GameObj)nextIdx}");

                if ((GameObj)nextIdx == GameObj.BtnG)
                {
                    _carAnimator.SetInteger(CAR_NUM, 2);
                    Logger.ContentTestLog("[HelpBtn] BtnG 도달 → CarAnim Seq 2 설정됨");
                }
            });
    }
    #endregion
}




public interface IPayload{};
public class EA012Payload : IPayload
{
    public string Narration { get; }

    public EA012Payload(string narration)
    {
        Narration = narration;
    }
}
