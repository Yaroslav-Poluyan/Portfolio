using _Scripts.CodeBase.Infrastructure.Factory;
using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.StateMachine
{
    public class GameLoopState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;

        public GameLoopState(GameStateMachine stateMachine, IGameFactory gameFactory)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
        }

        public void Exit()
        {
            Debug.Log("Exit GameLoopState");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _gameFactory.Cleanup();
        }

        public void Enter()
        {
            Debug.Log("Enter GameLoopState");
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}