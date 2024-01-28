using _Scripts.CodeBase.Infrastructure.AssetManagement;
using _Scripts.CodeBase.Services.SaveLoad;
using UnityEngine;
using Zenject;

namespace _Scripts.CodeBase.Infrastructure.Factory
{
    public abstract class FactoryBase
    {
        protected readonly DiContainer container;
        protected readonly IAssetProvider assetProvider;
        protected readonly IContainerProgressWriter saveLoadContainer;

        protected FactoryBase(DiContainer container, IAssetProvider assetProvider,
            IContainerProgressWriter saveLoadContainer)
        {
            this.container = container;
            this.assetProvider = assetProvider;
            this.saveLoadContainer = saveLoadContainer;
        }

        protected TValue CreateAndInject<TValue>(string assetPath, Vector3 position, Quaternion rotation,
            Transform parent = null) where TValue : Object
        {
            var prefab = Load<TValue>(assetPath);
            return CreateAndInject(prefab, position, rotation, parent);
        }

        protected TValue CreateAndInject<TValue>(TValue prefab, Vector3 position, Quaternion rotation,
            Transform parent = null) where TValue : Object
        {
            TValue obj;
            if (typeof(TValue) == typeof(GameObject))
            {
                obj = container.InstantiatePrefab(prefab, position, rotation, parent) as TValue;
            }
            else if (typeof(TValue) == typeof(Component))
            {
                obj = container.InstantiatePrefabForComponent<TValue>(prefab, position, rotation,
                    parent);
            }
            else
            {
                obj = container.InstantiatePrefabForComponent<TValue>(prefab, position, rotation,
                    parent);
            }

            Register(obj);
            return obj;
        }

        protected TValue Create<TValue>(string assetPath, Vector3 position, Quaternion rotation,
            Transform parent) where TValue : Object
        {
            var asset = Load<TValue>(assetPath);
            return Create(asset, position, rotation, parent);
        }

        private TValue Create<TValue>(TValue prefab, Vector3 position, Quaternion rotation,
            Transform parent) where TValue : Object
        {
            TValue obj;
            if (typeof(TValue) == typeof(GameObject))
            {
                obj = Object.Instantiate(prefab, position, rotation, parent);
            }
            else if (typeof(TValue) == typeof(Component))
            {
                obj = Object.Instantiate(prefab, position, rotation, parent);
            }
            else
            {
                obj = Object.Instantiate(prefab, position, rotation, parent);
            }

            Register(obj);
            return obj;
        }


        private void Register(Object progressReader)
        {
            saveLoadContainer.Register(progressReader);
        }

        private GameObject Load(string assetPath)
        {
            return assetProvider.Load(assetPath);
        }

        private TValue Load<TValue>(string assetPath) where TValue : Object
        {
            return assetProvider.LoadAs<TValue>(assetPath);
        }
    }
}