using System;

    public class StageFinished : BaseState
    {
      

        public override void Enter()
        {
            GameState = IState.GameStateList.StageFinished;
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }
    }

