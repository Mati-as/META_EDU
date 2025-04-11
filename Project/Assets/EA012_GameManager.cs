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
        Car,PoliceCar,
        Car_FireTruck,
        Car_Airplane,
        Car_Train,
        Building_Ambulance,
        Buidling_PoliceCar,
        Building_FireTruck,
        Building_Airplane,
        Building_Train,
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
    }

    public enum Seq
    {
        Default,
        SeqA_WheelStopGame,
        SeqB_Ambulance,
        SeqB_Ambulance_Finished,
        SeqC_PoliceCar,
        //SeqC_PoliceCar_Finished,
        SeqD_FireTruck,
      //  SeqD_FireTruck_Finished,
        SeqE_Airplane,
    //SeqE_Airplane_Finished,
        SeqF_Train,
       // SeqF_Train_Finished,
       WheelSelectFinished=100
        
    }

    public enum TireNum
    {
        Ambulance,
        PoliceCar,
        FireTruck,
        Bus,   
        Taxi
    }

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
            GetObject((int)GameObj.Buidling_PoliceCar).SetActive(false);
            GetObject((int)GameObj.Building_FireTruck).SetActive(false);
            GetObject((int)GameObj.Building_Airplane).SetActive(false);
            GetObject((int)GameObj.Building_Train).SetActive(false);
        });
;
        
        GetObject(buldingindex).SetActive(true);
        GetObject(buldingindex).transform.DOScale(0, 0.1f);
        GetObject(buldingindex).transform.DOScale(0, 0.1f);

    }
    public  Seq CurrentSeq
    {
        get => _currentSeq;
        set
        {
            ChangeThemeSeqAnim((int)value);
            _currentSeq = value;
            Messenger.Default.Publish(new EA012Payload(_currentSeq.ToString()));
            Logger.ContentTestLog($"Current Sequence: {CurrentSeq.ToString()}");



            // commin Init Part.
            currentRemovedTireCount = 0;
            
            switch (value)
            {
                case Seq.Default:
                    break;
                case Seq.SeqA_WheelStopGame:
                    AnimateAllSeats();
                    break;
                case Seq.SeqB_Ambulance:
                    RollTire((int)TireNum.Ambulance);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
                case Seq.SeqC_PoliceCar:
                    RollTire((int)TireNum.PoliceCar);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
                case Seq.SeqD_FireTruck:
                    RollTire((int)TireNum.FireTruck);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
                case Seq.SeqE_Airplane:
                    RollTire((int)TireNum.Bus);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
                case Seq.SeqF_Train:
                    RollTire((int)TireNum.Taxi);
                    ActivateBuilding((int)GameObj.Building_Ambulance);
                    break;
            }
        }
    }
    private Seq _currentSeq = Seq.Default;

    protected override void Init()
    {
        psResourcePath = "Runtime/SortedByScene/EA012/Fx_Click";
        base.Init();
        BindObject(typeof(GameObj));
        Messenger.Default.Publish(new EA012Payload(nameof(Seq.Default)));
        
        for (int i = (int)GameObj.Seat_A; i < (int)GameObj.Seat_A + SEAT_COUNT; i++)
        {
            isSeatClickedMap.Add(i, false);
        }

        SetTireGroupDictionary();
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        CurrentSeq = Seq.SeqA_WheelStopGame;
    }

    public override void OnRaySynced()
    {


        if (CurrentSeq == Seq.SeqA_WheelStopGame)
        {
            OnRaySyncedOnSeatSelection();
        }
        else if (CurrentSeq == Seq.SeqB_Ambulance)
        {
            OnRaySyncedOnTireSelection((int)TireNum.Ambulance);
        }
        else if (CurrentSeq == Seq.SeqC_PoliceCar)
        {
            
        }
        else if (CurrentSeq == Seq.SeqD_FireTruck)
        {
          
        }
        else if (CurrentSeq == Seq.SeqE_Airplane)
        {
            
        }
        base.OnRaySynced();
    }

    #region Seat Selection Part
    
    int TIRE_GROUPCOUNT = 5;
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
                    Logger.ContentTestLog("모두클릭됨");
                    CurrentSeq = Seq.SeqB_Ambulance;
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

            Logger.ContentTestLog($"Tire clicked: {hit.transform.name}");

            var clickedTire = hit.transform;
            int clickedTransformID = hit.transform.GetInstanceID();

            foreach (var _ in tireGroupMap[currentTireGroup])
            {
                if(_isClickedMap[clickedTransformID])continue;
                
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

                Logger.ContentTestLog($"타이어 제거됨: 그룹 {(TireNum)currentTireGroup}, 인덱스 {clickedTransformID}");
                return; // 하나만 처리하고 종료
                // }
            }
            
        }

        if (currentRemovedTireCount >= WHEEL_COUNT_TO_REMOVE)
        {
            Logger.ContentTestLog("모든 타이어 제거됨");
            currentRemovedTireCount = 0;
            CurrentSeq = Seq.WheelSelectFinished;
            DOVirtual.DelayedCall(10f, () =>
            {
                CurrentSeq = Seq.SeqC_PoliceCar;
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

    private void AnimateSeatLoop(GameObj seat)
    {
        var SeatTransform = GetObject((int)seat).transform;

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
