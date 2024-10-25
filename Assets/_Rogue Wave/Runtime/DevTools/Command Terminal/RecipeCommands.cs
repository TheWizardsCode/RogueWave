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
                if (requestedIndex < 0 || requestedIndex >= groupedRecipes.SelectMany(group => group).Count())
                {
                    Terminal.Log("Invalid index.\n");
                    DumpRecipeList(groupedRecipes);
                    Terminal.Log("\nPlease run the command again specifying the recipe to add using the index number or name above.");
                    return;
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
                                recipe = tuple.Recipe;
                                RogueLiteManager.runData.Add(recipe);
                                break;
                            }
                            index++;
                        }
                    }
                }
            } else
            {
                foreach (var group in groupedRecipes)
                {
                    var recipeDictionary = RecipeManager.allRecipes.Values
                        .GroupBy(recipe => recipe.DisplayName)
                        .ToDictionary(g => g.Key, g => g.First());

                    recipe = recipeDictionary.FirstOrDefault(kvp => kvp.Key.Contains(args[0].String, StringComparison.OrdinalIgnoreCase)).Value;

                    break;
                }
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

            IRecipe recipe;
            int index = 0;
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
                        RogueLiteManager.runData.Add(recipe); if (Application.isPlaying)
                        {
                            GameObject.FindAnyObjectByType<NanobotManager>().AddToRunRecipes(recipe);
                        }

                        Terminal.Log($"Added {recipe.DisplayName} to permanent collection.");
                        return;
                    }
                    index++;
                }
            }

            Terminal.LogError($"Index is not valid {args[0].Int}.");
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
