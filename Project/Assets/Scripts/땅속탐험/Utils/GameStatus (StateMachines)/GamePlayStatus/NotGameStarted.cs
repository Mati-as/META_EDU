namespace 땅속탐험.Utils
{
    public class NotGameStarted : BaseState
    {
        
        
        public override void Enter()
        {
            GameState = IState.GameStateList.NotGameStarted;
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }
    }
}