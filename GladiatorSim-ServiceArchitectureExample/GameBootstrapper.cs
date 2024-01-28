using _Scripts.CodeBase.Infrastructure.AssetManagement;
using _Scripts.CodeBase.Infrastructure.Factory;
using _Scripts.CodeBase.Infrastructure.SceneLoading;
using _Scripts.CodeBase.Infrastructure.StateMachine;
using _Scripts.CodeBase.Services.Curtain;
using _Scripts.CodeBase.Services.Input;
using _Scripts.CodeBase.Services.SaveLoad;
using _Scripts.CodeBase.StaticData;
using CodeBase.Services.SaveLoad;
using UnityEngine;
using Zenject;

// ReSharper disable Unity.PerformanceCriticalCodeInvocation

namespace _Scripts.CodeBase.Infrastructure
{
    public class GameBootstrapper : MonoInstaller, ICoroutineRunner
    {
        private Game _game;

        public override void InstallBindings()
        {
            BindCoroutineRunner();
            BindAssetProvider();
            BindStaticDataService();
            BindContainerProgressWriter();
            BindProgressCurtain();
            BindSceneReferencesSO();
            BindSceneLoader();
            BindInputService();
            BindGameFactory();
            BindGame();
        }

        public override void Start()
        {
            EnterToBootstrapState();
        }

        private void EnterToBootstrapState()
        {
            _game = Container.Resolve<Game>();
            InitGame();
        }

        private void InitGame()
        {
            _game.stateMachine.Enter<BootstrapState>();
        }

        private void BindCoroutineRunner() => Container.Bind<ICoroutineRunner>()
            .FromInstance(this)
            .AsSingle()
            .NonLazy();

        private void BindAssetProvider() => Container.Bind<IAssetProvider>()
            .To<AssetProvider>()
            .AsSingle()
            .NonLazy();

        private void BindContainerProgressWriter() =>
            Container.Bind<IContainerProgressWriter>()
                .To<SaveLoadContainer>()
                .AsSingle()
                .NonLazy();

        private void BindStaticDataService() =>
            Container.Bind<IStaticDataService>()
                .To<StaticDataService>()
                .AsSingle()
                .NonLazy();

        private void BindSceneReferencesSO() =>
            Container.Bind<SceneLoaderReferencesSO>()
                .FromMethod(LoadSceneLoaderReferencesSO)
                .AsSingle()
                .NonLazy();

        private void BindSceneLoader() => Container.Bind<ISceneLoader>()
            .To<SceneLoader>()
            .AsSingle()
            .NonLazy();

        private void BindGame() => Container.Bind<Game>()
            .To<Game>()
            .AsSingle()
            .NonLazy();

        private void BindGameFactory() =>
            Container.Bind<IGameFactory>()
                .To<GameFactory>()
                .AsSingle()
                .NonLazy();

        private void BindProgressCurtain() => Container.Bind<IProgressCurtain>()
            .FromMethod(CreateLoadingCurtain)
            .AsSingle()
            .NonLazy();

        private LoadingCurtain CreateLoadingCurtain(InjectContext context)
        {
            var assetProvider = context.Container.Resolve<IAssetProvider>();
            return Instantiate(assetProvider.LoadAs<LoadingCurtain>(AssetsPaths.LoadingCurtain));
        }

        private SceneLoaderReferencesSO LoadSceneLoaderReferencesSO(InjectContext context) =>
            context.Container.Resolve<IAssetProvider>()
                .LoadAs<SceneLoaderReferencesSO>(AssetsPaths.SceneReferencesSO);

        private void BindInputService()
        {
            Container.Bind<IInputService>()
                .FromMethod(RegisterInputService)
                .AsSingle()
                .NonLazy();
        }

        private static IInputService RegisterInputService()
        {
            if (Application.isEditor)
            {
                //check is gamepad connected
                return new PlayStationInputService();
            }

            if (Application.platform == RuntimePlatform.PS4)
            {
                return new PlayStationInputService();
            }

            Debug.LogError("Unknown platform");
            return new StandaloneInputService();
        }
    }
}