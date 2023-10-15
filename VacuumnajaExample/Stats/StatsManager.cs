using System.Collections.Generic;
using System.Linq;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.EconomySimulation.Pops;
using _Scripts.Network.Client;
using _Scripts.ResorcesAndRecipes;
using UnityEngine;

namespace _Scripts.Stats
{
    public class StatsManager : MonoBehaviour
    {
        public static StatsManager Instance;
        public readonly List<TickStat> Stats = new();

        public class TickStat
        {
            public float Population;
            public float SatisfyLevel;
            public Dictionary<ProductData, ProductTickStat> ProductsTickStat;

            public TickStat()
            {
                ProductsTickStat = new Dictionary<ProductData, ProductTickStat>();
                foreach (var productData in ProductsManager.Instance.Products)
                {
                    ProductsTickStat.Add(productData, new ProductTickStat
                    {
                        ProducedCount = 0,
                        ConsumedCount = 0,
                        SoldCount = 0,
                        AverageSellPrice = 0f,
                        AverageStorageFillPercent = 0
                    });
                }

                Population = 0;
                SatisfyLevel = 0;
            }
        }

        public class ProductTickStat
        {
            public int ProducedCount;
            public int ConsumedCount;
            public int SoldCount;
            public float AverageSellPrice;
            public float AverageStorageFillPercent;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void AddProducedProducts(ProductData data, int count)
        {
            if (Stats[Client.Tick].ProductsTickStat.TryGetValue(data, out var stat))
            {
                stat.ProducedCount += count;
            }
            else
            {
                Stats[Client.Tick].ProductsTickStat.Add(data, new ProductTickStat());
                Stats[Client.Tick].ProductsTickStat[data].ProducedCount = count;
            }
        }

        public void AddConsumedResources(ProductData data, int count)
        {
            if (Stats[Client.Tick].ProductsTickStat.TryGetValue(data, out var stat))
            {
                stat.ConsumedCount += count;
            }
            else
            {
                Stats[Client.Tick].ProductsTickStat.Add(data, new ProductTickStat());
                Stats[Client.Tick].ProductsTickStat[data].ConsumedCount = count;
            }
        }

        public void AddSoldResources(ProductData data, int count)
        {
            if (Stats[Client.Tick].ProductsTickStat.TryGetValue(data, out var stat))
            {
                stat.SoldCount += count;
            }
            else
            {
                Stats[Client.Tick].ProductsTickStat.Add(data, new ProductTickStat());
                Stats[Client.Tick].ProductsTickStat[data].SoldCount = count;
            }
        }


        private float CalculateAveragePrice(ProductData product)
        {
            var sum = 0f;
            var count = 0;
            foreach (var economyEntity in EconomyManager.Instance.Entities)
            {
                if (economyEntity.Production.RecipeData._outputItems.Any(x => x._productData == product))
                {
                    var priceForSell = economyEntity.Actor.GetPrice(product);
                    sum += priceForSell;
                    count++;
                }
            }

            return count != 0 ? sum / count : 0f;
        }

        private void CalculateAveragePrices()
        {
            foreach (var product in Stats[^1].ProductsTickStat.Keys)
            {
                var avgPrice = CalculateAveragePrice(product);
                Stats[^1].ProductsTickStat[product].AverageSellPrice = avgPrice;
            }
        }

        private void CalculateStoragesFillPercents()
        {
            foreach (var product in Stats[Client.Tick].ProductsTickStat.Keys)
            {
                var avgStorageCount = CalculateAverageStorageFillPercent(product);
                Stats[Client.Tick].ProductsTickStat[product].AverageStorageFillPercent = avgStorageCount;
            }
        }

        private float CalculateAverageStorageFillPercent(ProductData product)
        {
            var averageStorageFillPercent = 0f;
            var count = 0;
            foreach (var entity in EconomyManager.Instance.Entities)
            {
                if (entity.Production.RecipeData._outputItems.Any(x => x._productData == product))
                {
                    averageStorageFillPercent += entity.GetFillPercent(product);
                    count++;
                }
            }

            return count != 0 ? averageStorageFillPercent / count : 0f;
        }

        public void Tick()
        {
            Stats.Add(new TickStat());
            CalculateAveragePrices();
            CalculateStoragesFillPercents();
            CalculateAverageSatisfyLevel();
        }

        public void AddPopulation(float population)
        {
            Stats[Client.Tick].Population += population;
        }

        private void CalculateAverageSatisfyLevel()
        {
            var sum = 0f;
            var count = 0;
            foreach (var pop in PopManager.Instance.Pops)
            {
                sum += pop.SatisfyLevel;
                count++;
            }

            Stats[Client.Tick].SatisfyLevel = count != 0 ? sum / count : 0f;
        }
    }
}