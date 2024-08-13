using NeoFPS;
using RogueWave;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WizardsCode.RogueWave.UI;

namespace WizardsCode.RogueWave
{
    public class CharacterSetup : MonoBehaviour
    {
        [SerializeField, RequiredObjectProperty, Tooltip("The parent element for the recipes the character has.")]
        private RectTransform recipeList = null;
        [SerializeField, Tooltip("The UI element to use to represent a recipe. This will be cloned for each recipe.")]
        RecipeListUIElement buildItemPrototype;

        void Start()
        {
            ConfigureUI();
        }

        private void ConfigureUI()
        {
            IEnumerable<IGrouping<string, IRecipe>> groupedRunRecipes = RogueLiteManager.runData.Recipes
                .GroupBy(recipe => recipe.Category);

            foreach (var group in groupedRunRecipes)
            {
                //EditorGUILayout.LabelField($"{ExpandCamelCase(group.Key)}");
                foreach (var recipe in group)
                {
                    if (recipe.Category != "Weapon" && recipe.Category != "Ammunition")
                    {
                        InstantiateRecipeElement(recipe);
                    }
                }
            }
        }

        void InstantiateRecipeElement(IRecipe recipe)
        {
            RecipeListUIElement recipeUI = Instantiate(buildItemPrototype, recipeList);
            recipeUI.recipe = recipe;
            recipeUI.gameObject.SetActive(true);
        }

        private string ExpandCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");
        }
    }
}
