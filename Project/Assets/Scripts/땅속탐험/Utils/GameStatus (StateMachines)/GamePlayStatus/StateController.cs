using UnityEditor;

namespace 땅속탐험.Utils
{
    public class StateController : BaseState
    {
        
        public IState currentState;
        public IState.GameStateList currentStateInfo;
  

        public  override void Enter()
        {
            currentState?.Enter();
        }
        public override void Update()
        {
            currentState?.Update();
        }

        public override void Exit()
        {
            currentState?.Exit();
        }
        
        /// <summary>
        /// 상태변경 로직, 순서 섞이지 않도록 주의
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(IState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentStateInfo = newState.GameState;
            currentState.Enter();
          
        }

       
    }
}