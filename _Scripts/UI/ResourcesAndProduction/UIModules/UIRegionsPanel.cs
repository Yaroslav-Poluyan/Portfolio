using System.Collections.Generic;
using _Scripts.Core.Regions;
using _Scripts.Interfaces;
using _Scripts.Network.Client;
using _Scripts.UI.Regions;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ResourcesAndProduction.UIModules
{
    public class UIRegionsPanel : MonoBehaviour, IUI
    {
        [SerializeField] private UIRegionBlock _regionBlockPrefab;
        [SerializeField] private VerticalLayoutGroup _layoutGroupForBlocks;
        [SerializeField] private Scrollbar _scrollbar;
        private readonly List<UIRegionBlock> _createdRegionProductionBlocks = new();
        private List<Region> _sortedRegionsSinceLastRefresh = new();

        public void Open(int openAtRegionID)
        {
            gameObject.SetActive(true);
            GenerateRegionsUI();
            UIManager.Instance.CheckForCameraController();
            _scrollbar.gameObject.SetActive(true);
            if (openAtRegionID != -1)
            {
                OpenAtRegion(openAtRegionID);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
            RegionsUI.Instance.UIRegionInfo.Close();
            ClearRegionsUI();
            UIManager.Instance.CheckForCameraController();
            _scrollbar.gameObject.SetActive(false);
        }

        private void OpenAtRegion(int regionID)
        {
            var neededRegionPosition = _sortedRegionsSinceLastRefresh.FindIndex(x => x.ID == regionID);
            _scrollbar.value = 1 - (float) neededRegionPosition / _sortedRegionsSinceLastRefresh.Count;
        }

        private void ClearRegionsUI()
        {
            foreach (Transform child in _layoutGroupForBlocks.transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void GenerateRegionsUI()
        {
            ClearRegionsUI();
            var playerRegions = RegionsManager.Instance.GetPlayerRegions(Client.Instance.LocalPlayer.netId);
            playerRegions.Sort((x, y) => x.EconomyEntities.Count.CompareTo(y.EconomyEntities.Count));
            playerRegions.Reverse();
            _sortedRegionsSinceLastRefresh = playerRegions;
            foreach (var region in playerRegions)
            {
                var regionBlock = Instantiate(_regionBlockPrefab, _layoutGroupForBlocks.transform);
                regionBlock.Init(region);
                _createdRegionProductionBlocks.Add(regionBlock);
            }
        }

        public void RefreshUIBlockForRegion(Region region)
        {
            var regionBlock = _createdRegionProductionBlocks.Find(x => x.LinkedRegion == region);
            if (regionBlock != null)
            {
                regionBlock.Refresh();
            }
        }
    }
}