public abstract class BaseState : IState
{

    public IState.GameStateList Gamestate => IState.GameStateList.NotGameStarted;
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}