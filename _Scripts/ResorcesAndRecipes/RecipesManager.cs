using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine;

namespace _Scripts.ResorcesAndRecipes
{
    public class RecipesManager : MMSingleton<RecipesManager>
    {
        [field: SerializeField] public List<RecipeData> Recipes { get; private set; }

#if UNITY_EDITOR
        [SerializeField] private bool _getRecipesFromResources;
        private void OnValidate()
        {
            if (_getRecipesFromResources)
            {
                Recipes = Resources.LoadAll<RecipeData>("Recipes").ToList();
                _getRecipesFromResources = false;
            }
        }
#endif
        public RecipeData IdToRecipe(int id)
        {
            return Recipes.FirstOrDefault(x => x.ID == id);
        }

        public RecipeData GetRecipe(string recipeName)
        {
            return Recipes.FirstOrDefault(recipe => recipe.name == recipeName);
        }
    }
}