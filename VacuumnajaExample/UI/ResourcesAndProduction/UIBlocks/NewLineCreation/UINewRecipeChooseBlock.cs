using System;
using _Scripts.Core.Regions;
using _Scripts.ResorcesAndRecipes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ResourcesAndProduction.UIBlocks.NewLineCreation
{
    public class UINewRecipeChooseBlock : MonoBehaviour
    {
        public Action<(Region, RecipeData)> OnRecipeToAddChoosed;
        [SerializeField] private Button _button;
        [SerializeField] private UIRecipeBlock _recipeBlock;
        [SerializeField] private TextMeshProUGUI _alreadyExistText;
        [HideInInspector] public RecipeData _linkedRecipeData;
        [HideInInspector] public Region _linkedRegion;

        private void Start()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        public void Init(RecipeData recipeData, Region region)
        {
            _linkedRecipeData = recipeData;
            _linkedRegion = region;
            _recipeBlock.Init(recipeData);
        }

        private void OnButtonClick()
        {
            OnRecipeToAddChoosed?.Invoke((_linkedRegion, _linkedRecipeData));
        }

        public void SetInteractable(bool b)
        {
            _button.interactable = b;
            _alreadyExistText.gameObject.SetActive(!b);
        }
    }
}