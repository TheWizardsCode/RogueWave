using RogueWave;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizardsCode.CommandTerminal;

namespace WizardsCode.RogueWave.CommandTermiinal
{
    public class RecipeCommands
    {
        [RegisterCommand(Help = "List all recipes currentky in the players Permanent collection", MinArgCount = 0, MaxArgCount = 0)]
        public static void ListPermanentRecipes(CommandArg[] args)
        {
            RecipeManager.Initialise();
            List<IRecipe> recipes = new List<IRecipe>();
            foreach (string id in RogueLiteManager.persistentData.GetRecipeIDs()) {
                if (RecipeManager.TryGetRecipe(id, out IRecipe recipe))
                {
                    Terminal.Log(recipe.DisplayName);
                }
            }
        }

        [RegisterCommand(Help = "List all recipes currentky in the players Termporary collection", MinArgCount = 0, MaxArgCount = 0)]
        public static void ListTemporaryRecipes(CommandArg[] args)
        {
            RecipeManager.Initialise();
            List<IRecipe> recipes = new List<IRecipe>();
            foreach (IRecipe recipe in RogueLiteManager.runData.GetRecipes())
            {
                Terminal.Log(recipe.DisplayName);
            }
        }

        [RegisterCommand(Help = "List all recipes", MinArgCount = 0, MaxArgCount = 0)]
        public static void ListAllRecipes(CommandArg[] args)
        {
            RecipeManager.Initialise();
            IEnumerable<IGrouping<string, IRecipe>> groupedRecipes = RecipeManager.allRecipes.Values
                    .GroupBy(recipe => recipe.Category);

            DumpRecipeList(groupedRecipes);
        }

        [RegisterCommand(Help = "Add a specific recipe to the temporary collection. Recipe is identified by an index or name. If no recipe is identified in the parameters then a list of all recipes will be output to the terminal.", MinArgCount = 0, MaxArgCount = 1), ]
        public static void AddTemporaryRecipe(CommandArg[] args)
        {
            RecipeManager.Initialise();
            IEnumerable<IGrouping<string, IRecipe>> groupedRecipes = RecipeManager.allRecipes.Values
                    .GroupBy(recipe => recipe.Category);

            if (args.Length == 0)
            {
                Terminal.Log("You need to specify the recipe to add, avaialble recipes are:\n");
                DumpRecipeList(groupedRecipes);
                Terminal.Log("\nPlease run the command again specifying the recipe to add using the index number or name above.");
                return;
            }

            IRecipe recipe = null;
            if (int.TryParse(args[0].String, out int requestedIndex))
            {
                recipe = GetRecipe(requestedIndex, groupedRecipes);
            } else
            {
                recipe = GetRecipe(args[0].String, groupedRecipes);
            }

            if (recipe != null)
            {
                GameObject.FindAnyObjectByType<NanobotManager>().AddToRunRecipes(recipe);
                Terminal.Log($"Added {recipe.DisplayName} to temporary collection.");
            } 
            else
            {
                Terminal.LogError($"Selection is not valid. It should be an integer or a string matching one of the names above. You provided '{args[0]}'.");
            }
        }

        private static IRecipe GetRecipe(string filter, IEnumerable<IGrouping<string, IRecipe>> groupedRecipes)
        {
            foreach (var group in groupedRecipes)
            {
                var recipeDictionary = RecipeManager.allRecipes.Values
                    .GroupBy(recipe => recipe.DisplayName)
                    .ToDictionary(g => g.Key, g => g.First());

                return recipeDictionary.FirstOrDefault(kvp => kvp.Key.Contains(filter, StringComparison.OrdinalIgnoreCase)).Value;
            }

            return null;
        }

        private static IRecipe GetRecipe(int requestedIndex, IEnumerable<IGrouping<string, IRecipe>> groupedRecipes)
        {
            if (requestedIndex < 0 || requestedIndex >= groupedRecipes.SelectMany(group => group).Count())
            {
                Terminal.Log("Invalid index.\n");
                DumpRecipeList(groupedRecipes);
                Terminal.Log("\nPlease run the command again specifying the recipe to add using the index number or name above.");
                return null;
            }
            else
            {
                int index = 0;
                foreach (var group in groupedRecipes)
                {
                    var uniqueRecipes = group
                        .GroupBy(recipe => recipe.DisplayName)
                        .Select(g => new { Recipe = g.First(), Count = g.Count() })
                        .OrderBy(tuple => tuple.Recipe.DisplayName);

                    foreach (var tuple in uniqueRecipes)
                    {
                        if (index == requestedIndex)
                        {
                            return tuple.Recipe;
                        }
                        index++;
                    }
                }
            }

            return null;
        }

        [RegisterCommand(Help = "Add a specific recipe to the permanent collection. If no recipe is identified in the parameters then a list of all recipes will be output to the terminal", MinArgCount = 0, MaxArgCount = 1), ]
        public static void AddPermanentRecipe(CommandArg[] args)
        {
            RecipeManager.Initialise();
            IEnumerable<IGrouping<string, IRecipe>> groupedRecipes = RecipeManager.allRecipes.Values
                    .GroupBy(recipe => recipe.Category);

            if (args.Length == 0)
            {
                Terminal.Log("You need to specify the recipe to add, avaialble recipes are:\n");
                DumpRecipeList(groupedRecipes);
                Terminal.Log("\nPlease specify the recipe to add using the numbers above.");

                return;
            }

            IRecipe recipe = null;
            if (int.TryParse(args[0].String, out int requestedIndex))
            {
                recipe = GetRecipe(requestedIndex, groupedRecipes);
            }
            else
            {
                recipe = GetRecipe(args[0].String, groupedRecipes);
            }

            if (recipe != null)
            {
                //GameObject.FindAnyObjectByType<NanobotManager>().AddToRunRecipes(recipe);
                RogueLiteManager.persistentData.Add(recipe);
                Terminal.Log($"Added {recipe.DisplayName} to temporary collection.");
            }
            else
            {
                Terminal.LogError($"Selection is not valid. It should be an integer or a string matching one of the names above. You provided '{args[0]}'.");
            }
        }

        private static void DumpRecipeList(IEnumerable<IGrouping<string, IRecipe>> groupedRecipes)
        {
            int index = 0;

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
        }
    }
}
