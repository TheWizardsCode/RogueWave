using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RogueWave.Editor
{
    public class RecipeDataWindow : EditorWindow
    {
        private string filter;
        private bool filterIsValid;
        Vector2 scrollPosition;
        private AbstractRecipe[] recipes;
        private bool recipeEdited;

        // Create a menu option to display a summary of all recipe data
        [MenuItem("Tools/Rogue Wave/Recipe Data", priority = 105)]
        static void Init()
        {
            RecipeDataWindow window = (RecipeDataWindow)GetWindow(typeof(RecipeDataWindow));
            window.Show();
        }

        private void OnEnable()
        {
            UpdateRecipeList();
        }

        private void UpdateRecipeList()
        {
            recipes = Resources.LoadAll<AbstractRecipe>("");
            System.Array.Sort(recipes, (x, y) =>
            {
                int levelComparison = x.Level.CompareTo(y.Level);
                if (levelComparison == 0)
                {
                    return y.weight.CompareTo(x.weight);
                }
                return levelComparison;
            });
        }

        enum Status { Any, Invalid, Valid }
        Status filterStatus = Status.Any;

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            filter = EditorGUILayout.TextField("Filter", filter, GUILayout.Width(600));
            filterStatus = (Status)EditorGUILayout.EnumPopup(filterStatus);
            EditorGUILayout.EndHorizontal();

            List<AbstractRecipe> filteredRecipes = new List<AbstractRecipe>();
            foreach (AbstractRecipe recipe in recipes)
            {
                bool isValid = recipe.Validate(out string statusMsg);
                if ((filterStatus == Status.Invalid && isValid) || (filterStatus == Status.Valid && !isValid))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(filter) && !recipe.name.ToLower().Contains(filter.ToLower()) && isValid == filterIsValid)
                {
                    continue;
                }

                filteredRecipes.Add(recipe);
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Showing {filteredRecipes.Count} of {recipes.Length}");
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Lvl", GUILayout.Width(20));
            EditorGUILayout.LabelField("Display name", GUILayout.Width(300));
            EditorGUILayout.LabelField(new GUIContent("Cost", "Base buy cost of the recipe"), GUILayout.Width(40));
            EditorGUILayout.LabelField(new GUIContent("Wght", "Base weight of the recipe"), GUILayout.Width(30));
            EditorGUILayout.LabelField(new GUIContent("BT", "Build time for this recipe."), GUILayout.Width(30));
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField(new GUIContent("O", "Is this recipe currently owned?"), GUILayout.Width(10));
                EditorGUILayout.LabelField(new GUIContent("Buy", "Current buy cost of the recipe"), GUILayout.Width(30));
                EditorGUILayout.LabelField(new GUIContent("W", "Current weight of the recipe"), GUILayout.Width(30));
            }
            EditorGUILayout.EndHorizontal();

            foreach (AbstractRecipe recipe in filteredRecipes)
            {
                if (recipe.Validate(out string statusMsg))
                {
                    GUI.backgroundColor = Color.green;
                    statusMsg = "Click to select the recipe.";
                }
                else
                {
                    GUI.backgroundColor = Color.red;
                    statusMsg += "\nClick to select the recipe.";
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();

                recipe.Level = EditorGUILayout.IntField(recipe.Level, GUILayout.Width(20));
                if (GUILayout.Button(new GUIContent(recipe.DisplayName, statusMsg), GUILayout.Width(300)))
                {
                    EditorGUIUtility.PingObject(recipe);
                    Selection.activeObject = recipe;
                }
                recipe.baseBuyCost = EditorGUILayout.IntField(recipe.baseBuyCost, GUILayout.Width(40));
                recipe.baseWeight = EditorGUILayout.FloatField(recipe.baseWeight, GUILayout.Width(30));
                recipe.TimeToBuild = EditorGUILayout.FloatField(recipe.TimeToBuild, GUILayout.Width(30));

                if (Application.isPlaying)
                {
                    if (RogueLiteManager.runData.Contains(recipe))
                    {
                        EditorGUILayout.LabelField("Y", GUILayout.Width(10));
                        EditorGUILayout.LabelField(recipe.BuyCost.ToString(), GUILayout.Width(40));
                        EditorGUILayout.LabelField(recipe.weight.ToString(), GUILayout.Width(40));
                    } 
                    else
                    {
                        EditorGUILayout.LabelField("N", GUILayout.Width(10));
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    recipeEdited = true;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (recipeEdited 
                && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Tab))
            {
                AssetDatabase.SaveAssets();
                UpdateRecipeList();
                recipeEdited = false;
            }
        }
    }
}