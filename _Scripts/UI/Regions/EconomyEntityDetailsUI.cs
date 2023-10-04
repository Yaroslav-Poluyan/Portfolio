using System;
using System.Globalization;
using _Scripts.Core.EconomySimulation;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.Regions;
using _Scripts.Interfaces;
using _Scripts.UI.Stats.PricesSection;
using _Scripts.UI.Storage;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Regions
{
    public class EconomyEntityDetailsUI : MonoBehaviour, IUI
    {
        public static EconomyEntityDetailsUI Instance { get; private set; }
        [SerializeField] private UIStorageBlock _uiStorageBlock;
        [SerializeField] private UIProductPricesBlock _uiProductPricesBlock;
        [SerializeField] private TextMeshProUGUI _entityNameText;
        [SerializeField] private TextMeshProUGUI _entityMoneyCountText;
        private EconomyEntity _linkedEconomyEntity;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Update()
        {
            _entityMoneyCountText.text =
                "EE Money: " + _linkedEconomyEntity.Actor.Money.ToString("F5", CultureInfo.InvariantCulture);
        }

        public void Open(EconomyEntity economyEntity)
        {
            _linkedEconomyEntity = economyEntity;
            UIManager.Instance.CloseRegionsUI();
            gameObject.SetActive(true);
            _entityNameText.text = "Name: " + economyEntity.ID;
            _entityMoneyCountText.text =
                "EE Money: " + economyEntity.Actor.Money.ToString("F5", CultureInfo.InvariantCulture);
            _uiStorageBlock.Init(economyEntity.Storage);
            _uiProductPricesBlock.Init(economyEntity.Actor.GetPrices());
        }

        public void Close()
        {
            gameObject.SetActive(false);
            UIManager.Instance.OpenRegionsUIAtRegionDetails(_linkedEconomyEntity.RegionID);
        }
    }
}