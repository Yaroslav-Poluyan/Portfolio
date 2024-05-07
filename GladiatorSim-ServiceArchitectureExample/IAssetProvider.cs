using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Scripts.CodeBase.Infrastructure.AssetManagement
{
   public interface IAssetProvider
   {
      Task<GameObject> Load(string assetPath);
      Task<TValue> LoadAs<TValue>(string assetPath) where TValue : Object;
      public Task<TValue[]> LoadAll<TValue>(string assetsPath);
   }
}