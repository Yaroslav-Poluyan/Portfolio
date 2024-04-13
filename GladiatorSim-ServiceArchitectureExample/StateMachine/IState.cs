using System;
using System.Threading.Tasks;

namespace _Scripts.CodeBase.Infrastructure.StateMachine.States.Common
{
    public interface IState : IExitableState
    {
        /// <summary>
        /// Method to be called when entering the state.
        /// </summary>
        void Enter();
    }

    public interface IPayloadState<in TPayload> : IExitableState
    {
        /// <summary>
        ///  Method to be called when entering the state.
        /// </summary>
        /// <param name="payload">Payload to be passed to the state.</param>
        void Enter(TPayload payload);
    }

    /// <summary>
    ///  Interface for states that can be exited.
    /// </summary>
    public interface IExitableState
    {
        /// <summary>
        /// Method to be called when exiting the state.
        /// </summary>
        Task Exit();

        void RegisterOnExitTask(Func<Task> task);
        void RegisterOnExitTask(Action task);
    }
}