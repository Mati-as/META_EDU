namespace 땅속탐험.Utils
{
    public class StateController
    {
        private IState currentState;

        public void ChangeState(IState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        public void Update()
        {
            currentState?.Update();
        }
    }
}