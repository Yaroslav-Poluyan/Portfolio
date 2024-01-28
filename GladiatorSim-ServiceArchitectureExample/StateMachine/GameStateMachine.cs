using System;
using System.Collections.Generic;
using _Scripts.CodeBase.Infrastructure.Factory;
using _Scripts.CodeBase.Infrastructure.SceneLoading;
using _Scripts.CodeBase.Services.Curtain;
using _Scripts.CodeBase.StaticData;

namespace _Scripts.CodeBase.Infrastructure.StateMachine
{
    public class GameStateMachine
    {
        private readonly Dictionary<Type, IExitableState> _states;
        private IExitableState _activeState;

        public GameStateMachine(ISceneLoader sceneLoader, IProgressCurtain progressCurtain, IGameFactory gameFactory,
            IStaticDataService staticData)
        {
            _states = new Dictionary<Type, IExitableState>
            {
                [typeof(BootstrapState)] =
                    new BootstrapState(this, sceneLoader, staticData),
                [typeof(LoadLevelState)] =
                    new LoadLevelState(this, sceneLoader, progressCurtain, staticData,gameFactory),
                [typeof(GameLoopState)] =
                    new GameLoopState(this, gameFactory),
            };
        }

        public void Enter<TState>() where TState : class, IState
        {
            IState state = ChangeState<TState>();
            state.Enter();
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
        {
            TState state = ChangeState<TState>();
            state.Enter(payload);
        }

        private TState ChangeState<TState>() where TState : class, IExitableState
        {
            _activeState?.Exit();

            TState state = GetState<TState>();
            _activeState = state;

            return state;
        }

        private TState GetState<TState>() where TState : class, IExitableState =>
            _states[typeof(TState)] as TState;
    }
}