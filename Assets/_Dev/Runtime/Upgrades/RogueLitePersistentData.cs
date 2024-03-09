using System;
using System.Linq;
using System.Collections.Generic;

namespace RogueWave
{
    [Serializable]
    public class RogueLitePersistentData
    {
        // Can serialize value types, serializable structs and arrays/lists
        // Can NOT(!) serialize UnityEngine.Object references or non-serializable types like dictionary
        // Remember to set isDirty = true when changing a value

        internal int runNumber = 0;
        internal int currentGameLevel = 0; // The currentl level of the game, this advances each time the player completes all waves in a level.
        internal int currentNanobotLevel = 0; // The current level of the nanobots, this advances each time the player gahters enough resources to level up.

        int m_CurrentResources = 0; // The current resources of the player, the player gains resources by destroying enemies and loses resources by dying
        public int currentResources
        {
            get { return m_CurrentResources; }
            set
            {
                m_CurrentResources = value;
                isDirty = true;
            }
        }

        public List<string> RecipeIds = new List<string>();
        public List<string> _weaponBuildOrderBackingField = new List<string>(); // this is public to ensure it is serialized. TODO: write a custom serialiser for this class to avoid this
        internal List<string> WeaponBuildOrder
        {
            get
            {
                if (_weaponBuildOrderBackingField.Count > 0)
                {
                    return _weaponBuildOrderBackingField;
                }

                foreach (string id in RecipeIds)
                {
                    IRecipe recipe;
                    if (RecipeManager.TryGetRecipeFor(id, out recipe)
                        && (recipe is WeaponPickupRecipe || recipe is ToolPickupRecipe))
                    {
                        _weaponBuildOrderBackingField.Add(id);
                    }
                }
                
                return _weaponBuildOrderBackingField;
            }

            set
            {
                _weaponBuildOrderBackingField = value;
                isDirty = true;
            }
        }
        public RogueLitePersistentData()
        {
            currentNanobotLevel = 0;
            currentGameLevel = 0;
            if (runNumber == 0) // this will be the players first run
            {
                currentResources = 150;
            }
#if UNITY_EDITOR
            //currentResources = 100000;
            //Debug.Log("RogueLiteRunData: currentResources set to 100000 as we are running in the editor in debug mode.");
#endif
        }

        /// <summary>
        /// The recipes that will be available to the player in their NanobotManager when they start a level in a run.
        /// This will be reset on death.
        /// </summary>
        /// <returns>True if the recipe is added, false if not added because already present.</returns> 
        public bool Add(IRecipe recipe)
        {
            if (RecipeIds.Contains(recipe.UniqueID))
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

            RecipeIds.Add(recipe.UniqueID);
            if (recipe is WeaponPickupRecipe weapon)
            {
                if (WeaponBuildOrder.Contains(recipe.UniqueID) == false)
                {
                    if (weapon.overridePrimaryWeapon)
                    {
                        WeaponBuildOrder.Insert(0, recipe.UniqueID);
                    }
                    else
                    {
                        WeaponBuildOrder.Insert(1, recipe.UniqueID);
                    }
                }

                if (weapon.ammoRecipe != null)
                {
                    Add(weapon.ammoRecipe);
                }
            }

            isDirty = true;
            return true;
        }

        /// <summary>
        /// Get the number of instances of a supplied recipe that are in the player's permanent recipe collection.
        /// </summary>
        /// <param name="recipe">The recipe to count instances of.</param>
        /// <returns>The number of times the recipse appears in the persistent data.</returns>
        public int GetCount(IRecipe recipe)
        {
            return RecipeIds.Count(r => r == recipe.UniqueID);
        }

        internal bool Contains(IRecipe recipe)
        {
            return RecipeIds.Contains(recipe.UniqueID);
        }

        public bool isDirty { get; set; } // TODO: Need to wrap values above to automate setting this on change
    }
}