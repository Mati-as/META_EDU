namespace 땅속탐험.Utils.GameStatus.UIStatus
{
    public interface IUIState
    {
        public UIStateList UIState { get; } 
        public enum UIStateList
        {
            Tutorial,
            StoryIntro,
            InPlay,
            StoryOutro,
            Pause
        }
        public void Enter()
        {
        
        }

        public void Update()
        {
        
        }

        public void Exit()
        {
        
        }
    }
}