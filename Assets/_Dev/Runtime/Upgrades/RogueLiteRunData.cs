using NeoFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Playground
{
    /// <summary>
    /// This data represents the state of a single run of the game.
    /// All values in this class will be reset on death.
    /// </summary>
    [Serializable]
    public class RogueLiteRunData
    {
        public int currentLevel = 0; // The currentl level of the player, the player advances a level each time they destroy all waves in a game level
        public int currentResources = 0; // The current resources of the player, the player gains resources by destroying enemies and loses resources by dying

        private static List<FpsInventoryItemBase> m_RunLoadoutData = new List<FpsInventoryItemBase>();
        public List<FpsInventoryItemBase> Loadout { get { return m_RunLoadoutData; } }

        public RogueLiteRunData()
        {
            m_RunLoadoutData.Clear();
            m_RunRecipeData.Clear();

#if UNITY_EDITOR
            if (RogueLiteManager.persistentData.runNumber == 0) // this will be the players first run
            {
                currentResources = 100000;
                Debug.Log("RogueLiteRunData: currentResources set to 100000 as it is the first run for this players profile and we are running in the editor.");
            }
#else

            if (RogueLiteManager.persistentData.runNumber == 0) // this will be the players first run
            {
                currentResources = 150;
            }
#endif
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
            if (m_RunRecipeData.Contains(recipe))
            {
                recipe.Reset();
                return false;
            }

            m_RunRecipeData.Add(recipe);
            isDirty = true;
            return true;
        }

        // Add additional persistent data here
        // Can serialize value types, serializable structs and arrays/lists
        // Can NOT(!) serialize UnityEngine.Object references or non-serializable types like dictionary
        // Remember to set isDirty = true when changing a value

        public bool isDirty { get; set; } // TODO: Need to wrap values above to automate setting this on change
    }
}