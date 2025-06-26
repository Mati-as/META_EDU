using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EA_Party : Ex_BaseGameManager
{
    public enum Objs
    {
        SeatSelection,
    }

    private SeatSelectionController _seatSelectionController;

    protected override void Init()
    {
        base.Init();
        BindObject(typeof(Objs));
        _seatSelectionController = GetObject((int)Objs.SeatSelection).GetComponent<SeatSelectionController>();
        _seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
        _seatSelectionController.OnAllSeatSelected += OnAllSeatSelected;
      
    }

    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        _seatSelectionController.InitForNewClick();
    }

    private void OnAllSeatSelected()
    {
        Logger.ContentTestLog($"전체 자리 선택 완료 -----------------------EA_Party");
        //_seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
    }

    protected override void OnDestroy()
    {
        _seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
        _seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
        base.OnDestroy();
    }
}
