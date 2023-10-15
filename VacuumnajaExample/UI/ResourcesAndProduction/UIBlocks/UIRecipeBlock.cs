using _Scripts.ResorcesAndRecipes;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.UI.ResourcesAndProduction.UIBlocks
{
    public class UIRecipeBlock : MonoBehaviour
    {
        [SerializeField] private UIProductItemBlock _productItemBlockPrefab;
        [SerializeField] private VerticalLayoutGroup _inResourcesGroup;
        [SerializeField] private VerticalLayoutGroup _outResourcesGroup;

        public void Init(RecipeData data)
        {
            foreach (var inputItem in data._inputItems)
            {
                var productItem = Instantiate(_productItemBlockPrefab, _inResourcesGroup.transform);
                productItem.Init(inputItem._productData, inputItem._count);
            }

            foreach (var outputItem in data._outputItems)
            {
                var productItem = Instantiate(_productItemBlockPrefab, _outResourcesGroup.transform);
                productItem.Init(outputItem._productData, outputItem._count);
            }
        }
    }
}