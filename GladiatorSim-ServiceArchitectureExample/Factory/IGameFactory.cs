using System.Threading.Tasks;
using _Scripts.CodeBase.Gameplay.AI;
using _Scripts.CodeBase.Services;
using _Scripts.CodeBase.StaticData;
using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        public GameObject CreatePlayer(string path, Vector3 position, Quaternion rotation, Transform parent = null);

        public GameObject CreateEnemy(string prefabPath, Vector3 position, Quaternion rotation,
            Transform parent = null);

        public void Cleanup();
        void CreateEnemySpawners(LevelStaticData levelStaticData);
        void CreatePlayerSpawner(LevelStaticData levelStaticData);
    }
}