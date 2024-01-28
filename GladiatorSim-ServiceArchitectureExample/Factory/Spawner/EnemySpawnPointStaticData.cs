using System;
using _Scripts.CodeBase.Gameplay.AI;
using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.Factory.Spawner
{
    [Serializable]
    public class EnemySpawnPointStaticData
    {
        public string id;
        public Vector3 position;
        public string spawnPrefabPath;

        public EnemySpawnPointStaticData(string spawnPrefabPath, Vector3 position, string id)
        {
            this.spawnPrefabPath = spawnPrefabPath;
            this.position = position;
            this.id = id;
        }
    }
}