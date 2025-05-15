using System;



/// <summary>
/// CoreClass--------
/// UIManager 기본적인 초기화 수행합니다.
/// </summary>
public class UIManager_CommonBehaviorController : Base_UIManager
{
    public void ShowInitialMessage(string message, float duration = 5f, float delay = 1.5f)
    {
        Logger.ContentTestLog("ShowInitialMessage-----------------------------");
        if (message == string.Empty) return;

        PopFromZeroInstructionUI(message, duration, delay);
    }
}
