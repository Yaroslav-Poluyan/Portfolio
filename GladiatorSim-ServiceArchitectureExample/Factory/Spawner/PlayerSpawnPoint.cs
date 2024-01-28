using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.Factory.Spawner
{
    public class PlayerSpawnPoint : SpawnPoint
    {
        protected override GameObject Spawn()
        {
            return factory.CreatePlayer(PrefabPath, transform.position, transform.rotation, transform);
        }
    }
}