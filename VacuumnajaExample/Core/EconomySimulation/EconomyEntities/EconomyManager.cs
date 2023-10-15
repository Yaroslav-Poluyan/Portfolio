using System.Collections.Generic;
using System.Linq;
using _Scripts.Core.EconomySimulation.Interfaces;
using _Scripts.Core.EconomySimulation.Pops;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Core.EconomySimulation.EconomyEntities
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance;
        public float _defaultStartPrice = 1f;
        public float _defaultStartMoney = 100f;
        public int _capacityInTicks = 1;

        public List<Entity> Entities = new();

        private void Awake()
        {
            Instance = this;
        }

        public void Tick()
        {
            var shuffledEntites = ShuffleEntities();
            foreach (var economyEntity in shuffledEntites)
            {
                economyEntity.Product();
            }

            for (var index = 0; index < shuffledEntites.Count; index++)
            {
                var entity = shuffledEntites[index];
                entity.BuyResources();
                entity.MakeDecisionForBuyPrices();
                entity.MakeDecisionForSellPrices();
                entity.MakeGeneralDecisions();
            }

            for (var index = 0; index < shuffledEntites.Count; index++)
            {
                var entity = shuffledEntites[index];
                entity.BuyResources();
                entity.ConsumeNeeds();
                entity.MakeDecisionForBuyPrices();
                entity.MakeDecisionForSellPrices();
                entity.MakeGeneralDecisions();
            }

            foreach (var entity in shuffledEntites)
            {
                entity.ClearStorage();
            }
        }

        private List<Entity> ShuffleEntities()
        {
            var allEntities = new List<Entity>();
            allEntities.AddRange(EcomomyEntityManager.Instance.EconomyEntities);
            allEntities.AddRange(PopManager.Instance.Pops);
            var shuffled = allEntities.OrderBy(x => Random.value).ToList();
            return shuffled;
        }

        public void EndOfTick()
        {
        }
    }
}