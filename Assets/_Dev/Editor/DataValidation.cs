using UnityEngine;

namespace RogueWave.EditorTools
{
    public class DataValidation
    {
        [UnityEditor.MenuItem("Tools/Rogue Wave/Data/Validate Recipes")]
        public static void ValidateRecipes()
        {
            Debug.Log("Validating recipes...");
            
            AbstractRecipe[] recipes = Resources.LoadAll<AbstractRecipe>("Recipes");
            foreach (AbstractRecipe recipe in recipes)
            {
                if (recipe.DisplayName.Length > 30)
                {
                    Debug.LogError($"{recipe} has a name that is > 30 characters.");
                }

                if (recipe.Description.Length > 160)
                {
                    Debug.LogError($"{recipe} has a description that is > 160 characters.");
                }

                if (recipe.Icon == null)
                {
                    Debug.LogError($"{recipe} has no icon.");
                }

                if (recipe.HeroImage == null)
                {
                    Debug.LogError($"{recipe} has no hero image.");
                }

                foreach (AbstractRecipe dependency in recipe.Dependencies)
                {
                    if (dependency == null)
                    {
                        Debug.LogError($"{recipe} has a null dependency.");
                    }
                }

                foreach (AbstractRecipe complement in recipe.Complements)
                {
                    if (complement == null)
                    {
                        Debug.LogError($"{recipe} has a null complement.");
                    }
                }

                if (recipe.IsStackable && recipe.MaxStack == 1)
                {
                    Debug.LogError($"{recipe} is stackable, but has a max stack of 1.");
                }

                if (recipe.nameClips.Length == 0)
                {
                    Debug.LogError($"{recipe} has no name clips.");
                }
            }

            Debug.Log("Completed Valdiation");
        }
    }
}