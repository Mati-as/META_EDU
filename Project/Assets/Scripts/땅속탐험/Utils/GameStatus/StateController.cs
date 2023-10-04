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
        
        public void ChangeState(IState newState)
        {
            currentState = newState;
            
            currentStateInfo = newState.Gamestate;
            
            currentState?.Exit();
            currentState.Enter();
        }

       
    }
}