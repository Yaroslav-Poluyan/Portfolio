using System;
using System.Globalization;
using _Scripts.Core.EconomySimulation;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.EconomySimulation.Pops;
using _Scripts.Core.Regions;
using _Scripts.Interfaces;
using _Scripts.UI.Stats.PricesSection;
using _Scripts.UI.Storage;
using TMPro;
using UnityEngine;

namespace _Scripts.UI.Regions
{
    public class PopDetailsUI : MonoBehaviour, IUI
    {
        public static PopDetailsUI Instance { get; private set; }
        [SerializeField] private UIStorageBlock _uiStorageBlock;
        [SerializeField] private UIProductPricesBlock _uiProductPricesBlock;
        [SerializeField] private TextMeshProUGUI _entityNameText;
        [SerializeField] private TextMeshProUGUI _entityMoneyCountText;
        [SerializeField] private TextMeshProUGUI _happinessText;
        [SerializeField] private TextMeshProUGUI _populationText;
        private Pop _linkedPop;

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
                "Pop Money: " + _linkedPop.Actor.Money.ToString("F5", CultureInfo.InvariantCulture);
            _happinessText.text = "Happyness: " + _linkedPop.Happiness.ToString("F5", CultureInfo.InvariantCulture);
            _populationText.text = "Population: " + _linkedPop.Population.ToString("F5", CultureInfo.InvariantCulture);
        }

        public void Open(Pop pop)
        {
            _linkedPop = pop;
            UIManager.Instance.CloseRegionsUI();
            gameObject.SetActive(true);
            _entityNameText.text = "Name: " + pop.ID;
            _entityMoneyCountText.text =
                "EE Money: " + pop.Actor.Money.ToString("F5", CultureInfo.InvariantCulture);
            _uiStorageBlock.Init(pop.Storage);
            _uiProductPricesBlock.Init(pop.Actor.GetPrices());
        }

        public void Close()
        {
            gameObject.SetActive(false);
            UIManager.Instance.OpenRegionsUIAtRegionDetails(_linkedPop.RegionID);
        }
    }
}