using System.Collections.Generic;
using _Scripts.ResorcesAndRecipes;
using UnityEngine;

namespace _Scripts.Core.EconomySimulation.EconomyEntities
{
    public class Actor
    {
        public float Money;
        private readonly ActorConfig _actorConfig;
        private readonly Dictionary<ProductData, float> _productPrices = new();
        private readonly Dictionary<ProductData, float> _lastProductPrices = new();
        public bool ShouldChangeScale => Random.Range(0f, 1f) < _actorConfig._scaleChangeChance;
        public bool ShouldAddNewEconomyEntity => Random.Range(0f, 1f) < _actorConfig._addNewEconomyEntityChance;
        private float _defaultPrice = 0f;

        public float GetPrice(ProductData productData)
        {
            _productPrices.TryAdd(productData, _defaultPrice);
            return _productPrices[productData];
        }

        public float GetLastPrice(ProductData productData)
        {
            _lastProductPrices.TryAdd(productData, _defaultPrice);
            return _lastProductPrices[productData];
        }

        public Actor(ActorConfig actorConfig)
        {
            _actorConfig = actorConfig;
            _defaultPrice = EconomyManager.Instance._defaultStartPrice;
            Money = EconomyManager.Instance._defaultStartMoney;
        }

        public void DecreasePriceForBuy(ProductData productData)
        {
            var multiplier = 1f / _actorConfig._buyPriceMultiplyer;
            _productPrices.TryAdd(productData, _defaultPrice);
            _productPrices[productData] *= multiplier;
        }

        public void IncreasePriceForBuy(ProductData inputItemProductData)
        {
            var multiplier = _actorConfig._buyPriceMultiplyer;
            _productPrices.TryAdd(inputItemProductData, 0);
            if (_actorConfig._priceChangeType == ActorConfig.PriceChangeType.Exponential)
            {
                _productPrices[inputItemProductData] *= multiplier;
            }
            else
            {
                _productPrices[inputItemProductData] += _actorConfig._buyPriceAddition;
            }

            _productPrices[inputItemProductData] = Mathf.Clamp(_productPrices[inputItemProductData], 0f, Money);
        }

        public void DecreasePriceForSell(ProductData productData)
        {
            var multiplier = 1f / _actorConfig._sellPriceMultiplyer;
            _productPrices.TryAdd(productData, 0);
            if (_actorConfig._priceChangeType == ActorConfig.PriceChangeType.Exponential)
            {
                _productPrices[productData] *= multiplier;
            }
            else
            {
                _productPrices[productData] -= _actorConfig._sellPriceAddition;
            }

            _productPrices[productData] = Mathf.Clamp(_productPrices[productData], 0f, Money);
        }

        public void IncreasePriceForSell(ProductData inputItemProductData)
        {
            var multiplier = _actorConfig._sellPriceMultiplyer;
            _productPrices.TryAdd(inputItemProductData, _defaultPrice);
            _productPrices[inputItemProductData] *= multiplier;
        }

        public Dictionary<ProductData, float> GetPrices()
        {
            return _productPrices;
        }

        public void SetPrice(ProductData productData, float price)
        {
            _productPrices[productData] = price;
            _lastProductPrices[productData] = price;
        }
    }
}