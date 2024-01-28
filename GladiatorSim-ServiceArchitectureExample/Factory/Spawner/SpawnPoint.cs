using UnityEngine;

namespace _Scripts.CodeBase.Infrastructure.Factory.Spawner
{
    public abstract class SpawnPoint : MonoBehaviour
    {
        
        public string Id { get; set; }
        protected IGameFactory factory;
        protected string PrefabPath { get; set; }

        public void Construct(GameFactory gameFactory, string prefabPath)
        {
            factory = gameFactory;
            PrefabPath = prefabPath;
        }

        private void Start()
        {
            Spawn();
        }
        protected abstract GameObject Spawn();
    }
}