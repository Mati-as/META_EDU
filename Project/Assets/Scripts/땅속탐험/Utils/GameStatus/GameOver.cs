using System;

    public class GameOver : BaseState
    {
        public IState.GameStateList Gamestate => IState.GameStateList.GameOver;
        
        public override void Enter()
        {
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }
    }

