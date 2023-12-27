using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
    internal static class RecipeManager
    {
        static Dictionary<string, IRecipe> allRecipes = new Dictionary<string, IRecipe>();
        static Dictionary<string, IRecipe> powerupRecipes = new Dictionary<string, IRecipe>();
        static bool isInitialised = false;

        internal static void Initialise()
        {
            WeaponPickupRecipe[] recipes = Resources.LoadAll<WeaponPickupRecipe>("Recipes");
            foreach (WeaponPickupRecipe recipe in recipes)
            {
                allRecipes.Add(recipe.UniqueID, recipe);
                if (recipe.IsPowerUp)
                {
                    powerupRecipes.Add(recipe.UniqueID, recipe);
                }
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



        /// <summary>
        /// Gets a number of upgrade recipes that can be offered to the player.
        /// </summary>
        /// <param name="quantity">The number of upgrades to offer.</param>
        /// <returns>An array of recipes that can be offered to the player.</returns>
        internal static IRecipe[] GetOffers(int quantity)
        {
            if (isInitialised == false)
            {
                Initialise();
            }

            IRecipe[] offers = new IRecipe[quantity];
            List<IRecipe> candidates = GetPoweupCandidates();

            if (candidates.Count < quantity)
            {
                Debug.Log("TODO: handle the situation where there are not enough recipes to offer.");
                return offers;
            }

            for (int i = 0; i < quantity; i++)
            {
                int index = Random.Range(0, candidates.Count);
                offers[i] = candidates[index];
                candidates.RemoveAt(index);
            }

            return offers;
        }

        /// <summary>
        /// Get a list of powerup recipes that can be offered to the player.
        /// </summary>
        /// <returns></returns>
        private static List<IRecipe> GetPoweupCandidates()
        {
            // TODO: Remove recipes that are already in the player's inventory
            // TODO: Remove recipes that are already in the player's loadout
            // TODO: Remove some portion of the recipes that are not particularly useful for the player at this time

            return new List<IRecipe>(powerupRecipes.Values);
        }
    }
}
