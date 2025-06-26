using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class EA_Party : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Default,
        Intro,
        OnCream,
        OnDecorate,
        OnCandle,
        OnCelebrate,
        OnFinish
    }


    public enum Objs
    {
        SeatSelection,
        Buttons
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
                    break;

                case (int)MainSeq.OnCream:
                    
                    DOVirtual.DelayedCall(1.5f, () =>
                    {
                        baseUIManager.PopFromZeroInstructionUI("생크림을 터치해주세요!");
                        _buttonClickEventController.StartBtnClickSequential();
                    });
                    break;

                case (int)MainSeq.OnDecorate:
                    break;

                case (int)MainSeq.OnCandle:
                    break;

                case (int)MainSeq.OnCelebrate:
                    break;

                case (int)MainSeq.OnFinish:
                    break;
            }
        }
    }

    private SeatSelectionController _seatSelectionController;
    private ButtonClickEventController _buttonClickEventController;
    protected override void Init()
    {
        base.Init();
        BindObject(typeof(Objs));
        _seatSelectionController = GetObject((int)Objs.SeatSelection).GetComponent<SeatSelectionController>();
        _buttonClickEventController = GetObject((int)Objs.Buttons).GetComponent<ButtonClickEventController>();
        
        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
        _buttonClickEventController.OnButtonClicked += OnButtonClicked;
        
        _buttonClickEventController.OnAllBtnClicked -= OnAllBtnClicked;
        _buttonClickEventController.OnAllBtnClicked += OnAllBtnClicked;
        
        _seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
        _seatSelectionController.OnAllSeatSelected += OnAllSeatSelected;
        
    }



#if UNITY_EDITOR
    [SerializeField] private MainSeq _startSeq;    
#else
    [SerializeField] private MainSeq _startSeq = MainSeq.Intro;
#endif
    
    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();

        DOVirtual.DelayedCall(1f, () =>
        {
            baseUIManager.PopFromZeroInstructionUI("친구들! 각자 자리에 앉아 주세요!");
            _seatSelectionController.StartSeatSelection();
        });

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
        baseUIManager.PopFromZeroInstructionUI("잘했어! 빵에 생크림을 전부 올렸어!");
    }

    protected override void OnDestroy()
    {
        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
        _seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
        _seatSelectionController.OnAllSeatSelected -= OnAllSeatSelected;
        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
        _buttonClickEventController.OnButtonClicked -= OnButtonClicked;
        base.OnDestroy();
    }

    private void OnButtonClicked(int clickedButtonIndex)
    {
        
    }
}