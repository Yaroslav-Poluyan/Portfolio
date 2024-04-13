using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _Scripts.CodeBase.Gameplay.AI;
using _Scripts.CodeBase.Gameplay.Common.Interfaces;
using _Scripts.CodeBase.Gameplay.Player;
using _Scripts.CodeBase.Gameplay.Scenes.BattleChoose.UI;
using _Scripts.CodeBase.Gameplay.SpawnPoints;
using _Scripts.CodeBase.Infrastructure.AssetManagement;
using _Scripts.CodeBase.Infrastructure.LevelContextManagers;
using _Scripts.CodeBase.StaticData.Levels;
using UnityEngine;
using Zenject;

namespace _Scripts.CodeBase.Infrastructure.Factory.Game
{
    public class GameFactory : FactoryBase, IGameFactory
    {
        public GameFactory(DiContainer container, IAssetProvider assetProvider, LevelController levelController)
            : base(container, assetProvider, levelController)
        {
        }

        public async Task<GameObject> CreateAI(string prefabPath, Vector3 position, Quaternion rotation,
            Transform parent = null)
        {
            return await CreateAndInject<GameObject>(prefabPath, position, rotation, parent);
        }

        public void Cleanup()
        {
            container.Unbind<PlayerController>();
        }

        public async Task<List<IAIController>> CreateAIEnemy(ChooseBattlePanel.BattlePreset battlePreset,
            LevelStaticData levelStaticData)
        {
            var targetEnemies = battlePreset._battleType switch
            {
                ChooseBattlePanel.BattlePreset.BattleType.OneVsOne => 1,
                ChooseBattlePanel.BattlePreset.BattleType.OneVsTwo => 2,
                ChooseBattlePanel.BattlePreset.BattleType.OneVsThree => 3,
                ChooseBattlePanel.BattlePreset.BattleType.OneVsAll => levelStaticData._spawners.Count,
                _ => throw new ArgumentOutOfRangeException()
            };
            var createdAIControllers = new List<IAIController>();
            foreach (var spawner in levelStaticData._spawners)
            {
                if (targetEnemies == 0) break;
                var spawnPoint = await Create<AISpawnPoint>(AssetsPaths.EnemySpawnPoint, spawner.position,
                    Quaternion.identity, null);
                var aiParent = await CreateAI(spawner.spawnPrefabPath, spawner.position, Quaternion.identity,
                    spawnPoint.transform);
                var aiController = aiParent.GetComponentInChildren<IAIController>();
                spawnPoint.LinkedAIController = aiController;
                createdAIControllers.Add(aiController);
                targetEnemies--;
            }

            return createdAIControllers;
        }

        /// <summary>
        /// use this method to create player in the scene without already created spawn point
        /// </summary>
        /// <param name="battlePreset"></param>
        /// <param name="levelStaticData"></param>
        /// <returns></returns>
        public async Task<GameObject> CreatePlayer(ChooseBattlePanel.BattlePreset battlePreset,
            LevelStaticData levelStaticData)
        {
            var spawnPoint = await Create<GameObject>(AssetsPaths.PlayerSpawnPoint, levelStaticData._playerSpawnPoint,
                Quaternion.identity, null);
            var player = await CreatePlayer(AssetsPaths.PlayerPrefabPath, levelStaticData._playerSpawnPoint,
                Quaternion.identity, spawnPoint.transform);
            return player;
        }

        /// <summary>
        /// use this method to create player in the scene with already created spawn point
        /// </summary>
        /// <param name="prefabPath"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public async Task<GameObject> CreatePlayer(string prefabPath, Vector3 position, Quaternion rotation,
            Transform parent)
        {
            var player = await CreateAndInject<GameObject>(prefabPath, position, rotation, parent);
            container.Unbind<PlayerController>();
            container.Bind<PlayerController>().FromInstance(player.GetComponentInChildren<PlayerController>());
            return player;
        }

        public void CreateAIAllySpawners(LevelStaticData levelStaticData)
        {
        }
    }
}