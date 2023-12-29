using NeoFPS;
using NeoFPS.SinglePlayer;
using NeoSaveGames.SceneManagement;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Playground
{
    /// <summary>
    /// Show a UI that allows the player to select a recipe to build.
    /// This UI is designed to be run either Pre Spawn or during a run (post spawn).
    /// </summary>
    public class RecipeSelectorUI : MonoBehaviour
    {
        [Header("Behaviour")]
        [SerializeField, Tooltip("If this UI is being shown before the run actually starts, e.g. in a Hub Scene, this should be set to true. If, however, the player has been spawned and this is being shown in a run then this should be false.")]
        private bool m_PreSpawn = true;
        [SerializeField, Tooltip("If true then selections will be recorded in persistent data and will survive between runs. If false then the selection will be recorded in run data and lost upon death.")]
        private bool m_MakePersistentSelections = true;
        [SerializeField, Tooltip("The number of offers that should be shown to the plauer. This could be modified by the game situation.")]
        int m_NumberOfOffers = 3;
        [SerializeField, Tooltip("The number of selections that can be made. This could be modified by the game situation.")]
        int m_NumberOfSelections = 1;

        [Header("Resources")]
        [SerializeField, Tooltip("The number of resources currently available to the player. If this is null then it is assumed that the resources should not be shown.")]
        private Text m_ResourcesText = null;
        [SerializeField, Tooltip("A message informing the player that they do not have enough resourcwe sot build an upgrade. If this is null then it is assumed no message is needed.")]
        private RectTransform m_NotEnoughResourcesMessage = null;

        [Header("Start Run")]

        [SerializeField] private Button m_StartRunButton = null;
        [SerializeField] private string m_CombatScene = string.Empty;

        [Header("Shared")]

        [SerializeField] private Color m_DefaultColour = Color.black;
        [SerializeField] private Color m_GoodColour = Color.green;
        [SerializeField] private Color m_BadColour = Color.red;

        private WeaponPickupRecipe[] weaponRecipes;

        private RogueLitePersistentData m_PersistentData = null;
        private RogueLiteRunData m_RunData = null;
        private List<IRecipe> offers;

        private Texture2D optionsBackground;
        private int m_SelectionCount;

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
            if (m_PreSpawn)
            {
                ChooseRecipe();
            }
        }

        /// <summary>
        /// Display the UI and start the choosing process.
        /// When a recipe is chosen the UI will be hidden and the chosen recipe will be added to the player's runData.
        /// </summary>
        /// <param name="numberOfOffers">The number of offers that should be presented.</param>
        /// <param name="numberOfSelections">The number of selections that can be made.</param>
        public void ChooseRecipe() { 
            NeoFpsInputManager.captureMouseCursor = false;

            m_PersistentData = RogueLiteManager.persistentData;
            m_RunData = RogueLiteManager.runData;
            
            offers = RecipeManager.GetOffers(m_NumberOfOffers);

            if (m_StartRunButton != null)
                m_StartRunButton.onClick.AddListener(OnClickStartRun);

            optionsBackground = MakeTex(2, 2, new Color(0.4f, 0.4f, 0.4f, 0.5f));
        }

        void OnGUI()
        {
            if (offers == null)
                return;

            if (m_ResourcesText != null)
            {
                m_ResourcesText.text = RogueLiteManager.persistentData.currentResources.ToString();
            }

            if (m_PersistentData != null)
            {
                int numberOfOffers = offers.Count;
                float screenWidth = Screen.width;
                float screenHeight = Screen.height;
                float targetWidth = screenWidth * 0.9f;
                float targetHeight = screenHeight * 0.5f;

                float cardWidth = (targetWidth * 0.8f) / numberOfOffers; 

                float imageHeight = targetHeight * 0.6f;
                float imageWidth = 640 * (imageHeight / 960f);

                GUILayout.BeginArea(new Rect((screenWidth - targetWidth) / 2, (screenHeight - targetHeight) / 2, targetWidth, targetHeight));

                GUILayout.BeginHorizontal(GUILayout.Width(targetWidth), GUILayout.Height(targetHeight));
                GUILayout.FlexibleSpace();

                if (m_NotEnoughResourcesMessage != null)
                {
                    m_NotEnoughResourcesMessage.gameObject.SetActive(true);
                }

                for (int i = numberOfOffers - 1; i >= 0; i--)
                {
                    IRecipe offer = offers[i];
                    if (RogueLiteManager.persistentData.currentResources < offer.Cost)
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

                    GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
                    myButtonStyle.fontSize = 25;

                    GUILayout.BeginVertical(optionStyle, GUILayout.Width(cardWidth));
                    GUILayout.FlexibleSpace();

                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();

                    GUILayout.Box(offer.HeroImage, GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.FlexibleSpace();

                    GUILayout.Label(offer.Description, descriptionStyle, GUILayout.MinHeight(60), GUILayout.MaxHeight(60));

                    GUILayout.FlexibleSpace();
                    string btnText;
                    if (m_MakePersistentSelections)
                    {
                        btnText = $"{offer.DisplayName} ({offer.Cost} resources)";
                    }
                    else
                    {
                        btnText = $"{offer.DisplayName}";
                    }
                    if (GUILayout.Button(btnText, myButtonStyle, GUILayout.Height(50))) // Make the button taller
                    {
                        Select(offer);
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
            }   
        }

        private void Select(IRecipe offer)
        {
            m_SelectionCount++;

            if (nanobotManager != null)
            {
                nanobotManager.Add(offer, m_MakePersistentSelections);
            }

            if (m_MakePersistentSelections)
            {
                RogueLiteManager.persistentData.Add(offer);
                RogueLiteManager.persistentData.currentResources -= offer.Cost;
            } else
            {
                RogueLiteManager.runData.Add(offer);
            }

            offers.Remove(offer);

            if (m_SelectionCount == m_NumberOfSelections) {
                if (m_PreSpawn)
                {
                    OnClickStartRun();
                }
                else
                {
                    NeoFpsInputManager.captureMouseCursor = true;
                    Destroy(gameObject);
                }
            }
        }

        private void OnClickStartRun()
        {
            if (!string.IsNullOrWhiteSpace(m_CombatScene))
                NeoSceneManager.LoadScene(m_CombatScene);
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

        /* TODO: Reinstate this functionality by adding recipes to do it
        public void RefreshMoveSpeedMultiplierText()
        {
            RefreshValueText(m_MoveSpeedMultText, m_Data.moveSpeedMultiplier, 1f, true);
        }

        public void RefreshMoveSpeedPreAddText()
        {
            RefreshValueText(m_MoveSpeedPreText, m_Data.moveSpeedPreAdd, 0f, true);
        }

        public void RefreshMoveSpeedPostAddText()
        {
            RefreshValueText(m_MoveSpeedPostText, m_Data.moveSpeedPostAdd, 0f, true);
        }

        private void OnClickMoveSpeedMultiplier()
        {
            m_Data.moveSpeedMultiplier += m_MoveSpeedMultIncrement;
            RefreshMoveSpeedMultiplierText();
            m_Data.isDirty = true;
        }

        private void OnClickMoveSpeedPreAdd()
        {
            m_Data.moveSpeedPreAdd += m_MoveSpeedPreIncrement;
            RefreshMoveSpeedPreAddText();
            m_Data.isDirty = true;
        }

        private void OnClickMoveSpeedPostAdd()
        {
            m_Data.moveSpeedPostAdd += m_MoveSpeedPostIncrement;
            RefreshMoveSpeedPostAddText();
            m_Data.isDirty = true;
        }
        */

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
    }
}