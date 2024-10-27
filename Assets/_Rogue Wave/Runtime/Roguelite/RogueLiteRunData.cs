using NeoFPS;
using System;
using System.Collections.Generic;
using System.Linq;

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
            m_RunRecipeData.Clear();
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
        
        /// <summary>
        /// The recipes that will be available to the player in their NanobotManager when they start a level in a run.
        /// This will be reset on death.
        /// </summary>
        /// <returns>True if the recipe is added, false if not added because already present.</returns> 
        public bool Add(IRecipe recipe)
        {
            recipe.Reset();

            // This method should call RunData.AddRecipe, not directly add to the list, which means this checking is probably redundant if we use the right method.
            if (m_RunRecipeData.Contains(recipe))
            {
                if (recipe.IsStackable == false)
                {
                    return false;
                }
                
                if (GetCount(recipe) >= recipe.MaxStack)
                {
                    return false;
                }
            }

            if (recipe is WeaponRecipe weapon)
            {
                // REFACTOR: this code is a duplicate of code in the persistent data class
                if (RogueLiteManager.persistentData.WeaponBuildOrder.Contains(recipe.UniqueID) == false)
                {
                    if (weapon.overridePrimaryWeapon || RogueLiteManager.persistentData.WeaponBuildOrder.Count == 0)
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

            m_RunRecipeData.Add(recipe);
            isDirty = true;
            return true;
        }

        internal void Remove(IRecipe recipe)
        {
            m_RunRecipeData.Remove(recipe);
            isDirty = true;
        }

        /// <summary>
        /// Get a list of all Run Recipes currently in the player's possession.
        /// They will be grouped by category name (key) and IRecipe (value).
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IGrouping<string, IRecipe>> GetGroupedRecipes()
        {
            return m_RunRecipeData.GroupBy(recipe => recipe.Category);
        }

        public List<IRecipe> GetRecipes()
        {
            return m_RunRecipeData;
        }

        public IRecipe GetRecipeAt(int index)
        {
            return m_RunRecipeData[index];
        }

        public void Clear()
        {
            m_RunRecipeData.Clear();
            isDirty = true;
        }

        public int Count { get { return m_RunRecipeData.Count; } }

        /// <summary>
        /// Get the number of instances of a supplied recipe that are in the player's current recipe permanent + temporary collection.
        /// </summary>
        /// <param name="recipe">The recipe to count instances of.</param>
        /// <returns>The number of times the recipse appears in the permanent + temporary collections..</returns>
        public int GetCount(IRecipe recipe)
        {
            return m_RunRecipeData.Count(r => r == recipe);
        }

        public bool Contains(IRecipe recipe)
        {
            return m_RunRecipeData.Contains(recipe);
        }

        // Add additional persistent data here
        // Can serialize value types, serializable structs and arrays/lists
        // Can NOT(!) serialize UnityEngine.Object references or non-serializable types like dictionary
        // Remember to set isDirty = true when changing a value

        public bool isDirty { get; set; } // TODO: Need to wrap values above to automate setting this on change
    }
}