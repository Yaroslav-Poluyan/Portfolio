using _Scripts.Camera;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.EconomySimulation.Pops;
using _Scripts.Core.Regions;
using _Scripts.UI.Regions;
using _Scripts.UI.ResourcesAndProduction;
using _Scripts.UI.Stats;
using MoreMountains.Tools;
using UnityEngine;

namespace _Scripts.UI
{
    public class UIManager : MMSingleton<UIManager>
    {
        [SerializeField] private RegionsUI _regionsUI;
        [SerializeField] private StatsUI _statsUI;
        [SerializeField] private EconomyEntityDetailsUI _economyEntityDetailsUI;
        [SerializeField] private PopDetailsUI _popDeaitlsUI;

        public void CheckForCameraController()
        {
            var state = _statsUI.gameObject.activeSelf || _regionsUI.gameObject.activeSelf;
            CameraController.Instance.SetActiveState(!state);
        }

        public void OpenStatsUI()
        {
            if (_regionsUI.gameObject.activeSelf)
            {
                _regionsUI.Close();
            }

            if (_statsUI.gameObject.activeSelf)
            {
                _statsUI.Close();
            }
            else _statsUI.Open();
        }

        public void OpenRegionsUI()
        {
            if (_regionsUI.gameObject.activeSelf)
            {
                _regionsUI.Close();
            }
            else _regionsUI.Open();

            if (_statsUI.gameObject.activeSelf)
            {
                _statsUI.Close();
            }
        }

        public void OpenEconomyEntityDetailsUI(EconomyEntity economyEntity)
        {
            _economyEntityDetailsUI.Open(economyEntity);
        }

        public void CloseRegionsUI()
        {
            _regionsUI.Close();
        }

        public void OpenRegionsUIAtRegionDetails(int regionID)
        {
            _regionsUI.OpenAtRegionInfo(RegionsManager.Instance.GetRegion(regionID));
        }

        public void OpenPopDetailsUI(Pop pop)
        {
            _popDeaitlsUI.Open(pop);
        }
    }
}