using RogueWave;
using System.Collections.Generic;
using System.Linq;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave.CommandTermiinal
{
    public class RecipeCommands
    {
        [RegisterCommand(Help = "List all recipes", MinArgCount = 0, MaxArgCount = 0)]
        public static void ListRecipes(CommandArg[] args)
        {
            RecipeManager.Initialise();
            IEnumerable<IGrouping<string, IRecipe>> groupedRecipes = RecipeManager.allRecipes.Values
                    .GroupBy(recipe => recipe.Category);

            foreach (var group in groupedRecipes)
            {
                Terminal.Log($"\n{group.Key}\n{new string('=', group.Key.Length)}");

                var uniqueRecipes = group
                    .GroupBy(recipe => recipe.DisplayName)
                    .Select(g => new { Recipe = g.First(), Count = g.Count() })
                    .OrderBy(tuple => tuple.Recipe.DisplayName);
                foreach (var tuple in uniqueRecipes)
                {
                    Terminal.Log($"{tuple.Recipe.DisplayName} - {tuple.Recipe.Description}");
                }
            }
        }

        [RegisterCommand(Help = "Add a specific recipe to the temporary collection. If no recipe is identified in the parameters then a list of all recipes will be output to the terminal", MinArgCount = 0, MaxArgCount = 1), ]
        public static void AddTemporaryRecipe(CommandArg[] args)
        {
            RecipeManager.Initialise();
            IEnumerable<IGrouping<string, IRecipe>> groupedRecipes = RecipeManager.allRecipes.Values
                    .GroupBy(recipe => recipe.Category);

            int index = 0;

            if (args.Length == 0)
            {
                Terminal.Log("You need to specify the recipe to add, avaialble recipes are:\n");

                foreach (var group in groupedRecipes)
                {
                    Terminal.Log($"\n{group.Key}\n{new string('=', group.Key.Length)}");

                    var uniqueRecipes = group
                        .GroupBy(recipe => recipe.DisplayName)
                        .Select(g => new { Recipe = g.First(), Count = g.Count() })
                        .OrderBy(tuple => tuple.Recipe.DisplayName);
                    foreach (var tuple in uniqueRecipes)
                    {
                        Terminal.Log($"{index} - {tuple.Recipe.DisplayName} - {tuple.Recipe.Description}");
                        index++;
                    }
                }

                Terminal.Log("\nPlease specify the recipe to add using the numbers above.");

                return;
            }

            IRecipe recipe;

            index = 0;
            foreach (var group in groupedRecipes)
            {
                var uniqueRecipes = group
                    .GroupBy(recipe => recipe.DisplayName)
                    .Select(g => new { Recipe = g.First(), Count = g.Count() })
                    .OrderBy(tuple => tuple.Recipe.DisplayName);
                foreach (var tuple in uniqueRecipes)
                {
                    if (index == args[0].Int)
                    {
                        recipe = tuple.Recipe;
                        RogueLiteManager.runData.Add(recipe);
                        Terminal.Log($"Added {recipe.DisplayName} to temporary collection.");
                        return;
                    }
                    index++;
                }
            }

            Terminal.LogError($"Index is not valid {args[0].Int}.");
        }

        [RegisterCommand(Help = "Add a specific recipe to the permanent collection. If no recipe is identified in the parameters then a list of all recipes will be output to the terminal", MinArgCount = 0, MaxArgCount = 1), ]
        public static void AddPermanentRecipe(CommandArg[] args)
        {
            RecipeManager.Initialise();
            IEnumerable<IGrouping<string, IRecipe>> groupedRecipes = RecipeManager.allRecipes.Values
                    .GroupBy(recipe => recipe.Category);

            int index = 0;

            if (args.Length == 0)
            {
                Terminal.Log("You need to specify the recipe to add, avaialble recipes are:\n");

                foreach (var group in groupedRecipes)
                {
                    Terminal.Log($"\n{group.Key}\n{new string('=', group.Key.Length)}");

                    var uniqueRecipes = group
                        .GroupBy(recipe => recipe.DisplayName)
                        .Select(g => new { Recipe = g.First(), Count = g.Count() })
                        .OrderBy(tuple => tuple.Recipe.DisplayName);
                    foreach (var tuple in uniqueRecipes)
                    {
                        Terminal.Log($"{index} - {tuple.Recipe.DisplayName} - {tuple.Recipe.Description}");
                        index++;
                    }
                }

                Terminal.Log("\nPlease specify the recipe to add using the numbers above.");

                return;
            }

            IRecipe recipe;

            index = 0;
            foreach (var group in groupedRecipes)
            {
                var uniqueRecipes = group
                    .GroupBy(recipe => recipe.DisplayName)
                    .Select(g => new { Recipe = g.First(), Count = g.Count() })
                    .OrderBy(tuple => tuple.Recipe.DisplayName);
                foreach (var tuple in uniqueRecipes)
                {
                    if (index == args[0].Int)
                    {
                        recipe = tuple.Recipe;
                        RogueLiteManager.persistentData.Add(recipe);
                        Terminal.Log($"Added {recipe.DisplayName} to permanent collection.");
                        return;
                    }
                    index++;
                }
            }

            Terminal.LogError($"Index is not valid {args[0].Int}.");
        }
    }
}
