namespace 땅속탐험.Utils.GameStatus.UIStatus
{
    public interface IUIState
    {
        public UIStateList Gamestate { get; } 
        public enum UIStateList
        {
            Tutorial,
            StoryIntro,
            InPlay,
            StoryOutro
        }
    
        // GameStatus 프로퍼티 추가
        public void Enter();
        public void Update();
        public void Exit();
    }
}