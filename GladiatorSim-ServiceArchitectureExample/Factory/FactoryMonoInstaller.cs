using _Scripts.CodeBase.Services.SaveLoad;
using CodeBase.Services.SaveLoad;
using UnityEngine;
using Zenject;

namespace _Scripts.CodeBase.Infrastructure.Factory
{
    public class FactoryMonoInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IContainerProgressWriter>().To<SaveLoadContainer>().AsSingle().NonLazy();
            Container.Bind<IGameFactory>().To<GameFactory>().AsSingle().NonLazy();
        }
    }
}