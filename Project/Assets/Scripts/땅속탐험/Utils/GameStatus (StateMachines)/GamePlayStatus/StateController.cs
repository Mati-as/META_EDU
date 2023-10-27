using UnityEditor;

namespace 땅속탐험.Utils
{
    /// <summary>
    /// 클래스의 상태를 관리해주는 클래스 입니다.
    /// </summary>
    /// <remarks>
    /// This is a class controlling overall states.
    /// </remarks>
    public class StateController : IState
    {  
        public IState currentState { get; set; }
        public IState.GameStateList GameState{get;set;}
        

        public   void Enter()
        {
            currentState?.Enter();
        }
        public  void Update()
        {
            currentState?.Update();
        }

        public  void Exit()
        {
            currentState?.Exit();
        }
        
        /// <summary>
        /// 상태변경 로직, 순서 섞이지 않도록 주의
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(BaseState newState)
        {
            currentState?.Exit();
            currentState?.Enter();
            currentState = newState;
            GameState = newState.GameState;


        }

       
    }
}