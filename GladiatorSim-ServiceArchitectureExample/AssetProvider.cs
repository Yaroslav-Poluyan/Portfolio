using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Scripts.CodeBase.Infrastructure.AssetManagement
{
    public class AssetProvider : IAssetProvider
    {
        public GameObject Load(string assetPath)
        {
            return Resources.Load<GameObject>(assetPath);
        }

        public TValue LoadAs<TValue>(string assetPath) where TValue : Object
        {
            if (Resources.Load<TValue>(assetPath) == null)
            {
                Debug.LogError($"No such asset at path: {assetPath}");
            }

            return Resources.Load<TValue>(assetPath);
        }

        public TValue[] LoadAll<TValue>(string assetsPath) where TValue : Object
        {
            return Resources.LoadAll<TValue>(assetsPath);
        }

        public Task<GameObject> Instantiate(string address, Vector3 at) =>
            Addressables.InstantiateAsync(address, at, Quaternion.identity).Task;

        public Task<GameObject> Instantiate(string address) =>
            Addressables.InstantiateAsync(address).Task;
    }
}