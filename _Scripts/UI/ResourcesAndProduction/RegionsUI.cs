using System;
using _Scripts.Core.Regions;
using _Scripts.Interfaces;
using _Scripts.UI.Regions;
using _Scripts.UI.ResourcesAndProduction.UIModules;
using MoreMountains.Tools;
using UnityEngine;

namespace _Scripts.UI.ResourcesAndProduction
{
    public class RegionsUI : MMSingleton<RegionsUI>, IUI
    {
        [field: SerializeField] public UIRegionsPanel UIRegionsPanel { get; private set; }
        [field: SerializeField] public UIRegionInfo UIRegionInfo { get; private set; }

        public void Close()
        {
            gameObject.SetActive(false);
            UIRegionsPanel.Close();
            UIRegionInfo.Close();
            UIManager.Instance.CheckForCameraController();
        }

        public void Open()
        {
            gameObject.SetActive(true);
            OpenRegionsPanel();
            UIManager.Instance.CheckForCameraController();
        }

        public void OpenRegionsPanel(int openAtRegionID = -1)
        {
            UIRegionsPanel.Open(openAtRegionID);
            UIRegionInfo.Close();
        }

        public void RefreshUIBlockForRegion(int regionID)
        {
            var region = RegionsManager.Instance.GetRegion(regionID);
            UIRegionsPanel.RefreshUIBlockForRegion(region);
            UIRegionInfo.Refresh();
        }

        public void OpenAtRegionInfo(Region linkedRegion)
        {
            gameObject.SetActive(true);
            UIRegionsPanel.Close();
            UIRegionInfo.Open(linkedRegion);
        }
    }
}