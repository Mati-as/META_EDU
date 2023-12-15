using System;
using UnityEngine;


public class GameStart : BaseState
    {
        
        
        
        public override void Enter()
        {
            GameState = IState.GameStateList.GameStart;
        }

        public override void Update()
        {
        }

        public override void Exit()
        {
        }
    }

