public class AA006_VideoGameManager : VideoContentBaseGameManager
{
    protected override void OnGameStartStartButtonClicked()
    {
        base.OnGameStartStartButtonClicked();
        initialMessage= "눈꽃을 만들어 펭귄과 놀아볼까요?";
        _uiManagerCommonBehaviorController.ShowInitialMessage(initialMessage);
    }
}