using NeoFPS.SinglePlayer;
using NeoFPS;
using NeoSaveGames.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;
using NeoFPS.Samples;
using PlasticPipe.PlasticProtocol.Messages;
using System.Collections.ObjectModel;

namespace RogueWave.UI
{
    public class HubController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField, Tooltip("The number of resources currently available to the player. If this is null then it is assumed that the resources should not be shown.")]
        private Text m_ResourcesText = null;
        [SerializeField, Tooltip("The text readout for the current game level number.")]
        private TMPro.TMP_Text m_GameLevelNumberText = null;
        [SerializeField, Tooltip("The text readout for the current players Nanobot level number.")]
        private TMPro.TMP_Text m_NanobotLevelNumberText = null;
        [SerializeField, Tooltip("The button to move the player to the next screen on the way into combat.")]
        private MultiInputButton m_ContinueButton = null;

        static List<IRecipe> m_PermanentRecipes = new List<IRecipe>();
        static List<IRecipe> m_TemporaryRecipes = new List<IRecipe>();

        internal static bool isPermanentRecipesDirty = true;
        internal static bool isTemporryRecipesDirty = true;

        internal static ReadOnlyCollection<IRecipe> permanentRecipes
        {
            get { return m_PermanentRecipes.AsReadOnly(); }
        }

        internal static IReadOnlyList<IRecipe> temporaryRecipes
        {
            get { return m_TemporaryRecipes.AsReadOnly(); }
        }

        private void OnEnable()
        {
            m_ContinueButton.onClick.AddListener(QuitSelectionUI);
            NeoFpsInputManager.captureMouseCursor = false;

            GameLog.Info($"Entering Hub Scene with {RogueLiteManager.persistentData.currentResources} resources.");

            clearPermanentRecipes();
            foreach (string recipeID in RogueLiteManager.persistentData.RecipeIds)
            {
                if (RecipeManager.TryGetRecipe(recipeID, out IRecipe recipe))
                {
                    AddPermanentRecipe(recipe);
                }
            }

            ClearTemporaryRecipes();
            AddRangeOfTemporaryRecipes(RogueLiteManager.runData.Recipes);
            RemoveAllTemporaryRecipes(recipe => permanentRecipes.Contains(recipe));
        }

        private void OnDisable()
        {
            m_ContinueButton.onClick.RemoveListener(QuitSelectionUI);
            NeoFpsInputManager.captureMouseCursor = true;

            GameLog.Info($"Exiting Hub Scene with {RogueLiteManager.persistentData.currentResources} resources.");
        }

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

            if (RogueLiteManager.persistentData.WeaponBuildOrder.Count == 0)
            {
                m_ContinueButton.label = "Build a Weapon";
                m_ContinueButton.interactable = false;
            } else
            {
                m_ContinueButton.label = "Continue to Loadout Builder";
                m_ContinueButton.interactable = true;
            }
        }

        private void RemoveAllTemporaryRecipes(System.Predicate<IRecipe> match)
        {
            m_TemporaryRecipes.RemoveAll(match);
            isTemporryRecipesDirty = true;
        }

        private void AddRangeOfTemporaryRecipes(List<IRecipe> recipes)
        {
            m_TemporaryRecipes.AddRange(recipes);
            isTemporryRecipesDirty = true;
        }

        private void ClearTemporaryRecipes()
        {
            m_TemporaryRecipes.Clear();
            isTemporryRecipesDirty = true;
        }

        internal static void RemoveTemporaryRecipe(IRecipe recipe)
        {
            m_TemporaryRecipes.Remove(recipe);
            isTemporryRecipesDirty = true;
        }

        internal static void AddPermanentRecipe(IRecipe recipe)
        {
            m_PermanentRecipes.Add(recipe);
            isPermanentRecipesDirty = true;
        }

        private static void clearPermanentRecipes()
        {
            m_PermanentRecipes.Clear();
            isPermanentRecipesDirty = true;
        }

        public void QuitSelectionUI()
        {
            if (FpsSoloCharacter.localPlayerCharacter == null)
            {
                if (!string.IsNullOrWhiteSpace(RogueLiteManager.combatScene))
                {
                    NeoSceneManager.LoadScene(RogueLiteManager.combatScene);
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
