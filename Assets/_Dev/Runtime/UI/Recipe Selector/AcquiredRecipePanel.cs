using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RogueWave.UI
{
    public class AcquiredRecipePanel : MonoBehaviour
    {
        enum AcquisitionType
        {
            Permanent,
            Temporary
        }

        [Header("UI")]
        [SerializeField, Tooltip("The prototype object to use for displaying acquired recipes.")]
        RecipeCard recipeCardPrototype;
        [SerializeField, Tooltip("The kind of recipes to display.")]
        AcquisitionType acquisitionType;

        private IReadOnlyList<IRecipe> recipes = new List<IRecipe>();

        private void OnGUI()
        {
            switch (acquisitionType)
            {
                case AcquisitionType.Permanent:
                    recipes = HubController.permanentRecipes;
                    break;
                case AcquisitionType.Temporary:
                    recipes = HubController.temporaryRecipes;
                    break;
            }

            if ((acquisitionType == AcquisitionType.Permanent && HubController.isPermanentRecipesDirty)
                || (acquisitionType == AcquisitionType.Temporary && HubController.isTemporryRecipesDirty))
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }

                IEnumerable<IRecipe> recipesDistinct = recipes.Distinct();
                foreach (IRecipe recipe in recipesDistinct)
                {
                    RecipeCard card = Instantiate(recipeCardPrototype, transform);
                    if (recipe.IsStackable)
                    {
                        card.stackSize = HubController.permanentRecipes.Count(r => r.UniqueID == recipe.UniqueID);
                        card.stackSize += HubController.temporaryRecipes.Count(r => r.UniqueID == recipe.UniqueID);
                    }
                    switch (acquisitionType)
                    {
                        case AcquisitionType.Permanent:
                            card.cardType = RecipeCard.RecipeCardType.AcquiredPermanentMini;
                            break;
                        case AcquisitionType.Temporary:
                            card.cardType = RecipeCard.RecipeCardType.AcquiredTemporaryMini;
                            break;
                    }
                    card.recipe = recipe;
                }

                if (acquisitionType == AcquisitionType.Permanent)
                {
                    HubController.isPermanentRecipesDirty = false;
                } else
                {
                    HubController.isTemporryRecipesDirty = false;
                }
            }
        }
    }
}