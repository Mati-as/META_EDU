using UnityEngine.SceneManagement;

public class AA005_VideoGameManager : VideoContentBaseGameManager
{
    protected override void OnGameStartButtonClicked()
    {
        base.OnGameStartButtonClicked();
        initialMessage= "코스모스를 터치하면 나비가 나타나요!";
         baseUIManager.PopInstructionUIFromScaleZero(initialMessage);
        Managers.Sound.Play(SoundManager.Sound.Narration, "OnGameStartNarration/" + SceneManager.GetActiveScene().name + "_intronarration");
    }
}

