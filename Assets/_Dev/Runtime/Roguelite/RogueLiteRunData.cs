using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RogueWave
{
    /// <summary>
    /// This data represents the state of a single run of the game.
    /// All values in this class will be reset on death.
    /// </summary>
    [Serializable]
    public class RogueLiteRunData
    {
        private static List<FpsInventoryItemBase> m_RunLoadoutData = new List<FpsInventoryItemBase>();
        public List<FpsInventoryItemBase> Loadout { get { return m_RunLoadoutData; } }

        public RogueLiteRunData()
        {
            Loadout.Clear();
            Recipes.Clear();
        }

        /// <summary>
        /// Add an items that will be available to the player in their loadout when they start a level in a run.
        /// This will be lost on death.
        /// </summary>
        /// <returns>True if the item is added, false if not added because already present.</returns> 
        public bool AddToLoadout(FpsInventoryItemBase item)
        {
            if (Loadout.Contains(item))
            {
                return false;
            }

            Loadout.Add(item);
            isDirty = true;
            return true;
        }

        private static List<IRecipe> m_RunRecipeData = new List<IRecipe>();
        public List<IRecipe> Recipes { get { return m_RunRecipeData; } }

        /// <summary>
        /// The recipes that will be available to the player in their NanobotManager when they start a level in a run.
        /// This will be reset on death.
        /// </summary>
        /// <returns>True if the recipe is added, false if not added because already present.</returns> 
        public bool Add(IRecipe recipe)
        {
            if (Recipes.Contains(recipe))
            {
                if (recipe.IsStackable == false)
                {
                    recipe.Reset();
                    return false;
                }
                
                if (GetCount(recipe) >= recipe.MaxStack)
                {
                    recipe.Reset();
                    return false;
                }
            }

            recipe.Reset();

            if (recipe is WeaponRecipe weapon)
            {
                // REFACTOR: this code is a duplicate of code in the persistent data class
                if (RogueLiteManager.persistentData.WeaponBuildOrder.Contains(recipe.UniqueID) == false)
                {
                    if (weapon.overridePrimaryWeapon)
                    {
                        RogueLiteManager.persistentData.WeaponBuildOrder.Insert(0, recipe.UniqueID);
                    }
                    else
                    {
                        RogueLiteManager.persistentData.WeaponBuildOrder.Insert(1, recipe.UniqueID);
                    }
                }

                if (weapon.ammoRecipe != null)
                {
                    Add(weapon.ammoRecipe);
                }
            }

            Recipes.Add(recipe);
            isDirty = true;
            return true;
        }

        internal void Remove(IRecipe recipe)
        {
            Recipes.Remove(recipe);
            isDirty = true;
        }

        /// <summary>
        /// Get the number of instances of a supplied recipe that are in the player's current recipe permanent + temporary collection.
        /// </summary>
        /// <param name="recipe">The recipe to count instances of.</param>
        /// <returns>The number of times the recipse appears in the permanent + temporary collections..</returns>
        public int GetCount(IRecipe recipe)
        {
            return Recipes.Count(r => r == recipe);
        }

        public bool Contains(IRecipe recipe)
        {
            return Recipes.Contains(recipe);
        }

        // Add additional persistent data here
        // Can serialize value types, serializable structs and arrays/lists
        // Can NOT(!) serialize UnityEngine.Object references or non-serializable types like dictionary
        // Remember to set isDirty = true when changing a value

        public bool isDirty { get; set; } // TODO: Need to wrap values above to automate setting this on change
    }
}