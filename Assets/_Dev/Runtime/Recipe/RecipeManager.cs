using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    internal static class RecipeManager
    {
        static Dictionary<string, IRecipe> allRecipes = new Dictionary<string, IRecipe>();
        static bool isInitialised = false;

        internal static void Initialise()
        {
            WeaponPickupRecipe[] recipes = Resources.LoadAll<WeaponPickupRecipe>("Recipes");
            foreach (WeaponPickupRecipe recipe in recipes)
            {
                allRecipes.Add(recipe.UniqueID, recipe);
            }

            isInitialised = true;
        }

        /// <summary>
        /// Get the recipe that uses the supplied GUID.
        /// </summary>
        /// <param name="GUID"></param>
        /// <param name="recipe">The recipe return value if it exists in the collection of reciped.</param>
        /// <returns></returns>
        internal static bool TryGetRecipeFor(string GUID, out IRecipe recipe)
        {
            if (isInitialised == false)
            {
                Initialise();
            }

            bool success = allRecipes.TryGetValue(GUID, out recipe);

            if (success == false)
            {
                Debug.LogError($"No recipe found for GUID {GUID}. This shouldn't happen. Has someone changed the GUID?");
            }

            return success;
        }
    }
}
