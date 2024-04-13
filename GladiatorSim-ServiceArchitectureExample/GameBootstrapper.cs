using System.Threading.Tasks;
using _Scripts.CodeBase.Gameplay.GameSettings;
using _Scripts.CodeBase.Infrastructure.AssetManagement;
using _Scripts.CodeBase.Infrastructure.Factory.Game;
using _Scripts.CodeBase.Infrastructure.Factory.InfrastructureFactories;
using _Scripts.CodeBase.Infrastructure.Factory.UIFactory;
using _Scripts.CodeBase.Infrastructure.LevelContextManagers;
using _Scripts.CodeBase.Infrastructure.SceneLoading;
using _Scripts.CodeBase.Infrastructure.StateMachine;
using _Scripts.CodeBase.Infrastructure.StateMachine.States;
using _Scripts.CodeBase.Services;
using _Scripts.CodeBase.Services.Curtain;
using _Scripts.CodeBase.Services.Input;
using _Scripts.CodeBase.StaticData;
using _Scripts.CodeBase.StaticData.PlayerProgressData;
using UnityEngine;
using Zenject;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation

namespace _Scripts.CodeBase.Infrastructure
{
    public class GameBootstrapper : MonoInstaller, ICoroutineRunner
    {
        private Game _game;

        public override async void InstallBindings()
        {
            BindInfrastructureFactory();
            BindCoroutineRunner();
            BindAssetProvider();
            BindStaticDataService();
            BindPlayerProgressData();
            BindGameSettings();
            await BindLoadingSceneCurtain();
            await BindGameLoadCurtain();
            await BindSceneReferencesSO();
            BindSceneLoader();
            BindInputService();
            BindLevelController();
            BindGameFactory();
            BindUIFactory();
            BindTimeManager();
            _game = new Game(Container.Resolve<IInfrastructureFactory>());
            BindGameStateMachine();
            BindGame();
            EnterToBootstrapState();
        }

        public override void Start()
        {
        }

        private void BindTimeManager()
        {
            Container.Bind<TimeManagerService>()
                .AsSingle()
                .NonLazy();
        }

        private void BindGameSettings()
        {
            Container.Bind<GameSettings>().To<GameSettings>()
                .AsSingle()
                .NonLazy();
        }

        private void BindPlayerProgressData()
        {
            Container.Bind<PlayerProgressData>()
                .AsSingle()
                .NonLazy();
        }

        private void EnterToBootstrapState()
        {
            _game.StateMachine.Enter<BootstrapState>();
        }

        private void BindInfrastructureFactory() => Container.Bind<IInfrastructureFactory>()
            .To<InfrastructureFactory>()
            .AsSingle()
            .NonLazy();

        private void BindGameStateMachine() => Container.Bind<GameStateMachine>()
            .FromInstance(_game.StateMachine)
            .AsSingle();

        private void BindCoroutineRunner() => Container.Bind<ICoroutineRunner>()
            .FromInstance(this)
            .AsSingle()
            .NonLazy();

        private void BindAssetProvider() => Container.Bind<IAssetProvider>()
            .To<AssetProvider>()
            .AsSingle()
            .NonLazy();

        private void BindStaticDataService() =>
            Container.Bind<IStaticDataService>()
                .To<StaticDataService>()
                .AsSingle()
                .NonLazy();

        private async Task BindSceneReferencesSO()
        {
            var sceneLoaderReferencesSO = await LoadSceneLoaderReferencesSO(Container);
            Container.Bind<SceneLoaderReferencesSO>()
                .FromInstance(sceneLoaderReferencesSO)
                .AsSingle()
                .NonLazy();
        }

        private void BindSceneLoader() => Container.Bind<ISceneLoader>()
            .To<SceneLoader>()
            .AsSingle()
            .NonLazy();

        private void BindGame() => Container.Bind<Game>()
            .AsSingle()
            .NonLazy();

        private void BindGameFactory() =>
            Container.Bind<IGameFactory>()
                .To<GameFactory>()
                .AsSingle()
                .NonLazy();

        private void BindUIFactory() =>
            Container.Bind<IUIFactory>()
                .To<UIFactory>()
                .AsSingle()
                .NonLazy();

        private async Task BindGameLoadCurtain()
        {
            var curtain = await CreateLoadingGameCurtain(Container);
            Container.Bind<IGameLoadCurtain>()
                .FromInstance(curtain)
                .AsSingle()
                .NonLazy();
        }

        private async Task BindLoadingSceneCurtain()
        {
            var curtain = await CreateLoadingSceneCurtain(Container);
            Container.Bind<ISceneLoadingCurtain>()
                .FromInstance(curtain)
                .AsSingle()
                .NonLazy();
        }

        private async Task<LoadingGameCurtain> CreateLoadingGameCurtain(DiContainer container)
        {
            var assetProvider = container.Resolve<IAssetProvider>();
            var prefab = await assetProvider.LoadAs<LoadingGameCurtain>(AssetsPaths.LoadingGameCurtain);
            var loadingCurtain = Instantiate(prefab);
            loadingCurtain.HideForce();
            return loadingCurtain;
        }

        private async Task<LoadingSceneCurtain> CreateLoadingSceneCurtain(DiContainer container)
        {
            var assetProvider = container.Resolve<IAssetProvider>();
            var prefab = await assetProvider.LoadAs<LoadingSceneCurtain>(AssetsPaths.LoadingSceneCurtain);
            var loadingCurtain = Instantiate(prefab);
            loadingCurtain.HideForce();
            return loadingCurtain;
        }

        private async Task<SceneLoaderReferencesSO> LoadSceneLoaderReferencesSO(DiContainer container)
        {
            var so = await container.Resolve<IAssetProvider>()
                .LoadAs<SceneLoaderReferencesSO>(AssetsPaths.SceneReferencesSO);
            return so;
        }

        private void BindInputService()
        {
            Container.Bind<IInputService>()
                .FromMethod(RegisterInputService)
                .AsSingle()
                .NonLazy();
        }

        private void BindLevelController()
        {
            Container.Bind<LevelController>()
                .To<LevelController>()
                .AsSingle()
                .NonLazy();
        }

        private IInputService RegisterInputService()
        {
            if (Application.platform == RuntimePlatform.PS4)
            {
                //create new class via zenject
                return Container.Instantiate<PlayStationInputServiceBase>();
            }

            Debug.LogWarning("Unknown platform. Setting inputs to defaults: PlayStationInputServiceBase");
            return Container.Instantiate<PlayStationInputServiceBase>();
        }
    }
}