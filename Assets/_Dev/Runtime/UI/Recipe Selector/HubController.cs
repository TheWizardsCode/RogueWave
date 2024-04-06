using NeoFPS.SinglePlayer;
using NeoFPS;
using NeoSaveGames.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using System.Security.Permissions;
using log4net.Core;

namespace RogueWave.UI
{
    public class HubController : MonoBehaviour
    {
        [SerializeField, Tooltip("The scene to load when entering combat from this scene."), Scene]
        string m_CombatScene = string.Empty;

        [Header("UI Elements")]
        [SerializeField, Tooltip("The number of resources currently available to the player. If this is null then it is assumed that the resources should not be shown.")]
        private Text m_ResourcesText = null;
        [SerializeField, Tooltip("The text readout for the current game level number.")]
        private TMPro.TMP_Text m_GameLevelNumberText = null;
        [SerializeField, Tooltip("The text readout for the current players Nanobot level number.")]
        private TMPro.TMP_Text m_NanobotLevelNumberText = null;

        internal static List<IRecipe> permanentRecipes = new List<IRecipe>();
        internal static List<IRecipe> temporaryRecipes = new List<IRecipe>();

        private void OnGUI()
        {
            if (m_ResourcesText != null)
            {
                m_ResourcesText.text = RogueLiteManager.persistentData.currentResources.ToString();
            }

            if (m_GameLevelNumberText != null)
            {
                m_GameLevelNumberText.text = (RogueLiteManager.persistentData.currentGameLevel + 1).ToString();
            }

            if (m_NanobotLevelNumberText != null)
            {
                m_NanobotLevelNumberText.text = (RogueLiteManager.persistentData.currentNanobotLevel + 1).ToString();
            }

            permanentRecipes.Clear();
            foreach (string recipeID in RogueLiteManager.persistentData.RecipeIds)
            {
                if (RecipeManager.TryGetRecipe(recipeID, out IRecipe recipe))
                {
                    permanentRecipes.Add(recipe);
                }
            }

            temporaryRecipes.Clear();
            temporaryRecipes.AddRange(RogueLiteManager.runData.Recipes);
            temporaryRecipes.RemoveAll(recipe => permanentRecipes.Contains(recipe));
        }

        public void QuitSelectionUI()
        {
            if (FpsSoloCharacter.localPlayerCharacter == null)
            {
                if (!string.IsNullOrWhiteSpace(m_CombatScene))
                {
                    NeoSceneManager.LoadScene(m_CombatScene);
                }
            }
            else
            {
                NeoFpsInputManager.captureMouseCursor = true;
                Destroy(gameObject);
            }
        }

#if UNITY_EDITOR
        [SerializeField, Tooltip("To add recipes to the character for testing in game drop them here and click the Add Text Recipes button.")]
        AbstractRecipe[] testRecipes;

        [Button("Add test recipes")]
        void AddTestRecipes()
        {
            foreach (AbstractRecipe recipe in testRecipes)
            {
                RogueLiteManager.persistentData.Add(recipe);
            }
        }
#endif
    }
}
