using _Scripts.CodeBase.Infrastructure.StateMachine;
using _Scripts.CodeBase.Infrastructure.StateMachine.States;
using _Scripts.CodeBase.Infrastructure.StateMachine.States.Common;

namespace _Scripts.CodeBase.Infrastructure.Factory.InfrastructureFactories
{
  /// <summary>
  /// Factory for infrastructure objects.
  /// </summary>
  public interface IInfrastructureFactory
  {
    /// <summary>
    /// Create state for state machine.
    /// </summary>
    /// <param name="stateMachine">State machine reference. Necessary for implemented dependency.</param>
    /// <typeparam name="TState">Type a created state. Must be implement <see cref="IExitableState"/>.</typeparam>
    /// <returns>A created state with an implemented dependency.</returns>
    /// <seealso cref="IExitableState"/>
    TState CreateState<TState>(IStateMachine stateMachine) where TState : IExitableState;
  }
}