using System.Collections;
using System.Collections.Generic;
using _Scripts.Network.Client;
using _Scripts.UI.ResourcesAndProduction.UIBlocks;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.Storage
{
    public class UIStorageBlock : MonoBehaviour
    {
        [SerializeField] private UIProductItemBlock _productItemBlockPrefab;
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;
        private readonly List<UIProductItemBlock> _createdProductItemBlocks = new();
        private Coroutine _updateCoroutine;
        private Core.Regions.Storage _linkedStorage;

        public void Init(Core.Regions.Storage storage)
        {
            _linkedStorage = storage;
            CreateAllResourcesUI();
        }

        private void CreateAllResourcesUI()
        {
            ClearStorageUI();
            foreach (var (product, count) in _linkedStorage.Products)
            {
                var productItem = Instantiate(_productItemBlockPrefab, _gridLayoutGroup.transform);
                productItem.Init(product, count);
                _createdProductItemBlocks.Add(productItem);
            }

            _updateCoroutine ??= StartCoroutine(UpdateProductItemsCoroutine());
        }

        private void ClearStorageUI()
        {
            foreach (Transform child in _gridLayoutGroup.transform)
            {
                Destroy(child.gameObject);
            }

            _createdProductItemBlocks.Clear();
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        private IEnumerator UpdateProductItemsCoroutine()
        {
            var delay = new WaitForSeconds(.5f);
            while (enabled)
            {
                yield return delay;
                UpdateProductItems();
            }
        }

        private void UpdateProductItems()
        {
            foreach (var createdProductItemBlock in _createdProductItemBlocks)
            {
                var count = _linkedStorage.Products[createdProductItemBlock.ProductData];
                createdProductItemBlock.SetCount(count);
            }
        }
    }
}