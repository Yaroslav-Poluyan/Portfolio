using System.Collections;
using System.Collections.Generic;
using _Scripts.ResorcesAndRecipes;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Stats.PricesSection
{
    public class UIProductPricesBlock : MonoBehaviour
    {
        [SerializeField] private UIProductPriceBlock _productItemBlockPrefab;
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;
        private readonly List<UIProductPriceBlock> _createdPricesBlocks = new();
        private Coroutine _updateCoroutine;
        private Dictionary<ProductData, float> _linkedPrices;

        public void Init(Dictionary<ProductData, float> prices)
        {
            _linkedPrices = prices;
            CreateAllResourcesUI();
        }

        private void CreateAllResourcesUI()
        {
            ClearPricesUI();
            foreach (var (product, price) in _linkedPrices)
            {
                var productItem = Instantiate(_productItemBlockPrefab, _gridLayoutGroup.transform);
                productItem.Init(product, price);
                _createdPricesBlocks.Add(productItem);
            }

            _updateCoroutine ??= StartCoroutine(UpdateProductItemsCoroutine());
        }

        private void ClearPricesUI()
        {
            foreach (Transform child in _gridLayoutGroup.transform)
            {
                Destroy(child.gameObject);
            }

            _createdPricesBlocks.Clear();
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        private IEnumerator UpdateProductItemsCoroutine()
        {
            while (enabled)
            {
                yield return null;
                UpdateProductItems();
            }
        }

        private void UpdateProductItems()
        {
            foreach (var createdProductItemBlock in _createdPricesBlocks)
            {
                var price = _linkedPrices[createdProductItemBlock.ProductData];
                createdProductItemBlock.SetCount(price);
            }
        }
    }
}