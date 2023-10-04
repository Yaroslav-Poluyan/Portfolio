using _Scripts.Core.Regions;
using _Scripts.Interfaces;
using _Scripts.Network.Client;
using _Scripts.ResorcesAndRecipes;
using _Scripts.UI.ResourcesAndProduction.UIBlocks.NewLineCreation;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ResourcesAndProduction.UIModules
{
    public class UINewProductionCreationPanel : MonoBehaviour, IUI
    {
        [SerializeField] private VerticalLayoutGroup _layoutGroup;
        [SerializeField] private UINewRecipeChooseBlock _newRecipeChooseBlockPrefab;

        public void Open(Region region)
        {
            gameObject.SetActive(true);
            ClearNewRecipeChooseBlocks();
            var recipes = RecipesManager.Instance.Recipes;
            foreach (var recipe in recipes)
            {
                var recipeBlock = Instantiate(_newRecipeChooseBlockPrefab, _layoutGroup.transform);
                recipeBlock.Init(recipe, region);
                recipeBlock.OnRecipeToAddChoosed += OnRecipeToAddChoosed;
                if (region.EconomyEntities.Exists(x => x.Production.RecipeData == recipe))
                {
                    recipeBlock.SetInteractable(false);
                }
            }

            UIManager.Instance.CheckForCameraController();
        }

        private void OnRecipeToAddChoosed((Region region, RecipeData recipeData) t)
        {
            Client.Instance.LocalPlayer.RequestAddProduction(t.region.ID, t.recipeData.ID);
            Close();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            ClearNewRecipeChooseBlocks();
            UIManager.Instance.CheckForCameraController();
        }

        private void ClearNewRecipeChooseBlocks()
        {
            foreach (Transform child in _layoutGroup.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}