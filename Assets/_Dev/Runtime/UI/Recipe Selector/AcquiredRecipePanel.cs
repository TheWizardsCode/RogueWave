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

        int processedReciupes = 0;

        private List<IRecipe> recipes = new List<IRecipe>();

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

            IEnumerable<IRecipe> recipesDistinct = recipes.Distinct();
            if (HubController.isDirty)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(transform.GetChild(i).gameObject);
                }

                foreach (IRecipe recipe in recipesDistinct)
                {
                    RecipeCard card = Instantiate(recipeCardPrototype, transform);
                    if (recipe.IsStackable)
                    {
                        card.stackSize = HubController.permanentRecipes.FindAll(r => r.UniqueID == recipe.UniqueID).Count;
                        card.stackSize += HubController.temporaryRecipes.FindAll(r => r.UniqueID == recipe.UniqueID).Count;
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

                    HubController.isDirty = false;
                }
            }
        }
    }
}