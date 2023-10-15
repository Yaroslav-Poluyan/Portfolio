using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.Regions;
using _Scripts.ResorcesAndRecipes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Core.EconomySimulation.Pops
{
    public class PopManager : MonoBehaviour
    {
        public static PopManager Instance;
        public readonly List<Pop> Pops = new();
        [SerializeField] private ActorConfig _popActorConfig;
        [SerializeField] private PopsConfig _popConfig;
        [SerializeField] private float _initialPopPopulation = 100f;
        [SerializeField] private List<InitialPopRecipePreset> _initialRecipes = new();
        [field: SerializeField] public List<PopNeed> PopNeeds { get; private set; }

        [Serializable]
        public struct PopNeed
        {
            public ProductData _productData;
            public int _count;
        }

        [Serializable]
        public struct InitialPopRecipePreset
        {
            public RecipeData _recipeData;
            public int _count;
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
                    var region = RegionsManager.Instance.Regions[index];
                    var pop = region.AddPop(preset._recipeData.ID);
                    pop.Config = _popConfig;
                    pop.Population = _initialPopPopulation;
                    region.Name = preset._recipeData.name;
                    pop?.GiveStartupProducts();
                }
            }
        }

        public void Tick()
        {
            /*var shuffledPops = ShuffleEntities(Pops);
            foreach (var pop in shuffledPops)
            {
                pop.Product();
            }

            for (var index = 0; index < shuffledPops.Count; index++)
            {
                var pop = shuffledPops[index];
                pop.BuyResources();
                pop.ConsumeNeeds();
                pop.MakeDecisionForBuyPrices();
                pop.MakeDecisionForSellPrices();
                pop.MakeGeneralDecisions();
            }

            for (var index = 0; index < shuffledPops.Count; index++)
            {
                var pop = shuffledPops[index];
                pop.BuyResources();
                pop.ConsumeNeeds();
                pop.MakeDecisionForBuyPrices();
                pop.MakeDecisionForSellPrices();
                pop.MakeGeneralDecisions();
            }*/
        }

        private List<Pop> ShuffleEntities(List<Pop> pops)
        {
            var shuffled = pops.OrderBy(x => Random.value).ToList();
            return shuffled;
        }

        public Pop AddPop(Region region, RecipeData recipeData)
        {
            var pop = new Pop
            {
                Production = new Production(recipeData),
                Storage = new Storage(),
                Actor = new Actor(_popActorConfig),
                RegionID = region.ID,
                ID = Pops.Count,
            };
            EconomyManager.Instance.Entities.Add(pop);
            Pops.Add(pop);
            return pop;
        }

        public Pop GetPop(int id)
        {
            return Pops[id];
        }

        public void SetProductionScale(int popID, int scale)
        {
            Pops[popID].Production.Scale = scale;
        }

        public void UpgradeProduction(int popID)
        {
            //EconomyEntities[economyEntityID].Production.MaxScale++;
        }

        public void ClearStorages()
        {
            foreach (var pop in Pops)
            {
                pop.ClearStorage();
            }
        }
    }
}