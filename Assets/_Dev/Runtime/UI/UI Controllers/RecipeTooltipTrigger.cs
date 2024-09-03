using ModelShark;
using RogueWave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// RecipeTooltipConfiguration is placed on an UI object displaying a recipe.
    /// The main controller will call `Initialize` to set the recipe to display.
    /// </summary>
    public class RecipeTooltipTrigger : TooltipTrigger
    {
        private IRecipe recipe = null;

        /// <summary>
        /// Initialize the tooltip with the recipe to display.
        /// This will configure the details in the tooltip, such as title, description, stack size, dependencies and unlocks.
        /// </summary>
        /// <param name="recipe">The recipe this tooltip is to display</param>
        /// <param name="isOffer">True if this is a tooltip for a recipe being offered to the player, false if this is a recipe the player already has.</param>
        internal void Initialize(IRecipe recipe, bool isOffer)
        {
            base.Initialize();

            this.recipe = recipe;

            SetText("Title", recipe.DisplayName);
            SetText("Description", recipe.Description);

            int count = RogueLiteManager.GetTotalCount(recipe);
            if (isOffer)
            {
                count++;
            }

            if (recipe.IsStackable)
            {
                SetText("Stack", $"({count} of {recipe.MaxStack})");
            }
            else
            {
                SetText("Stack", " ");
            }

            AddDependenciesToTooltip();
            AddUnlocksToTooltip();
        }

        private void AddDependenciesToTooltip()
        {
            StringBuilder sb = new StringBuilder();
            if (recipe.Dependencies.Length > 0)
            {
                foreach (var dep in recipe.Dependencies)
                {
                    sb.AppendLine(dep.DisplayName);
                }
                SetText("Dependencies", sb.ToString());
            }
            else
            {
                SetText("Dependencies", "None");
            }
        }

        private void AddUnlocksToTooltip()
        {
            // OPTIMIZATION: configure this at build time and cache in the recipe object
            StringBuilder sb = new StringBuilder();
            foreach (IRecipe candidate in RecipeManager.allRecipes.Values)
            {
                if (candidate.Dependencies.Length > 0)
                {
                    foreach (var dep in candidate.Dependencies)
                    {
                        if (dep == recipe)
                        {
                            if (candidate.Dependencies.Length > 1)
                            {
                                sb.AppendLine($"{candidate.DisplayName} (partial)");
                            }
                            else
                            {
                                sb.AppendLine(candidate.DisplayName);
                            }

                            break;
                        }
                    }
                }
            }
            if (sb.Length > 0)
            {
                SetText("Unlocks", sb.ToString());
            }
            else
            {
                SetText("Unlocks", "None");
            }
        }
    }
}
