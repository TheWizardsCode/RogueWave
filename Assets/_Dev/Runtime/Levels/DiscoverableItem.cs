using NeoFPS;
using RogueWave;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// Discoverable items are items that can be found by the player in the game world. They are typically hidden in the game world and the player must explore to find them.
    /// This class is used to define the possible items that can be found and the probability of finding them.
    /// 
    /// <seealso cref="DiscoverableItemTile"/>
    /// </summary>
    public class DiscoverableItem : MonoBehaviour
    {
        [SerializeField, Tooltip("The collection of pickup recipes from which the discovered item will be chosen.")]
        List<AbstractRecipe> possibleDrops = null;
        [SerializeField, Tooltip("The fallback recipe to use if no valid recipes are found in the list of possibleDrops.")]
        AbstractRecipe fallbackRecipe = null;

        private void Awake()
        {
            if (possibleDrops == null || possibleDrops.Count == 0)
            {
                Debug.LogError("No possible drops have been defined for this DiscoverableItem.");
            }

            DestructibleController destructible = GetComponent<DestructibleController>();
            
            // Remove all possibleDrops that the player already has the maximum number of.
            IItemRecipe recipe = null;
            WeightedRandom<IItemRecipe> weightedRandom = new WeightedRandom<IItemRecipe>();
            for (int i = possibleDrops.Count - 1; i >= 0 ; i--)
            {
                recipe = possibleDrops[i] as IItemRecipe;
                if (recipe == null)
                {
                    Debug.LogError($"{possibleDrops[i]} in a DiscoverableItem is not a valid recipe for a pickup item. Removing it.");
                    possibleDrops.RemoveAt(i);
                    continue;
                }

                if (recipe.Level < RogueLiteManager.persistentData.currentNanobotLevel)
                {
                    possibleDrops.RemoveAt(i);
                }

                if (RogueLiteManager.runData.GetCount(recipe) >= recipe.MaxStack)
                {
                    possibleDrops.RemoveAt(i);
                }

                weightedRandom.Add(recipe, recipe.weight);
            }


            if (weightedRandom.Count == 0)
            {
                recipe = fallbackRecipe as IItemRecipe;
            }
            else
            {
                recipe = weightedRandom.GetRandom();
            }
            destructible.resourcesPrefab = recipe.Item.GetComponent<Pickup>();

            Destroy(this);
        }
    }
}
