public class EA035_WinterFood_GameManager : Ex_BaseGameManager
{
    private enum MainSeq
    {
        Default,
        Intro,
        Fish_Intro,
        Fish_Flour,
        Fish_Bean,
        Fish_Eat,
        Fish_Finish,
        Bread_Intro,
        Bread_Flour,
        Bread_Bean,
        Bread_Eat,
        Bread_Finish,
        OnFinish
    }


    public enum Objs
    {
        SeatSelection,
        Buttons,
        CakeA,
        CakeB,
        CakeC,
        CakeCream_A,
        CakeCream_B,
        CakeCream_C,
        CandySetRoot,
        OriginPosParent,
        TargetPos,
        CandleStartPos,
        Candles
    }

    private SeatSelectionController _seatSelectionController;
    private ButtonClickEventController _buttonClickEventController;

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
                
                
                
                case (int)MainSeq.Fish_Intro:
                    break;
                case (int)MainSeq.Fish_Flour:
                    break;
                case (int)MainSeq.Fish_Bean:
                    break;
                case (int)MainSeq.Fish_Eat:
                    break;
                case (int)MainSeq.Fish_Finish:
                    break;
                
                
                case (int)MainSeq.Bread_Intro:
                    break;
                case (int)MainSeq.Bread_Flour:
                    break;
                case (int)MainSeq.Bread_Bean:
                    break;
                case (int)MainSeq.Bread_Eat:
                    break;
                case (int)MainSeq.Bread_Finish:
                    break;
                
                
                
                
                case (int)MainSeq.OnFinish:
                    break;
            }
        }
    }

    protected override void Init()
    {
        base.Init();
    }
}