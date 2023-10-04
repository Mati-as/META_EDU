public abstract class BaseState : IState
{

    public GameStateList Gamestate { get; } 
    public enum GameStateList
    {
        NotGameStarted,
        GameStart,
        StageStart,
        StageFinished,
        GameOver,
        Paused
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}