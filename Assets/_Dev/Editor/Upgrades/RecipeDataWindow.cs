using UnityEditor;
using UnityEngine;

namespace RogueWave.Editor
{
    public class RecipeDataWindow : EditorWindow
    {
        Vector2 scrollPosition;
        private AbstractRecipe[] recipes;
        private bool recipeEdited;

        // Create a menu option to display a summary of all recipe data
        [MenuItem("Tools/Rogue Wave/Recipe Data", priority = 100)]
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

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Lvl", GUILayout.Width(20));
            EditorGUILayout.LabelField("Display name", GUILayout.Width(300));
            EditorGUILayout.LabelField(new GUIContent("Cost", "Base buy cost of the recipe"), GUILayout.Width(30));
            EditorGUILayout.LabelField(new GUIContent("Wght", "Base weight of the recipe"), GUILayout.Width(30));
            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField(new GUIContent("O", "Is this recipe currently owned?"), GUILayout.Width(10));
                EditorGUILayout.LabelField(new GUIContent("Buy", "Current buy cost of the recipe"), GUILayout.Width(30));
                EditorGUILayout.LabelField(new GUIContent("W", "Current weight of the recipe"), GUILayout.Width(30));
            }
            EditorGUILayout.EndHorizontal();

            foreach (AbstractRecipe recipe in recipes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();

                recipe.Level = EditorGUILayout.IntField(recipe.Level, GUILayout.Width(20));
                recipe.name = EditorGUILayout.TextArea(recipe.name, GUILayout.Width(300));
                if (GUILayout.Button("Edit", GUILayout.Width(40)))
                {
                    Selection.activeObject = recipe;
                }
                recipe.baseBuyCost = EditorGUILayout.IntField(recipe.baseBuyCost, GUILayout.Width(40));
                recipe.baseWeight = EditorGUILayout.FloatField(recipe.baseWeight, GUILayout.Width(40));

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