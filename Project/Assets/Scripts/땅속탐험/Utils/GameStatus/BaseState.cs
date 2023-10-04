public abstract class BaseState : IState
{
    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}