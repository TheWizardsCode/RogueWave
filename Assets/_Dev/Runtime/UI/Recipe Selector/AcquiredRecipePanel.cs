using System.Collections;
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

            if (recipes.Count != transform.childCount)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }

                foreach (IRecipe recipe in recipes.Distinct())
                {
                    RecipeCard card = Instantiate(recipeCardPrototype, transform);
                    if (recipe.IsStackable)
                    {
                        card.stackSize = recipes.FindAll(r => r.UniqueID == recipe.UniqueID).Count;
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
            }
        }
    }
}