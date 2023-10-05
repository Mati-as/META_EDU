using System;
using UnityEngine;


public class GameStart : MonoBehaviour, IState
    {
        
        public IState.GameStateList Gamestate => IState.GameStateList.GameStart;
        public  void Enter()
        {
            
        }

        public void Update()
        {
        }

        public void Exit()
        {
        }

        //구독처리
        private void Start()
        {
        }
    }

