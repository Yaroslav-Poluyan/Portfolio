using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _Scripts.ResorcesAndRecipes.Tools
{
    public class RecipesTool : MonoBehaviour
    {
    }

    public class RecipesToolrWindow : EditorWindow
    {
        [MenuItem("Vacuumnaja/Recipes Tool")]
        public static void ShowWindow()
        {
            GetWindow<RecipesToolrWindow>("Recipes Tool");
        }

        private void OnGUI()
        {
            var allRecipes = Resources.LoadAll<RecipeData>("Recipes");
            EditorGUILayout.LabelField("Recipes: " + allRecipes.Length);
            foreach (var recipe in allRecipes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(recipe, typeof(RecipeData), false);
                if (allRecipes.Any(x => x.ID == recipe.ID && x != recipe))
                {
                    GUI.color = Color.red;
                    EditorGUILayout.LabelField("ID: " + recipe.ID);
                    GUI.color = Color.white; // Reset color
                }
                else
                {
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField("ID: " + recipe.ID, EditorStyles.boldLabel);
                    GUI.color = Color.white; // Reset color
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Set IDs"))
            {
                for (int i = 0; i < allRecipes.Length; i++)
                {
                    allRecipes[i].ID = i;
                    EditorUtility.SetDirty(allRecipes[i]);
                }
            }
        }
    }
}