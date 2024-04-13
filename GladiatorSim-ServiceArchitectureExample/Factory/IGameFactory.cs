using System.Collections.Generic;
using System.Threading.Tasks;
using _Scripts.CodeBase.Gameplay.AI;
using _Scripts.CodeBase.Gameplay.AI.Gladiator;
using _Scripts.CodeBase.Gameplay.Common.Interfaces;
using _Scripts.CodeBase.Gameplay.Player;
using _Scripts.CodeBase.Gameplay.Scenes.BattleChoose.UI;
using _Scripts.CodeBase.Services;
using _Scripts.CodeBase.StaticData;
using _Scripts.CodeBase.StaticData.Levels;
using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.Factory.Game
{
    public interface IGameFactory : IService
    {
        public Task<GameObject> CreatePlayer(string path, Vector3 position, Quaternion rotation,
            Transform parent = null);

        public Task<GameObject> CreatePlayer(ChooseBattlePanel.BattlePreset battlePreset,
            LevelStaticData levelStaticData);

        public Task<GameObject> CreateAI(string prefabPath, Vector3 position, Quaternion rotation,
            Transform parent = null);

        public Task<List<IAIController>> CreateAIEnemy(ChooseBattlePanel.BattlePreset battlePreset,
            LevelStaticData levelStaticData);

        public void Cleanup();
    }
}