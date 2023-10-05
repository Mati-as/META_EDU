namespace 땅속탐험.Utils.GameStatus.UIStatus
{
    public class UIStateController :IUIState
    {
        public IUIState.UIStateList UIState { get; }

        public IUIState currentUIState;
        
        public void Enter()
        {
            currentUIState?.Enter();
        }

        public void Update()
        {
            currentUIState?.Exit();
        }

        public void Exit()
        {
            currentUIState?.Exit();
        }
    }
}
