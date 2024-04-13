using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Scripts.CodeBase.Infrastructure.AssetManagement
{
    public class AssetProvider : IAssetProvider
    {
        public async Task<GameObject> Load(string assetPath)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(assetPath);
            await handle.Task;
            return handle.Result;
        }

        public async Task<TValue> LoadAs<TValue>(string assetPath) where TValue : Object
        {
            if (assetPath.EndsWith(".prefab"))
            {
                var handle = Addressables.LoadAssetAsync<GameObject>(assetPath);
                await handle.Task;

                if (handle.Result == null)
                {
                    Debug.LogError($"No such asset at path: {assetPath}");
                }

                if (typeof(TValue) != typeof(GameObject)) return handle.Result.GetComponent<TValue>();
                return handle.Result as TValue;
            }

            if (assetPath.EndsWith(".asset"))
            {
                var handle = Addressables.LoadAssetAsync<ScriptableObject>(assetPath);
                await handle.Task;

                if (handle.Result == null)
                {
                    Debug.LogError($"No such asset at path: {assetPath}");
                }

                return handle.Result as TValue;
            }

            Debug.LogError($"Unsupported asset type at path: {assetPath}");
            return null;
        }

        public async Task<TValue[]> LoadAll<TValue>(string assetsPath)
        {
            var handle = Addressables.LoadAssetsAsync<TValue>(assetsPath, null);
            await handle.Task;
            return handle.Result.ToArray();
        }
    }
}