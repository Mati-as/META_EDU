using System;

    public class GameOver : BaseState
    {
        
        
        public override void Enter()
        {
            GameState = IState.GameStateList.GameOver;
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }
    }

