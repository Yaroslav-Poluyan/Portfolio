using System;
using System.Collections.Generic;
using _Scripts.Core.Regions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ResourcesAndProduction.UIBlocks
{
    public class UIRegionProductionBlock : MonoBehaviour
    {
        [SerializeField] private ShowType _showType = ShowType.EconomyEntities;
        [SerializeField] private UIProductionBlock _productionBlockPrefab;
        [SerializeField] private UICreateNewProductionBlock _createNewProductionBlockPrefab;
        [SerializeField] private TextMeshProUGUI _regionNameText;
        [SerializeField] private GridLayoutGroup _layoutGroupForBlocks;
        private List<UIProductionBlock> _productionBlocks = new List<UIProductionBlock>();
        public Region LinkedRegion { get; private set; }

        private enum ShowType
        {
            EconomyEntities,
            Pops
        }

        public void Init(Region region)
        {
            LinkedRegion = region;
            _regionNameText.text = _showType.ToString();
            CreateProductionsUI();
        }

        private void ClearProductionsUI()
        {
            foreach (Transform productionBlock in _layoutGroupForBlocks.transform)
            {
                Destroy(productionBlock.gameObject);
            }

            _productionBlocks.Clear();
        }

        private void CreateProductionsUI()
        {
            ClearProductionsUI();
            switch (_showType)
            {
                case ShowType.EconomyEntities:
                    foreach (var economyEntity in LinkedRegion.EconomyEntities)
                    {
                        var recipeBlock = Instantiate(_productionBlockPrefab, _layoutGroupForBlocks.transform);
                        recipeBlock.Init(economyEntity);
                        _productionBlocks.Add(recipeBlock);
                    }

                    break;
                case ShowType.Pops:
                    foreach (var pop in LinkedRegion.Pops)
                    {
                        var recipeBlock = Instantiate(_productionBlockPrefab, _layoutGroupForBlocks.transform);
                        recipeBlock.Init(pop);
                        _productionBlocks.Add(recipeBlock);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var newProductionBlock = Instantiate(_createNewProductionBlockPrefab, _layoutGroupForBlocks.transform);
            newProductionBlock.Init(this);
        }

        public void Refresh()
        {
            var recipeBlocksCount = _productionBlocks.Count;
            var neededBlocksCount = 0;
            switch (_showType)
            {
                case ShowType.EconomyEntities:
                    neededBlocksCount = LinkedRegion.EconomyEntities.Count;
                    break;
                case ShowType.Pops:
                    neededBlocksCount = LinkedRegion.Pops.Count;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (recipeBlocksCount != neededBlocksCount)
            {
                CreateProductionsUI();
                return;
            }

            foreach (var recipeBlock in _productionBlocks)
            {
                recipeBlock.Refresh();
            }
        }
    }
}