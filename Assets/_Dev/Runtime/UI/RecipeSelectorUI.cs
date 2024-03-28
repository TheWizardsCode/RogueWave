using NaughtyAttributes;
using NeoFPS;
using NeoFPS.SinglePlayer;
using NeoSaveGames.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace RogueWave
{
    /// <summary>
    /// Show a UI that allows the player to select a recipe to build.
    /// This UI is designed to be run either Pre Spawn or during a run (post spawn).
    /// </summary>
    public class RecipeSelectorUI : MonoBehaviour
    {
        [Header("Behaviour")]
        [SerializeField, Tooltip("If true then the selector will be opened as soon as it is instantiated. If false a call to `ChooseRecipe` is required to open the selector.")]
        bool isOpen = false;
        [SerializeField, Tooltip("If true then selections will be recorded in persistent data and will survive between runs. If false then the selection will be recorded in run data and lost upon death.")]
        private bool m_MakePersistentSelections = true;
        [SerializeField, Tooltip("If true then the player will be given the item as well as the recipe. If they are given the item they will be charged for it, and it will be available immediately.")]
        private bool m_buildItem = false;
        [SerializeField, Tooltip("The number of offers that should be shown to the plauer. This could be modified by the game situation.")]
        int m_NumberOfOffers = 3;
        [SerializeField, Tooltip("The maximum number of selections that can be purchased in a single trip to the store. This could be modified by the game situation.")]
        int m_AllowedNumberOfPurchases = 3;

        [Header("Resources")]
        [SerializeField, Tooltip("The number of resources currently available to the player. If this is null then it is assumed that the resources should not be shown.")]
        private Text m_ResourcesText = null;
        [SerializeField, Tooltip("A message informing the player that they do not have enough resourcwe sot build an upgrade. If this is null then it is assumed no message is needed.")]
        private RectTransform m_NotEnoughResourcesMessage = null;

        [Header("Start Run")]
        [SerializeField] private string m_CombatScene = string.Empty;

        [Header("Shared")]

        [SerializeField] private Color m_DefaultColour = Color.black;
        [SerializeField] private Color m_GoodColour = Color.green;
        [SerializeField] private Color m_BadColour = Color.red;

        private List<IRecipe> offers;

        private Texture2D optionsBackground;
        private Texture2D acquiredBackground;
        private int m_SelectionCount;
        List<IRecipe> permanentRecipes = new List<IRecipe>();

        private NanobotManager nanobotManager {
            get
            {
                if (FpsSoloCharacter.localPlayerCharacter != null)
                {
                    return FpsSoloCharacter.localPlayerCharacter.GetComponent<NanobotManager>();
                } else
                {
                    return null;
                }
            }
        }

        void Start()
        {
            int weapons = 0;
            if (RogueLiteManager.persistentData.runNumber == 0)
            {
                weapons = 1;
            }

            offers = RecipeManager.GetOffers(m_NumberOfOffers, weapons);

            if (FpsSoloCharacter.localPlayerCharacter != null && offers.Count == 0)
            {
                QuitSelectionUI();
            }
            else if (FpsSoloCharacter.localPlayerCharacter == null)
            {
                NeoFpsInputManager.captureMouseCursor = false;
            }

            optionsBackground = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.5f));
            acquiredBackground = MakeTex(2, 2, new Color(0f, 0.2f, 0f, 0.5f));

        }

        void OnGUI()
        {
            if (isOpen == false || offers == null)
                return;

            if (m_ResourcesText != null)
            {
                m_ResourcesText.text = RogueLiteManager.persistentData.currentResources.ToString();
            }

            int numberOfOffers = offers.Count;

            float buttonHeight = Screen.height * 0.05f;
            ActionButtonsGUI(10, buttonHeight);

            float heightOffset = buttonHeight * 1.5f;
            float cardHeight = Screen.height * 0.40f;
            OfferCardsGUI(numberOfOffers, heightOffset, cardHeight);

            float upgradesHeight = Screen.height * 0.2f;
            RunUpgradesGUI(buttonHeight + cardHeight + 50, upgradesHeight);
            PermanentUpgradesGUI(buttonHeight + cardHeight + upgradesHeight + 100, upgradesHeight);
        }

        private void PermanentUpgradesGUI(float heightOffset, float targetHeight)
        {
            IRecipe recipe;
            permanentRecipes.Clear();
            foreach (string id in RogueLiteManager.persistentData.RecipeIds)
            {
                if (RecipeManager.TryGetRecipeFor(id, out recipe))
                {
                    permanentRecipes.Add(recipe);
                }
            }

            UpgradesGUI("Current Permanent Upgrades", permanentRecipes, heightOffset, targetHeight);
        }

        private void RunUpgradesGUI(float heightOffset, float targetHeight)
        {
            UpgradesGUI("Current Run Upgrades", RogueLiteManager.runData.Recipes.Except(permanentRecipes).ToList(), heightOffset, targetHeight);
        }

        private void UpgradesGUI(string label, List<IRecipe> recipes, float heightOffset, float targetHeight)
        {
            float targetWidth = Screen.width * 0.9f;
            float imageWidth = targetWidth * 0.06f;
            float imageHeight = imageWidth;

            GUIStyle sectionBoxStyle = new GUIStyle(GUI.skin.box);
            sectionBoxStyle.normal.background = acquiredBackground;

            GUILayout.BeginArea(new Rect((Screen.width - targetWidth) / 2, Screen.height - targetHeight - heightOffset, targetWidth, targetHeight), sectionBoxStyle);
            {
                GUIStyle headingStyle = new GUIStyle(GUI.skin.label);
                headingStyle.fontSize = 25;

                GUILayout.Label(label, headingStyle);

                GUILayout.BeginHorizontal(GUILayout.Width(targetWidth), GUILayout.Height(targetHeight - headingStyle.lineHeight));
                {
                    GUILayout.FlexibleSpace();

                    foreach (IRecipe recipe in recipes)
                    {
                        GUILayout.BeginVertical();
                        {
                            GUILayout.FlexibleSpace();

                            GUILayout.Box(recipe.HeroImage, GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
                            if (recipe.IsStackable)
                            {
                                GUILayout.Label($"{recipe.DisplayName} ({RogueLiteManager.persistentData.GetCount(recipe)} of {recipe.MaxStack})");
                            }
                            else
                            {
                                GUILayout.Label(recipe.DisplayName);
                            }

                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndVertical();
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        private void OfferCardsGUI(int numberOfOffers, float heightOffset, float targetHeight)
        {
            float targetWidth = Screen.width * 0.9f;

            float cardWidth = Mathf.Min((targetWidth * 0.9f) / numberOfOffers, (targetWidth * 0.9f) / 3);
            float imageWidth = cardWidth * 0.6f;
            float imageHeight = imageWidth;

            GUILayout.BeginArea(new Rect((Screen.width - targetWidth) / 2, Screen.height - targetHeight - heightOffset, targetWidth, targetHeight));
            {
                GUILayout.BeginHorizontal(GUILayout.Width(targetWidth), GUILayout.Height(targetHeight));
                {
                    GUILayout.FlexibleSpace();

                    if (m_NotEnoughResourcesMessage != null)
                    {
                        m_NotEnoughResourcesMessage.gameObject.SetActive(true);
                    }

                    for (int i = numberOfOffers - 1; i >= 0; i--)
                    {
                        IRecipe offer = offers[i];
                        if (RogueLiteManager.persistentData.currentResources < offer.BuyCost)
                        {
                            continue;
                        }

                        if (m_NotEnoughResourcesMessage != null)
                        {
                            m_NotEnoughResourcesMessage.gameObject.SetActive(false);
                        }

                        GUIStyle optionStyle = new GUIStyle(GUI.skin.box);
                        optionStyle.normal.background = optionsBackground;

                        GUIStyle descriptionStyle = new GUIStyle(GUI.skin.textArea);
                        descriptionStyle.fontSize = 16;
                        descriptionStyle.alignment = TextAnchor.MiddleCenter;
                        descriptionStyle.normal.textColor = Color.grey;

                        GUIStyle selectionButtonStyle = new GUIStyle(GUI.skin.button);
                        selectionButtonStyle.fontSize = 25;

                        GUILayout.BeginVertical(optionStyle, GUILayout.Width(cardWidth));
                        GUILayout.FlexibleSpace();

                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        if (offer.HeroImage != null)
                        {
                            GUILayout.Box(offer.HeroImage, GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
                        } else
                        {
                            Debug.LogWarning($"No image for {offer.DisplayName}");
                            GUIStyle style = new GUIStyle(GUI.skin.box);
                            style.fontSize = 50;
                            GUILayout.Box(offer.DisplayName, style, GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
                        }

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        GUILayout.FlexibleSpace();

                        GUILayout.Label(offer.Description, descriptionStyle, GUILayout.MinHeight(60), GUILayout.MaxHeight(60));

                        GUILayout.FlexibleSpace();
                        string selectionButtonText;
                        if (m_MakePersistentSelections)
                        {
                            selectionButtonText = $"Buy {offer.DisplayName} for {offer.BuyCost} resources";
                        }
                        else
                        {
                            selectionButtonText = $"{offer.DisplayName}";
                        }
                        if (GUILayout.Button(selectionButtonText, selectionButtonStyle, GUILayout.Height(50)))
                        {
                            Select(offer);
                        }

                        GUILayout.EndVertical();
                    }

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        private void ActionButtonsGUI(float heightOffset, float targetHeight)
        {
            float targetWidth = Screen.width * 0.9f;

            GUILayout.BeginArea(new Rect((Screen.width - targetWidth) / 2, Screen.height - targetHeight - heightOffset, targetWidth, targetHeight));
            {
                GUIStyle startRunButtonStyle = new GUIStyle(GUI.skin.button);
                startRunButtonStyle.fontSize = 25;

                string startRunButtonText = "Back to the Action";
                if (FpsSoloCharacter.localPlayerCharacter == null)
                {
                    startRunButtonText = "Configure Loadout";
                }

                if (GUILayout.Button(startRunButtonText, startRunButtonStyle, GUILayout.Height(targetHeight)))
                {
                    QuitSelectionUI();
                }
            }
            GUILayout.EndArea();
        }

        private void Select(IRecipe offer)
        {
            m_SelectionCount++;

            if (nanobotManager != null)
            {
                RogueLiteManager.runData.Add(offer);
                nanobotManager.Add(offer);
            }

            if (m_MakePersistentSelections)
            {
                RogueLiteManager.persistentData.Add(offer);
                RogueLiteManager.persistentData.currentResources -= offer.BuyCost;
                RogueLiteManager.SaveProfile();
            }

            offers.RemoveAll(o => o == offer);

            if (m_buildItem)
            {
                StartCoroutine(nanobotManager.BuildRecipe(offer));
            }

            if (m_SelectionCount == m_AllowedNumberOfPurchases)
            {
                QuitSelectionUI();
            }
        }

        private void QuitSelectionUI()
        {
            isOpen = false;
            
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

        Color GetValueColour(float value, float standard, bool greater)
        {
            if (Mathf.Abs(value - standard) < 0.0001f)
                return m_DefaultColour;

            if (greater && value > standard)
                return m_GoodColour;
            if (!greater && value < standard)
                return m_GoodColour;

            return m_BadColour;
        }

        void RefreshValueText(Text uiText, float value, float standard, bool greater)
        {
            if (uiText != null)
            {
                uiText.text = value.ToString("F3");
                uiText.color = GetValueColour(value, standard, greater);
            }
        }

        Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
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