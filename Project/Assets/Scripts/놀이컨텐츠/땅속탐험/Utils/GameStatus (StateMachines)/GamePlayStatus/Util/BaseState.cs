public abstract class BaseState : IState
{
    public virtual IState.GameStateList GameState { get;  set; }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}