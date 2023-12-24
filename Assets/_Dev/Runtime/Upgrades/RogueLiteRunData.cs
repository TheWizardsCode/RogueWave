using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Playground
{
    [Serializable]
    public class RogueLiteRunData
    {


        private static List<FpsInventoryItemBase> m_RunLoadoutData = new List<FpsInventoryItemBase>();
        public List<FpsInventoryItemBase> Loadout { get { return m_RunLoadoutData; } }
        /// <summary>
        /// Add an items that will be available to the player in their loadout when they start a level in a run.
        /// This will be lost on death.
        /// </summary>
        /// <returns>True if the item is added, false if not added because already present.</returns> 
        public bool Add(FpsInventoryItemBase item)
        {
            if (m_RunLoadoutData.Contains(item))
            {
                return false;
            }

            m_RunLoadoutData.Add(item);
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
            if (m_RunRecipeData.Contains(recipe))
            {
                return false;
            }

            m_RunRecipeData.Add(recipe);
            return true;
        }

        // Add additional persistent data here
        // Can serialize value types, serializable structs and arrays/lists
        // Can NOT(!) serialize UnityEngine.Object references or non-serializable types like dictionary
        // Remember to set isDirty = true when changing a value

        public bool isDirty { get; set; } // TODO: Need to wrap values above to automate setting this on change
    }
}