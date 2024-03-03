using NeoFPS.SinglePlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RogueWave
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
                if (allRecipes.ContainsKey(recipe.UniqueID))
                {
                    Debug.LogError($"Duplicate recipe found for GUID {recipe.UniqueID} the two recipes are {recipe} and {allRecipes[recipe.uniqueID]}. This shouldn't happen.");
                    continue;
                }

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

            // Are we required to offer a weapon?
            List<WeaponPickupRecipe> weaponCandidates = null;
            if (requiredWeaponCount > 0)
            {
                weaponCandidates = GetOfferCandidates<WeaponPickupRecipe>();

                int idx = Random.Range(0, weaponCandidates.Count);
                for (int i = 0; i < weaponCandidates.Count; i++)
                {
                    if (weaponCandidates[idx].ShouldBuild)
                    {
                        int weight = 1;
                        offers.Add(weaponCandidates[idx]);
                        quantity--;
                        requiredWeaponCount--;
                        break;
                    }
                }

                if (offers.Count == 0)
                {
                    // there aren't any weapons that can be offered to the player.
                    return GetOffers(quantity, 0);
                }
            }

            List<IRecipe> candidates = GetOfferCandidates<IRecipe>();
            if (weaponCandidates != null)
            {
                candidates.RemoveAll(c => offers.Any(o => o.UniqueID == c.UniqueID));
            }

            WeightedRandom<IRecipe> weights = new WeightedRandom<IRecipe>();

            foreach (IRecipe candidate in candidates)
            {
                // TODO: calculate weights based on the player's current state and the recipe's attributes.
                weights.Add(candidate, candidate.weight);
            }

            for (int i = 0; i < quantity; i++)
            {
                if (weights.Count == 0)
                {
                    break;
                }

                IRecipe recipe = weights.GetRandom();
                offers.Add(recipe);
                weights.Remove(recipe);
            }

            return offers;
        }

        /// <summary>
        /// Get a list of powerup recipes that can be offered to the player.
        /// </summary>
        /// <returns>A list of possible offers. They have not yet been given weights.</returns>
        private static List<T> GetOfferCandidates<T>() where T : IRecipe
        {
            // TODO: cache the results of this search. Invalidate the case when a new recipe is added to the NanobotManager.
#if UNITY_EDITOR
            Debug.Log($"Getting offer candidates for {typeof(T)}." +
                $"\nNanobot level: {RogueLiteManager.persistentData.currentNanobotLevel}" +
                $"\nPowerup recipes: {powerupRecipes.Count}" +
                $"\nResources: {RogueLiteManager.persistentData.currentResources}");
#endif

            List<T> candidates = new List<T>();
            foreach (IRecipe recipe in powerupRecipes.Values)
            {

                if (RogueLiteManager.persistentData.currentResources < recipe.Cost)
                {
#if UNITY_EDITOR
                    Debug.Log($"Skip: {recipe} is too expensive for the player.");
#endif
                    continue;
                }

                if (RogueLiteManager.persistentData.currentNanobotLevel < recipe.Level)
                {
#if UNITY_EDITOR
                    Debug.Log($"Skip: {recipe} level of {recipe.Level} is higher than the current nanobot level of {RogueLiteManager.persistentData.currentNanobotLevel}.");
#endif
                    continue;
                }

                if (recipe is not T)
                {
#if UNITY_EDITOR
                    Debug.Log($"Skip: {recipe} is not of type {typeof(T)}.");
#endif
                    continue;
                } 

                if (recipe.CanOffer == false)
                {
#if UNITY_EDITOR
                    Debug.Log($"Skip: {recipe} is not available for offer.");
#endif
                    continue;
                }

                // TODO: Remove some portion of the recipes that are not particularly useful for the player at this time
#if UNITY_EDITOR
                Debug.Log($"Offer candidate: {recipe}");
#endif

                candidates.Add((T)recipe);
            }

#if UNITY_EDITOR
            string listOfCandidates = "";
            foreach (T candidate in candidates)
            {
                listOfCandidates += $"\t{candidate} with weight {candidate.weight}\n";
            }
            Debug.Log($"Offer candidates: {candidates.Count}\n{listOfCandidates}");
#endif
            return candidates;
        }
    }
}
