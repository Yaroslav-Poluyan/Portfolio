using System;
using System.Collections.Generic;
using _Scripts.Interfaces;
using _Scripts.Network.Client;
using _Scripts.ResorcesAndRecipes;
using _Scripts.Stats;
using MoreMountains.Tools;
using UnityEngine;

namespace _Scripts.UI.Stats
{
    public class StatsUI : MMSingleton<StatsUI>, IUI
    {
        [SerializeField] private UIProductStatsBlock _productStatsBlockPrefab;
        [SerializeField] private Transform _content;
        private readonly List<UIProductStatsBlock> _createdProductStatsBlocks = new();

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            Init();
        }

        private void Update()
        {
            Tick();
        }

        public void Tick()
        {
            var stats = StatsManager.Instance.Stats;
            foreach (var createdProductStatsBlock in _createdProductStatsBlocks)
            {
                if (stats[^1].ProductsTickStat
                    .TryGetValue(createdProductStatsBlock.LinkedProduct, out var stat))
                {
                    createdProductStatsBlock.Refresh(stat);
                }
            }
        }

        private void Init()
        {
            ClearCreatedProductStatsBlocks();
            foreach (var product in ProductsManager.Instance.Products)
            {
                var productStatsBlock = Instantiate(_productStatsBlockPrefab, _content);
                productStatsBlock.Init(product, new StatsManager.ProductTickStat());
                _createdProductStatsBlocks.Add(productStatsBlock);
            }
        }

        private void ClearCreatedProductStatsBlocks()
        {
            foreach (Transform child in _content)
            {
                Destroy(child.gameObject);
            }

            _createdProductStatsBlocks.Clear();
        }
    }
}