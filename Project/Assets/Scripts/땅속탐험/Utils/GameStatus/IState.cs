using System;


public interface IState
{
  public void Enter();
  public void Update();
  public void Exit();
  
  //animation status.
  //public void OnStateIK();
}
