using _Scripts.Core.EconomySimulation;
using _Scripts.Core.EconomySimulation.EconomyEntities;
using _Scripts.Core.EconomySimulation.Pops;
using _Scripts.Core.Regions;
using _Scripts.Network.Client;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ResourcesAndProduction.UIBlocks
{
    public class UIProductionBlock : MonoBehaviour
    {
        [SerializeField] private UIRecipeBlock _recipeBlock;
        [SerializeField] private TextMeshProUGUI _productionScaleText;
        [SerializeField] private Button _scaleProductionButtonPlus;
        [SerializeField] private Button _scaleProductionButtonMinus;
        [SerializeField] private Button _openDetailsButton;
        private Production _linkedProduction;
        private Pop _linkedPop;
        private EconomyEntity _linkedEconomyEntity;

        private void Awake()
        {
            _openDetailsButton.onClick.AddListener(OpenDetailsButtonPressed);
            _scaleProductionButtonPlus.onClick.AddListener(() => ScaleProductionButtonPressed(1));
            _scaleProductionButtonMinus.onClick.AddListener(() => ScaleProductionButtonPressed(-1));
        }

        public void Init(EconomyEntity economyEntity)
        {
            _linkedEconomyEntity = economyEntity;
            _linkedProduction = _linkedEconomyEntity.Production;
            _recipeBlock.Init(_linkedProduction.RecipeData);
            _productionScaleText.text = _linkedProduction.Scale.ToString();
        }

        public void Init(Pop pop)
        {
            _linkedPop = pop;
            _linkedProduction = _linkedPop.Production;
            _recipeBlock.Init(_linkedProduction.RecipeData);
            _productionScaleText.text = _linkedProduction.Scale.ToString();
        }

        private void OpenDetailsButtonPressed()
        {
            if (_linkedEconomyEntity != null) UIManager.Instance.OpenEconomyEntityDetailsUI(_linkedEconomyEntity);
            else if (_linkedPop != null) UIManager.Instance.OpenPopDetailsUI(_linkedPop);
        }

        /// <summary>
        /// +1 -1 button
        /// </summary>
        /// <param name="value"></param>
        private void ScaleProductionButtonPressed(int value)
        {
            Client.Instance.LocalPlayer.RequestSetProductionScale(_linkedProduction.ID,
                _linkedProduction.Scale + value);
        }

        public void Refresh()
        {
            _productionScaleText.text = _linkedProduction.Scale.ToString();
        }
    }
}