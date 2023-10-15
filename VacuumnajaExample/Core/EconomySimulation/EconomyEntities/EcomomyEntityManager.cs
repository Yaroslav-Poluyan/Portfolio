using System;
using System.Collections.Generic;
using _Scripts.Core.Regions;
using _Scripts.ResorcesAndRecipes;
using UnityEngine;

namespace _Scripts.Core.EconomySimulation.EconomyEntities
{
    public class EcomomyEntityManager : MonoBehaviour
    {
        public static EcomomyEntityManager Instance;
        public readonly List<EconomyEntity> EconomyEntities = new();
        [SerializeField] private ActorConfig _actorConfig;
        [SerializeField] private List<InitialEconomyEntityRecipePreset> _initialRecipes = new();

        [Serializable]
        public struct InitialEconomyEntityRecipePreset
        {
            public RecipeData _recipeData;
            public int _count;
            public int _scale;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void InitWorld()
        {
            for (var index = 0; index < _initialRecipes.Count; index++)
            {
                var preset = _initialRecipes[index];
                for (int j = 0; j < preset._count; j++)
                {
                    var region = RegionsManager.Instance.Regions[0];
                    var entity = region.AddEconomyEntity(preset._recipeData.ID);
                    entity.Production.Scale = preset._scale;
                    region.Name = preset._recipeData.name;
                    entity?.GiveStartupProducts();
                }
            }
        }

        public void Tick()
        {
        }

        public EconomyEntity AddEconomyEntity(Region region, RecipeData recipeData)
        {
            var economyEntity = new EconomyEntity
            {
                Production = new Production(recipeData),
                Storage = new Storage(),
                Actor = new Actor(_actorConfig),
                RegionID = region.ID,
                ID = EconomyEntities.Count,
            };
            EconomyEntities.Add(economyEntity);
            EconomyManager.Instance.Entities.Add(economyEntity);
            return economyEntity;
        }

        public EconomyEntity GetEconomyEntity(int id)
        {
            return EconomyEntities[id];
        }

        public void SetProductionScale(int economyEntityID, int scale)
        {
            EconomyEntities[economyEntityID].Production.Scale = scale;
        }

        public void UpgradeProduction(int economyEntityID)
        {
            //EconomyEntities[economyEntityID].Production.MaxScale++;
        }

        public void ClearStorages()
        {
            foreach (var economyEntity in EconomyEntities)
            {
                economyEntity.ClearStorage();
            }
        }
    }
}