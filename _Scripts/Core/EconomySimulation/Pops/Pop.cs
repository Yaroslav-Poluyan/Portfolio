using System.Linq;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.EconomySimulation.Interfaces;
using _Scripts.ResorcesAndRecipes;
using _Scripts.Stats;
using UnityEngine;

namespace _Scripts.Core.EconomySimulation.Pops
{
    public class Pop : Entity
    {
        public PopsConfig Config { get; set; }
        public int Happiness { get; private set; }
        public int SatisfyLevel { get; private set; }
        public float Population { get; set; }


        public override int GetExcessCount(ProductData data)
        {
            base.GetExcessCount(data);
            if (PopManager.Instance.PopNeeds.Any(x => x._productData == data))
            {
                var neededForProduction =
                    (from productAndCount in PopManager.Instance.PopNeeds
                        where productAndCount._productData == data
                        select productAndCount._count * Production.Scale).FirstOrDefault();
                var atStorage = Storage.Products[data];
                return atStorage - neededForProduction;
            }

            return Storage.Products[data];
        }

        public override void SetMinimalSellPriceForProducts()
        {
            foreach (var (productData, _) in Production.RecipeData._outputItems)
            {
                var totalInputCost = 0f;
                var totalOutputValue = 0f;
                var thisProductOutputValue = 0f;

                foreach (var (product, count) in Production.RecipeData._inputItems)
                {
                    var lastPrice = Actor.GetLastPrice(product);
                    totalInputCost += lastPrice * (count * Production.Scale);
                }

                //base needs
                var popNeed = PopManager.Instance.PopNeeds[0];
                var popNeedLastPrice = Actor.GetLastPrice(popNeed._productData);
                totalInputCost += popNeedLastPrice * (popNeed._count * Production.Scale);
                //
                foreach (var (product, count) in Production.RecipeData._outputItems)
                {
                    var lastPrice = Actor.GetLastPrice(product);
                    var productOutputValue = lastPrice * (count * Production.Scale);
                    totalOutputValue += productOutputValue;

                    if (product == productData)
                    {
                        thisProductOutputValue = productOutputValue;
                    }
                }

                if (totalOutputValue == 0f) continue;
                var thisProductCost = totalInputCost * (thisProductOutputValue / totalOutputValue);
                var minPrice = thisProductCost / (thisProductOutputValue / Actor.GetLastPrice(productData));
                var currentPrice = Actor.GetPrice(productData);
                Actor.SetPrice(productData, Mathf.Max(minPrice, currentPrice));
            }
        }

        protected override int GetLackCount(ProductData data)
        {
            base.GetLackCount(data);
            if (PopManager.Instance.PopNeeds.Any(x => x._productData == data))
            {
                var neededForProduction =
                    (from productAndCount in PopManager.Instance.PopNeeds
                        where productAndCount._productData == data
                        select productAndCount._count * Production.Scale).FirstOrDefault();
                var atStorage = Storage.Products[data];
                return neededForProduction - atStorage;
            }

            return 0 - Storage.Products[data];
        }

        public override void BuyResources()
        {
            var isAllNeededResourcesAvailable = true;
            //pop needs
            foreach (var productAndCount in PopManager.Instance.PopNeeds)
            {
                var countToBuy = productAndCount._count * Production.Scale -
                                 Storage.Products[productAndCount._productData];
                var freeSpace = GetFreeSpaceCount(productAndCount._productData);
                if (countToBuy > freeSpace)
                {
                    countToBuy = freeSpace;
                }

                BuyResource(new RecipeData.ProductAndCount(productAndCount._productData, countToBuy));
            }

            //
            foreach (var productAndCount in Production.RecipeData._inputItems)
            {
                var countToBuy = productAndCount._count * Production.Scale -
                                 Storage.Products[productAndCount._productData];
                var freeSpace = GetFreeSpaceCount(productAndCount._productData);
                if (countToBuy > freeSpace)
                {
                    countToBuy = freeSpace;
                }

                BuyResource(new RecipeData.ProductAndCount(productAndCount._productData, countToBuy));
                countToBuy = productAndCount._count * Production.Scale -
                             Storage.Products[productAndCount._productData];
                if (countToBuy > 0) isAllNeededResourcesAvailable = false;
            }


            if (isAllNeededResourcesAvailable)
            {
                foreach (var productAndCount in Production.RecipeData._optionalInputItems)
                {
                    var countToBuy = productAndCount._count * Production.Scale -
                                     Storage.Products[productAndCount._productData];
                    BuyResource(new RecipeData.ProductAndCount(productAndCount._productData, countToBuy));
                }
            }
        }

