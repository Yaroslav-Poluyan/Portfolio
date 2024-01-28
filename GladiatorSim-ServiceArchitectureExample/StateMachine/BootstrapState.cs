using _Scripts.CodeBase.Infrastructure.SceneLoading;
using _Scripts.CodeBase.Services.Input;
using _Scripts.CodeBase.StaticData;
using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.StateMachine
{
    public class BootstrapState : IState
    {
        private const SceneType Bootstrapper = SceneType.Bootstrapper;
        private readonly GameStateMachine _stateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly IStaticDataService _staticData;

        public BootstrapState(GameStateMachine stateMachine, ISceneLoader sceneLoader, IStaticDataService staticData)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _staticData = staticData;
            LoadData();
        }

        public void Enter()
        {
            _sceneLoader.Load(Bootstrapper, onLoaded: EnterLoadLevel);
        }

        public void Exit()
        {
        }

        private void EnterLoadLevel()
        {
            _stateMachine.Enter<LoadLevelState, SceneType>(SceneType.Game);
        }

        private void LoadData()
        {
            _staticData.Load();
        }
    }
}