using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Playground
{
    internal static class RecipeManager
    {
        static Dictionary<string, IRecipe> allRecipes = new Dictionary<string, IRecipe>();
        static Dictionary<string, IRecipe> powerupRecipes = new Dictionary<string, IRecipe>();
        static bool isInitialised = false;

        internal static void Initialise()
        {
            AbstractRecipe[] itemRecipes = Resources.LoadAll<AbstractRecipe>("Recipes");
            foreach (AbstractRecipe recipe in itemRecipes)
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
        /// <param name="requiredWeaponCount">The number of weapons that must be offered.</param>
        /// <returns>An array of recipes that can be offered to the player.</returns>
        internal static List<IRecipe> GetOffers(int quantity, int requiredWeaponCount)
        {
            if (isInitialised == false)
            {
                Initialise();
            }

            List<IRecipe> offers = new List<IRecipe>();

            // Always offer a weapon on the first run
            if (requiredWeaponCount > 0)
            {
                List<WeaponPickupRecipe> weaponCandidates = GetPowerUpCandidates<WeaponPickupRecipe>();
                offers.Add(weaponCandidates[Random.Range(0, weaponCandidates.Count)]);
                quantity--;
                requiredWeaponCount--;
            }

            List<IRecipe> candidates = GetPowerUpCandidates<IRecipe>();

            for (int i = 0; i < quantity; i++)
            {
                if (candidates.Count == 0)
                {
                    break;
                }

                int index = Random.Range(0, candidates.Count);
                offers.Add(candidates[index]);
                candidates.RemoveAt(index);
            }

            return offers;
        }

        /// <summary>
        /// Get a list of powerup recipes that can be offered to the player.
        /// </summary>
        /// <returns></returns>
        private static List<T> GetPowerUpCandidates<T>() where T : IRecipe
        {
            List<T> candidates = new List<T>();
            foreach (IRecipe recipe in powerupRecipes.Values)
            {
                if (recipe is not T)
                {
                    continue;
                } 

                if (RogueLiteManager.persistentData.currentResources < recipe.Cost)
                {
                    continue;
                }

                if (RogueLiteManager.runData.GetCount(recipe) >= recipe.MaxStack
                    || RogueLiteManager.persistentData.GetCount(recipe) >= recipe.MaxStack)
                {
                    continue;
                }

                // TODO: Remove some portion of the recipes that are not particularly useful for the player at this time

                candidates.Add((T)recipe);
            }

            return candidates;
        }
    }
}
