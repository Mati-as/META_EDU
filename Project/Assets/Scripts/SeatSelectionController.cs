using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;



public class SeatSelectionController : Ex_MonoBehaviour
{
    public enum SeatType
    {
        Seat_A,
        Seat_B,
        Seat_C,
        Seat_D,
        Seat_E,
        Seat_F,
        Seatcount_Max
    }

    private Color _defaultColor;
    public Color ColorToChange;
    [SerializeField] private Color _selectedColor;
    protected override void Init()
    {
        BindObject(typeof(SeatType));
    }


    public void AnimateAllSeats()
    {
        for (int i = (int)SeatType.Seat_A; i <= (int)SeatType.Seatcount_Max; i++)
        {
            Logger.ContentTestLog($"AnimateAllSeats :Animating seat {(SeatType)i}");
            AnimateSeatLoop((SeatType)i);
        }
    }

    public void AnimateSeatLoop(SeatType seat)
    {
        var SeatTransform = GetObject((int)seat).transform;

        _sequenceMap[(int)seat]?.Kill();
        _sequenceMap[(int)seat] = DOTween.Sequence();
        _sequenceMap[(int)seat]
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 1.1f, 0.25f))
            .Append(SeatTransform.DOScale(_defaultSizeMap[(int)seat] * 0.9f, 0.35f))
            .SetLoops(-1, LoopType.Yoyo)
            .OnKill(() =>
            {
                SeatTransform.DOScale(_defaultSizeMap[(int)seat], 1);
            });

        _sequenceMap[(int)seat].Play();
    }
    
    public void DeactivateAllSeats()
    {
        for (int i = (int)SeatType.Seat_A; i <= (int)SeatType.Seatcount_Max; i++)
        {
            Logger.ContentTestLog($"AnimateAllSeats :Animating seat {(SeatType)i}");
            AnimateSeatLoop((SeatType)i);
        }
    }
    
    private readonly Dictionary<int, bool> isSeatClickedMap = new();
    private readonly Dictionary<int, MeshRenderer> _seatMeshRendererMap = new();
    
    private int _seatClickedCount = 0;

    private void InitForNewClick()
    {
        
    }
    public void OnSeatClicked(RaycastHit hit,Color color = default(Color))
    {
        bool isAllSeatClicked = true;
        int ID =hit.transform.GetInstanceID();
        if (isSeatClickedMap[_tfIdToEnumMap[ID]]) return;
        isSeatClickedMap[_tfIdToEnumMap[ID]] = true;

        var renderer = hit.transform.GetComponent<MeshRenderer>();
        _seatMeshRendererMap.TryAdd(_tfIdToEnumMap[ID], renderer);
        _seatMeshRendererMap[_tfIdToEnumMap[ID]].material.DOColor(_selectedColor, 0.35f);

        Managers.Sound.Play(SoundManager.Sound.Effect, "EA012/Seat_" + _seatClickedCount);
        _seatClickedCount++;


        _sequenceMap[_tfIdToEnumMap[ID]]?.Kill();

        foreach (int key in isSeatClickedMap.Keys)
            if (!isSeatClickedMap[key])
                isAllSeatClicked = false;

        if (isAllSeatClicked)
        {
            Logger.ContentTestLog("모든 자리가 선택되었습니다--------");

            OnAllSeatClicked();
            // Messenger.Default.Publish(new EA012Payload("OnSeatSelectFinished"));
        }
        
    }

    private void OnAllSeatClicked(Action action = null)
    {
        action?.Invoke();
    }

}
