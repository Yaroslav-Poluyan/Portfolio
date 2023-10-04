using System.Linq;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.EconomySimulation.Pops;
using _Scripts.Core.Regions;
using _Scripts.ResorcesAndRecipes;
using _Scripts.Stats;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Core.EconomySimulation.Interfaces
{
    public abstract class Entity
    {
        public Actor Actor { get; set; }
        public Production Production { get; set; }
        public Storage Storage { get; set; }
        public int ID { get; set; }
        public int RegionID { get; set; }
        public abstract void ConsumeNeeds();
        public abstract void MakeGeneralDecisions();
        public abstract float GetFillPercent(ProductData product);
        protected abstract int GetFreeSpaceCount(ProductData productData);
        public abstract void BuyResources();

        protected void BuyResource(RecipeData.ProductAndCount productAndCount)
        {
            var remainingCountToBuy = productAndCount._count;
            foreach (var entityseller in EconomyManager.Instance.Entities)
            {
                if (remainingCountToBuy <= 0) return;
                if (entityseller.Production.RecipeData._inputItems.Any(x =>
                        x._productData == productAndCount._productData)) continue;
                var sellerPrice = entityseller.Actor.GetPrice(productAndCount._productData);
                if (sellerPrice > Actor.GetPrice(productAndCount._productData)) continue;
                var supplyCount = entityseller.GetExcessCount(productAndCount._productData);
                if (supplyCount <= 0) continue;
                var price = (sellerPrice + Actor.GetPrice(productAndCount._productData)) / 2;
                var canBuy = (int) (Actor.Money / price);
                if (price <= 0) canBuy = supplyCount;
                if (canBuy < 0) canBuy = int.MaxValue;
                var countToBuy = Mathf.Min(supplyCount, remainingCountToBuy, canBuy);
                var cost = countToBuy * price;
                if (countToBuy == 0) continue;
                entityseller.Storage.Products[productAndCount._productData] -= countToBuy;
                Storage.Products[productAndCount._productData] += countToBuy;
                StatsManager.Instance.AddSoldResources(productAndCount._productData, countToBuy);
                remainingCountToBuy -= countToBuy;
                entityseller.Actor.Money += cost;
                Actor.Money -= cost;
                entityseller.Actor.SetPrice(productAndCount._productData, price);
                Actor.SetPrice(productAndCount._productData, price);
            }
        }

        private float GetPrice(ProductData productData)
        {
            SetMinimalSellPriceForProducts();
            return Actor.GetPrice(productData);
        }

        public void Product()
        {
            foreach (var optionalInputItem in Production.RecipeData._optionalInputItems)
            {
                var ableToConsume = optionalInputItem._count * Production.Scale;
                var haveToConsume = Storage.Products[optionalInputItem._productData];
                var countToConsume = Mathf.Min(ableToConsume, haveToConsume);
                Storage.Products[optionalInputItem._productData] -= countToConsume;
                StatsManager.Instance.AddConsumedResources(optionalInputItem._productData, ableToConsume);
            }

            if (Production.RecipeData._inputItems.Count == 0)
            {
                foreach (var outputItem in Production.RecipeData._outputItems)
                {
                    var countToProduce = outputItem._count * Production.Scale;
                    var freeSpace = GetFreeSpaceCount(outputItem._productData);
                    if (countToProduce > freeSpace)
                    {
                        countToProduce = freeSpace;
                    }

                    Storage.Products[outputItem._productData] += countToProduce;
                    StatsManager.Instance.AddProducedProducts(outputItem._productData, countToProduce);
                }

                return;
            }

            float percentOfMaxProduction = 1;
            foreach (var productAndCount in Production.RecipeData._inputItems)
            {
                var atStorage = Storage.Products[productAndCount._productData];
                var neededForProduction = productAndCount._count * Production.Scale;
                if (atStorage < neededForProduction)
                {
                    percentOfMaxProduction = Mathf.Min(percentOfMaxProduction, atStorage / (float) neededForProduction);
                }
            }

            //
            foreach (var (product, count) in Production.RecipeData._outputItems)
            {
                var freeSpace = GetFreeSpaceCount(product);
                var countToProduce = count * Production.Scale;
                if (countToProduce > freeSpace)
                {
                    percentOfMaxProduction = Mathf.Min(percentOfMaxProduction, freeSpace / (float) countToProduce);
                }
            }

            //
            var newProductionScale = (int) (Production.Scale * percentOfMaxProduction);
            //если нет места для ни одного продукта, то производство останавливается
            var freeSpaces =
                Production.RecipeData._outputItems.Count(outputItem => GetFreeSpaceCount(outputItem._productData) > 0);
            if (freeSpaces == 0) newProductionScale = 0;
            foreach (var outputItem in Production.RecipeData._outputItems)
            {
                var count = outputItem._count * newProductionScale;
                var canBeAdded = GetFreeSpaceCount(outputItem._productData);
                if (count > canBeAdded)
                {
                    Debug.LogError("Can't add " + count + " " + outputItem._productData.name + " to storage");
                }

                Storage.Products[outputItem._productData] += count;
                StatsManager.Instance.AddProducedProducts(outputItem._productData,
                    count);
            }

            foreach (var inputItem in Production.RecipeData._inputItems)
            {
                var itemToConsume = inputItem._count * newProductionScale;
                Storage.Products[inputItem._productData] -= itemToConsume;
                StatsManager.Instance.AddConsumedResources(inputItem._productData, itemToConsume);
            }
        }

        protected virtual int GetLackCount(ProductData data)
        {
            if (Production.RecipeData._inputItems.Any(x => x._productData == data))
            {
                var neededForProduction =
                    (from productAndCount in Production.RecipeData._inputItems
                        where productAndCount._productData == data
                        select productAndCount._count * Production.Scale).FirstOrDefault();
                var atStorage = Storage.Products[data];
                return neededForProduction - atStorage;
            }

            if (Production.RecipeData._optionalInputItems.Any(x => x._productData == data))
            {
                var neededForProduction =
                    (from productAndCount in Production.RecipeData._optionalInputItems
                        where productAndCount._productData == data
                        select productAndCount._count * Production.Scale).FirstOrDefault();
                var atStorage = Storage.Products[data];
                return neededForProduction - atStorage;
            }

            return 0 - Storage.Products[data];
        }

        public virtual int GetExcessCount(ProductData data)
        {
            if (Production.RecipeData._inputItems.Any(x => x._productData == data))
            {
                var neededForProduction =
                    (from productAndCount in Production.RecipeData._inputItems
                        where productAndCount._productData == data
                        select productAndCount._count * Production.Scale).FirstOrDefault();
                var atStorage = Storage.Products[data];
                return atStorage - neededForProduction;
            }

            if (Production.RecipeData._optionalInputItems.Any(x => x._productData == data))
            {
                var neededForProduction =
                    (from productAndCount in Production.RecipeData._optionalInputItems
                        where productAndCount._productData == data
                        select productAndCount._count * Production.Scale).FirstOrDefault();
                var atStorage = Storage.Products[data];
                return atStorage - neededForProduction;
            }

            return Storage.Products[data];
        }

        public void MakeDecisionForBuyPrices()
        {
            foreach (var (product, _) in Production.RecipeData._inputItems)
            {
                var lack = GetLackCount(product);
                if (lack > 0)
                {
                    Actor.IncreasePriceForBuy(product);
                }
            }

            //SetMinimalSellPriceForProducts();
        }

        public void MakeDecisionForSellPrices()
        {
            foreach (var (product, _) in Production.RecipeData._outputItems)
            {
                var excess = GetExcessCount(product);
                if (excess > 0)
                {
                    Actor.DecreasePriceForSell(product);
                }
            }

            //SetMinimalSellPriceForProducts();
        }

        public void ClearStorage()
        {
            foreach (var product in Storage.Products.Keys.ToList())
            {
                var excess = GetExcessCount(product);
                if (excess <= 0) continue;
                Storage.Products[product] -= excess;
            }
        }

        public void GiveStartupProducts()
        {
            foreach (var (product, count) in Production.RecipeData._inputItems)
            {
                Storage.Products[product] = count * Production.Scale;
            }
        }

        public abstract void SetMinimalSellPriceForProducts();
    }
}