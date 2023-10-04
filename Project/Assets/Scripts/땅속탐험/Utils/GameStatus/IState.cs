using System;


public interface IState
{
 // GameStatus 프로퍼티 추가
  public void Enter();
  public void Update();
  public void Exit();
  
  //animation status.
  //public void OnStateIK();
}