        public override void MakeGeneralDecisions()
        {
        }

        public override void ConsumeNeeds()
        {
            //var satisfyLevel = -1;
            foreach (var popNeed in PopManager.Instance.PopNeeds)
            {
                var need = popNeed._count * Production.Scale;
                var atStorage = Storage.Products[popNeed._productData];
                if (atStorage < need) need = atStorage;
                Storage.Products[popNeed._productData] -= need;
                StatsManager.Instance.AddConsumedResources(popNeed._productData, need);
                Production.Scale = Mathf.FloorToInt(Population / Config._populationPerScale);
            }

            /*SatisfyLevel = satisfyLevel;
            if (satisfyLevel == -1) //base needs are not satisfied
            {
                Happiness = 0;
                Population *= 1 / Config._deathRate;
            }
            else if (satisfyLevel == PopManager.Instance.PopNeeds.Count - 1) //all needs are satisfied
            {
                Happiness = 100;
                Population *= Config._growthRate;
            }
            else //some needs are satisfied
            {
                Happiness = (int) (satisfyLevel / (float) PopManager.Instance.PopNeeds.Count * 100);
                Population *= Config._growthRate;
            }

            StatsManager.Instance.AddPopulation(Population);
            Production.Scale = Mathf.FloorToInt(Population / Config._populationPerScale);*/
        }

        protected override int GetFreeSpaceCount(ProductData productData)
        {
            var maxCanBeStoragedPerTick = 0;
            if (Production.RecipeData._inputItems.Any(x => productData == x._productData))
            {
                maxCanBeStoragedPerTick = Production.RecipeData._inputItems
                    .Where(productAndCount => productAndCount._productData == productData)
                    .Sum(productAndCount => productAndCount._count * Production.Scale);
            }
            else if (Production.RecipeData._optionalInputItems.Any(x => productData == x._productData))
            {
                maxCanBeStoragedPerTick = Production.RecipeData._optionalInputItems
                    .Where(productAndCount => productAndCount._productData == productData)
                    .Sum(productAndCount => productAndCount._count * Production.Scale);
            }
            else if (Production.RecipeData._outputItems.Any(x => productData == x._productData))
            {
                maxCanBeStoragedPerTick = Production.RecipeData._outputItems
                    .Where(productAndCount => productAndCount._productData == productData)
                    .Sum(productAndCount => productAndCount._count * Production.Scale);
            }
            else if (PopManager.Instance.PopNeeds.Any(x => productData == x._productData))
            {
                maxCanBeStoragedPerTick = PopManager.Instance.PopNeeds
                    .Where(productAndCount => productAndCount._productData == productData)
                    .Sum(productAndCount => productAndCount._count * Production.Scale);
            }

            var result = EconomyManager.Instance._capacityInTicks * maxCanBeStoragedPerTick -
                         Storage.Products[productData];
            if (result < 0)
            {
                result = 0;
            }

            return result;
        }

        public override float GetFillPercent(ProductData product)
        {
            var maxCanBeStoragedPerTick = 0;
            if (Production.RecipeData._inputItems.Any(x => product == x._productData))
            {
                maxCanBeStoragedPerTick = Production.RecipeData._inputItems
                    .Where(productAndCount => productAndCount._productData == product)
                    .Sum(productAndCount => productAndCount._count * Production.Scale);
            }
            else if (Production.RecipeData._optionalInputItems.Any(x => product == x._productData))
            {
                maxCanBeStoragedPerTick = Production.RecipeData._optionalInputItems
                    .Where(productAndCount => productAndCount._productData == product)
                    .Sum(productAndCount => productAndCount._count * Production.Scale);
            }
            else if (Production.RecipeData._outputItems.Any(x => product == x._productData))
            {
                maxCanBeStoragedPerTick = Production.RecipeData._outputItems
                    .Where(productAndCount => productAndCount._productData == product)
                    .Sum(productAndCount => productAndCount._count * Production.Scale);
            }
            else if (PopManager.Instance.PopNeeds.Any(x => product == x._productData))
            {
                maxCanBeStoragedPerTick = PopManager.Instance.PopNeeds
                    .Where(productAndCount => productAndCount._productData == product)
                    .Sum(productAndCount => productAndCount._count * Production.Scale);
            }

            if (EconomyManager.Instance._capacityInTicks * maxCanBeStoragedPerTick == 0f) return 1f;
            return Storage.Products[product] /
                   (float) (EconomyManager.Instance._capacityInTicks * maxCanBeStoragedPerTick);
        }
    }
}