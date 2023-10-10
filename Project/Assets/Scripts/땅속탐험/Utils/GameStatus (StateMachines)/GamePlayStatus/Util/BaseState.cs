public abstract class BaseState : IState
{
    public IState.GameStateList GameState { get; set; }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}