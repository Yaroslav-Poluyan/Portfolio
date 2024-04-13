using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.StateMachine.States.Common
{
    public abstract class ExitableStateBase : IExitableState
    {
        private readonly IStateMachine _gameStateMachine;

        private readonly List<Func<Task>> _onExitTasks = new();

        protected ExitableStateBase(IStateMachine gameStateMachine)
        {
            _gameStateMachine = gameStateMachine;
        }

        public void RegisterOnExitTask(Func<Task> task) => _onExitTasks.Add(task);
        public void RegisterOnExitTask(Action task) => _onExitTasks.Add(() => Task.Run(task));

        public virtual async Task Exit()
        {
            Debug.Log("<color=orange>Exiting state " + GetType().Name + "</color>");
            await ProcessOnExitTasks();
        }

        /// <summary>
        /// Process all tasks to be executed when exiting the state.
        /// </summary>
        private Task ProcessOnExitTasks()
        {
            var tasks = new Task[_onExitTasks.Count];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = _onExitTasks[i].Invoke();
            }

            _onExitTasks.Clear();
            return Task.FromResult(Task.WhenAll(tasks));
        }
    }
}