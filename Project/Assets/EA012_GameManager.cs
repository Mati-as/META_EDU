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
        SeqC_PoliceCar,
        SeqD_FireTruck,
        SeqE_Airplane,
        SeqF_Train,
    }

    public enum TireNum
    {
        Ambulance,
        PoliceCar,
        FireTruck,
        Bus,   
        Taxi
    }
    
    
    private const int SEAT_COUNT = 7;
    private const int CAR_COUNT = 5;
    private const int totalWheelCount = 20;

    private int _currentSelectedSeatCount;
    private int _currentCarSeqCount;
    
    private Dictionary<int,Material> _materialMap = new();
    private Dictionary<int,bool> isSeatClickedMap = new();
    private Dictionary<int, Dictionary<int,Transform>> tireGroupDictionary = new();
    
    
    
    public  Seq CurrentSeq
    {
        get => _currentSeq;
        set
        {
            ChangeThemeSeqAnim((int)value);
            _currentSeq = value;
            Messenger.Default.Publish(new EA012Payload(_currentSeq.ToString()));
            Logger.ContentTestLog($"Current Sequence: {CurrentSeq.ToString()}");


            switch (value)
            {
                case Seq.Default:
                    break;
                case Seq.SeqA_WheelStopGame:
                    AnimateAllSeats();
                    break;
                case Seq.SeqB_Ambulance:
                    RollTire((int)TireNum.Ambulance);
                    break;
                case Seq.SeqC_PoliceCar:
                    RollTire((int)TireNum.PoliceCar);
                    break;
                case Seq.SeqD_FireTruck:
                    RollTire((int)TireNum.FireTruck);
                    break;
                case Seq.SeqE_Airplane:
                    RollTire((int)TireNum.Bus);
                    break;
                case Seq.SeqF_Train:
                    RollTire((int)TireNum.Taxi);
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
        
        for (int i = (int)GameObj.Seat_A; i < SEAT_COUNT; i++)
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
            CurrentSeq = Seq.SeqC_PoliceCar;
        }
        else if (CurrentSeq == Seq.SeqC_PoliceCar)
        {
            CurrentSeq = Seq.SeqD_FireTruck;
        }
        else if (CurrentSeq == Seq.SeqD_FireTruck)
        {
            CurrentSeq = Seq.SeqE_Airplane;
        }
        else if (CurrentSeq == Seq.SeqE_Airplane)
        {
            CurrentSeq = Seq.SeqF_Train;
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
            tireGroupDictionary.Add(i, newDict);
            var prefab = Resources.Load<GameObject>("Runtime/SortedByScene/EA012/Wheel_" + tireGroupName);

            int TIRE_COUNT = 20;
            for (int tire = 0; tire < TIRE_COUNT; tire++)
            {
                var tirePrefab = Instantiate(prefab).transform;
                tireGroupDictionary[i].Add(tire, tirePrefab);
            }

            tireGroupName++;
        }
    }
    
    private void RollTire(int currentTireGroup)
    {
        for (int i = 0; i < tireGroupDictionary[currentTireGroup].Count; i++)
        {
            var tire = tireGroupDictionary[currentTireGroup][i];

            // 1. 랜덤 시작점 (OutA~OutC)
            int startIndex = Random.Range((int)GameObj.OutA, (int)GameObj.OutC + 1);
            Transform startTransform = GetObject(startIndex).transform;
            tire.position = startTransform.position;
            tire.rotation = startTransform.rotation;

            // 2. 랜덤 도착지점 (PosA~PosI)
            int destinationIndex = Random.Range((int)GameObj.PosA, (int)GameObj.PosI + 1);
            Transform destinationTransform = GetObject(destinationIndex).transform;

            // 3. 도착 후 좌우 loop용 위치 계산
            List<Vector3> loopPoints = new List<Vector3>();
            loopPoints.Add(destinationTransform.position);

            // 좌우 인덱스 체크
            if (destinationIndex > (int)GameObj.PosA)
                loopPoints.Add(GetObject(destinationIndex - 1).transform.position);
            if (destinationIndex < (int)GameObj.PosI)
                loopPoints.Add(GetObject(destinationIndex + 1).transform.position);

            // loop 순서를 좀 섞어주기
            loopPoints = loopPoints.OrderBy(_ => Random.value).ToList();

            // 4. 타이어 이동 경로 설정
            Sequence seq = DOTween.Sequence();
            seq.Append(tire.DOMove(destinationTransform.position, 1f).SetEase(Ease.InOutSine));

            // 5. loop 애니메이션 추가
            seq.Append(tire.DOPath(loopPoints.ToArray(), 2f, PathType.Linear)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo));
        }
    }
    #endregion

    private void OnRaySyncedOnSeatSelection()
    {
        foreach (var hit in GameManager_Hits)
        {
            int hitTransformID = hit.transform.GetInstanceID();
            if (hit.transform.gameObject.name.Contains("Seat"))
            {
                isSeatClickedMap[_tfIdToEnumMap[hitTransformID]] = true;

                _sequenceMap[_tfIdToEnumMap[hitTransformID]]?.Kill();
                
                bool isAllSeatClicked = true;
                foreach (var key in isSeatClickedMap.Keys)
                {
                    if (!isSeatClickedMap[key])
                    {
                        isAllSeatClicked = false;
                    }
                }
                
                if(isAllSeatClicked) 
                {
                    Logger.ContentTestLog("모두클릭됨");
                    break;
                }
             
             
                PlayParticleEffect(hit.transform.position);
                
            }
        }
    }

    #region 타이어 등장파트 
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
