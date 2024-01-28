using System;
using _Scripts.CodeBase.Infrastructure.Factory;
using _Scripts.CodeBase.Infrastructure.SceneLoading;
using _Scripts.CodeBase.Services.Curtain;
using _Scripts.CodeBase.StaticData;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.CodeBase.Infrastructure.StateMachine
{
    public class LoadLevelState : IPayloadedState<SceneType>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly ISceneLoader _sceneLoader;
        private readonly IProgressCurtain _loadingCurtain;
        private readonly IStaticDataService _staticData;
        private IGameFactory _gameFactory;

        public LoadLevelState(GameStateMachine gameStateMachine, ISceneLoader sceneLoader,
            IProgressCurtain loadingCurtain, IStaticDataService staticData, IGameFactory gameFactory)
        {
            _stateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
            _loadingCurtain = loadingCurtain;
            _staticData = staticData;
            _gameFactory = gameFactory;
        }

        public void Enter(SceneType sceneType)
        {
            Debug.Log("Enter LoadLevelState");
            _loadingCurtain.Show();
            _sceneLoader.Load(sceneType, OnLoaded);
        }

        public void Exit()
        {
            Debug.Log("Exit LoadLevelState");
            _loadingCurtain.Hide();
        }

        private void OnLoaded()
        {
            InitWorld();
            _stateMachine.Enter<GameLoopState>();
        }

        private void InitWorld()
        {
            var levelStaticData = GetLevelStaticData();
            InitPlayerSpawner(levelStaticData);
            InitEnemySpawners(levelStaticData);
        }

        private void InitEnemySpawners(LevelStaticData levelStaticData)
        {
            _gameFactory.CreateEnemySpawners(levelStaticData);
        }

        private void InitPlayerSpawner(LevelStaticData levelStaticData)
        {
            _gameFactory.CreatePlayerSpawner(levelStaticData);
        }

        private LevelStaticData GetLevelStaticData()
        {
            return _staticData.ForLevel(SceneManager.GetActiveScene().name);
        }
    }
}