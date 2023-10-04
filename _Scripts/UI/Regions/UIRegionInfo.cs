using System;
using System.Collections;
using _Scripts.Core.Regions;
using _Scripts.UI.ResourcesAndProduction;
using _Scripts.UI.ResourcesAndProduction.UIBlocks;
using _Scripts.UI.ResourcesAndProduction.UIModules;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Regions
{
    public class UIRegionInfo : MonoBehaviour
    {
        [SerializeField] private UINewProductionCreationPanel _uiNewProductionCreationPanel;
        [SerializeField] private UIRegionProductionBlock _regionProductionBlock;
        [SerializeField] private UIRegionProductionBlock _regionPopProductionBlock;
        [SerializeField] private TextMeshProUGUI _regionNameText;
        private Region _linkedRegion;
        private Coroutine _updateUICoroutine;

        private void OnEnable()
        {
            if (_updateUICoroutine != null) StopCoroutine(_updateUICoroutine);
            _updateUICoroutine = null;
            _updateUICoroutine ??= StartCoroutine(UpdateUICoroutine());
        }

        private void OnDisable()
        {
            if (_updateUICoroutine != null) StopCoroutine(_updateUICoroutine);
            _updateUICoroutine = null;
        }

        private IEnumerator UpdateUICoroutine()
        {
            var delay = new WaitForSeconds(1f);
            while (this)
            {
                yield return delay;
                Refresh();
            }
        }

        public void Open(Region region)
        {
            _linkedRegion = region;
            gameObject.SetActive(true);
            _regionNameText.text = region.Name;
            _regionProductionBlock.Init(region);
            _regionPopProductionBlock.Init(region);
        }

        public void BackToRegionsPanel()
        {
            _uiNewProductionCreationPanel.Close();
            RegionsUI.Instance.OpenRegionsPanel(_linkedRegion.ID);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void OpenNewProductionCreationPanel()
        {
            _uiNewProductionCreationPanel.Open(_linkedRegion);
        }

        public void Refresh()
        {
            _regionProductionBlock.Refresh();
            _regionPopProductionBlock.Refresh();
        }
    }
}