using UnityEngine.SceneManagement;

public class AA006_VideoGameManager : VideoContentBaseGameManager
{
    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        initialMessage= "눈꽃을 만들어 펭귄과 놀아볼까요?";
       
        baseUIManager.PopInstructionUIFromScaleZero(initialMessage);
        Managers.Sound.Play(SoundManager.Sound.Narration, "OnGameStartNarration/" + SceneManager.GetActiveScene().name + "_intronarration");
    }
}