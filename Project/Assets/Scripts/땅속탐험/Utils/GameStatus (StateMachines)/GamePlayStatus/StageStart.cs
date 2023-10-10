using System;

    public class StageStart : BaseState
    {
     
      
        public override void Enter()
        {
            GameState = IState.GameStateList.StageStart;
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }
    }

