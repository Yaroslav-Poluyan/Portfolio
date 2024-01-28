﻿namespace _Scripts.CodeBase.Infrastructure.StateMachine
{
  public interface IState: IExitableState
  {
    void Enter();
  }

  public interface IPayloadedState<in TPayload> : IExitableState
  {
    void Enter(TPayload payload);
  }
  
  public interface IExitableState
  {
    void Exit();
  }
}