using System.Collections;
using System.Collections.Generic;
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
    }

    public enum Sequence
    {
        Default,
        SeqA_WheelStopGame,
        SeqB_Ambulance,
        SeqC_PoliceCar,
        SeqD_FireTruck,
        SeqE_Airplane,
        SeqF_Train,
    }
    
    
    private const int SEAT_COUNT = 7;
    private const int CAR_COUNT = 5;
    private const int totalWheelCount = 20;

    private int _currentSelectedSeatCount;
    private int _currentCarSeqCount;
    
    private Dictionary<int,Material> _materialMap = new();
    private Dictionary<int,bool> isSeatClickedMap = new();
    private Dictionary<int,>
    
    
    public  Sequence currentSequence
    {
        get => _currentSequence;
        set
        {
            ChangeThemeSeqAnim((int)value);
            _currentSequence = value;
            Messenger.Default.Publish(new EA012Payload(_currentSequence.ToString()));
            Logger.ContentTestLog($"Current Sequence: {currentSequence.ToString()}");
            
            
            switch (value)
            {
                case Sequence.Default:
                    break;
                case Sequence.SeqA_WheelStopGame:
                  AnimateAllSeats();
                    break;
                case Sequence.SeqB_Ambulance:
                    break;
                case Sequence.SeqC_PoliceCar:
                    break;
                case Sequence.SeqD_FireTruck:
                    break;
                case Sequence.SeqE_Airplane:
                    break;
                case Sequence.SeqF_Train:
                    break;
           
            }
        }
    }
    private Sequence _currentSequence = Sequence.Default;

    protected override void Init()
    {
        psResourcePath = "Runtime/EA012/Fx_Click";
        base.Init();
        BindObject(typeof(GameObj));
        Messenger.Default.Publish(new EA012Payload(nameof(Sequence.Default)));
        
        for (int i = (int)GameObj.Seat_A; i < SEAT_COUNT; i++)
        {
            isSeatClickedMap.Add(i, false);
        }
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        currentSequence = Sequence.SeqA_WheelStopGame;
    }

    public override void OnRaySynced()
    {


        if (currentSequence == Sequence.SeqA_WheelStopGame)
        {
            OnRaySyncedOnSeatSelection();
        }
        else if (currentSequence == Sequence.SeqB_Ambulance)
        {
            currentSequence = Sequence.SeqC_PoliceCar;
        }
        else if (currentSequence == Sequence.SeqC_PoliceCar)
        {
            currentSequence = Sequence.SeqD_FireTruck;
        }
        else if (currentSequence == Sequence.SeqD_FireTruck)
        {
            currentSequence = Sequence.SeqE_Airplane;
        }
        else if (currentSequence == Sequence.SeqE_Airplane)
        {
            currentSequence = Sequence.SeqF_Train;
        }
        base.OnRaySynced();
    }

    #region Seat Selection Part

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
