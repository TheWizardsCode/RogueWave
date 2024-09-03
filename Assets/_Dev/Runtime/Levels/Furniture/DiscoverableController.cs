using NeoFPS;
using RogueWave;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    public class DiscoverableController : DestructibleController
    {
        [SerializeField, Tooltip("The collection of pickup recipes from which the discovered item will be chosen. If none are valid for this player then the resources prefab will be used.")]
        List<AbstractRecipe> possibleDrops = null;

        protected override void OnIsAliveChanged(bool isAlive)
        {
            // Calculate weights for the possible drops, skipping any that the player already has the maximum number of.
            IItemRecipe recipe = null;
            WeightedRandom<IItemRecipe> weightedRandom = new WeightedRandom<IItemRecipe>();
            for (int i = possibleDrops.Count - 1; i >= 0; i--)
            {
                recipe = possibleDrops[i] as IItemRecipe;
                if (recipe == null)
                {
                    Debug.LogError($"{possibleDrops[i]} in a DiscoverableItem is not a valid recipe for a pickup item. Removing it.");
                    possibleDrops.RemoveAt(i);
                    continue;
                }

                if (RogueLiteManager.runData.GetCount(recipe) >= recipe.MaxStack)
                {
                    continue;
                }

                weightedRandom.Add(recipe, recipe.weight);
            }


            if (weightedRandom.Count > 0)
            {
                recipe = weightedRandom.GetRandom();
                resourcesPrefab = recipe.Item.GetComponent<Pickup>();
            }

            base.OnIsAliveChanged(isAlive);
        }
    }
}
