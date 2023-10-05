using System;


public interface IState
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
    
 // GameStatus 프로퍼티 추가
  public void Enter();
  public void Update();
  public void Exit();
  
  //animation status.
  //public void OnStateIK();
}
