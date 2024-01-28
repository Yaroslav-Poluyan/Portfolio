using _Scripts.CodeBase.Gameplay.Player;
using _Scripts.CodeBase.Infrastructure.AssetManagement;
using _Scripts.CodeBase.Infrastructure.Factory.Spawner;
using _Scripts.CodeBase.Services.SaveLoad;
using _Scripts.CodeBase.StaticData;
using UnityEngine;
using Zenject;

namespace _Scripts.CodeBase.Infrastructure.Factory
{
    public class GameFactory : FactoryBase, IGameFactory
    {
        private GameObject _heroGameObject;

        public GameFactory(DiContainer container, IAssetProvider assetProvider,
            IContainerProgressWriter progressWritersContainer)
            : base(container, assetProvider, progressWritersContainer)
        {
        }

        public GameObject CreatePlayer(string prefabPath, Vector3 position, Quaternion rotation,
            Transform parent = null)
        {
            var player = CreateAndInject<GameObject>(prefabPath, position, rotation, parent);
            _heroGameObject = player;
            container.Bind<PlayerController>().FromInstance(player.GetComponent<PlayerController>());
            return player;
        }

        public GameObject CreateEnemy(string prefabPath, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            return CreateAndInject<GameObject>(prefabPath, position, rotation, parent);
        }

        public void Cleanup()
        {
            container.Unbind<PlayerController>();
        }

        public void CreateEnemySpawners(LevelStaticData levelStaticData)
        {
            foreach (var spawner in levelStaticData._spawners)
            {
                var spawnPoint =
                    Create<EnemySpawnPoint>(AssetsPaths.EnemySpawnPoint, spawner.position, Quaternion.identity, null);
                spawnPoint.Construct(this, spawner.spawnPrefabPath);
            }
        }

        public void CreatePlayerSpawner(LevelStaticData levelStaticData)
        {
            var spawnPoint =
                Create<PlayerSpawnPoint>(AssetsPaths.PlayerSpawnPoint, levelStaticData._playerSpawnPoint,
                    Quaternion.identity, null);
            spawnPoint.Construct(this, AssetsPaths.PlayerPrefabPath);
        }
    }
}