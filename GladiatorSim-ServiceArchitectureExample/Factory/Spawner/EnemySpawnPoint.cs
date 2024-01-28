using _Scripts.CodeBase.Gameplay.AI;
using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.Factory.Spawner
{
    public class EnemySpawnPoint : SpawnPoint
    {
        protected override GameObject Spawn()
        {
            return factory.CreateEnemy(PrefabPath, transform.position, transform.rotation, transform);
        }
    }
}