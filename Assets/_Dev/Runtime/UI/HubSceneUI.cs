using Codice.CM.Common.Replication;
using NeoFPS;
using NeoSaveGames.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Playground
{
    public class HubSceneUI : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField, Tooltip("The number of resources currently available to the player.")]
        private Text m_ResourcesText = null;

        [Header("Start Run")]

        [SerializeField] private Button m_StartRunButton = null;
        [SerializeField] private string m_CombatScene = string.Empty;

        [Header("Shared")]

        [SerializeField] private Color m_DefaultColour = Color.black;
        [SerializeField] private Color m_GoodColour = Color.green;
        [SerializeField] private Color m_BadColour = Color.red;

        [Header("Move Speed Example")]

        [SerializeField] private Text m_MoveSpeedMultText = null;
        [SerializeField] private Button m_MoveSpeedMultButton = null;
        [SerializeField] private float m_MoveSpeedMultIncrement = 0.05f;

        [SerializeField] private Text m_MoveSpeedPreText = null;
        [SerializeField] private Button m_MoveSpeedPreButton = null;
        [SerializeField] private float m_MoveSpeedPreIncrement = 0.05f;

        [SerializeField] private Text m_MoveSpeedPostText = null;
        [SerializeField] private Button m_MoveSpeedPostButton = null;
        [SerializeField] private float m_MoveSpeedPostIncrement = 0.05f;

        private WeaponPickupRecipe[] weaponRecipes;

        private RogueLitePersistentData m_Data = null;
        private List<IRecipe> offers;

        private Texture2D optionsBackground;

        void Start()
        {
            NeoFpsInputManager.captureMouseCursor = false;

            m_Data = RogueLiteManager.persistentData;

            offers = RecipeManager.GetOffers(3);

            if (m_MoveSpeedMultButton != null)
                m_MoveSpeedMultButton.onClick.AddListener(OnClickMoveSpeedMultiplier);
            if (m_MoveSpeedPreButton != null)
                m_MoveSpeedPreButton.onClick.AddListener(OnClickMoveSpeedPreAdd);
            if (m_MoveSpeedPostButton != null)
                m_MoveSpeedPostButton.onClick.AddListener(OnClickMoveSpeedPostAdd);

            RefreshMoveSpeedMultiplierText();
            RefreshMoveSpeedPreAddText();
            RefreshMoveSpeedPostAddText();

            if (m_StartRunButton != null)
                m_StartRunButton.onClick.AddListener(OnClickStartRun);

            optionsBackground = MakeTex(2, 2, new Color(0.4f, 0.4f, 0.4f, 0.5f));
        }

        void OnGUI()
        {
            m_ResourcesText.text = RogueLiteManager.runData.currentResources.ToString();

            if (m_Data != null)
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

                for (int i = numberOfOffers - 1; i >= 0; i--)
                {
                    IRecipe offer = offers[i];
                    if (RogueLiteManager.runData.currentResources < offer.Cost)
                    {
                        continue;
                    }

                    GUIStyle optionStyle = new GUIStyle(GUI.skin.box);
                    optionStyle.normal.background = optionsBackground;

                    GUIStyle descriptionStyle = new GUIStyle(EditorStyles.textArea);
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
                    if (GUILayout.Button($"{offer.DisplayName} ({offer.Cost} resources)", myButtonStyle, GUILayout.Height(50))) // Make the button taller
                    {
                        m_Data.Add(offer);
                        offers.Remove(offer);
                        RogueLiteManager.runData.currentResources -= offer.Cost;
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndArea();
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