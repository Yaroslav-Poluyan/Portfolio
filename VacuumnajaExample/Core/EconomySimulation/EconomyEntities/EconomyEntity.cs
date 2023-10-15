using System.Linq;
using _Scripts.Core.EconomySimulation.Interfaces;
using _Scripts.Core.EconomySimulation.Pops;
using _Scripts.ResorcesAndRecipes;
using _Scripts.Stats;
using UnityEngine;

namespace _Scripts.Core.EconomySimulation.EconomyEntities
{
    public class EconomyEntity : Entity
    {
        public override void BuyResources()
        {
            var isAllNeededResourcesAvailable = true;
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

                var thisProductCost = totalInputCost * (thisProductOutputValue / totalOutputValue);
                var minPrice = thisProductCost / (thisProductOutputValue / Actor.GetLastPrice(productData));
                var currentPrice = Actor.GetPrice(productData);
                Actor.SetPrice(productData, Mathf.Max(minPrice, currentPrice));
            }
        }

        public override void ConsumeNeeds()
        {
        }

        public override void MakeGeneralDecisions()
        {
            var isExcess = false;
            foreach (var (product, _) in Production.RecipeData._outputItems)
            {
                var excessCount = GetExcessCount(product);
                isExcess = isExcess || excessCount > 0;
            }
            if (!isExcess)
            {
                if (!Production.IsMaxScale)
                {
                    if (Actor.ShouldChangeScale) Production.Scale++;
                }
            }
            /*else 
            {
                if (Actor.ShouldChangeScale) Production.Scale--;
            }*/
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

            return EconomyManager.Instance._capacityInTicks * maxCanBeStoragedPerTick - Storage.Products[productData];
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

            if (EconomyManager.Instance._capacityInTicks * maxCanBeStoragedPerTick == 0f) return 1f;
            return Storage.Products[product] /
                   (float) (EconomyManager.Instance._capacityInTicks * maxCanBeStoragedPerTick);
        }
    }
}