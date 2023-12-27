using Codice.CM.Common.Replication;
using NeoFPS;
using NeoSaveGames.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Playground
{
    public class HubSceneUI : MonoBehaviour
    {
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
        }

        void OnGUI()
        {
            if (m_Data != null)
            {
                GUILayout.BeginArea(new Rect(10f, 10f, 300f, 1000f));

                for (int i = offers.Count - 1; i >= 0; i--)
                {
                    IRecipe offer = offers[i];

                    if (RogueLiteManager.runData.currentResources >= offer.Cost && GUILayout.Button($"{offer.DisplayName} ({offer.Cost} resources)"))
                    {
                        m_Data.Add(offer);
                        offers.Remove(offer);
                        RogueLiteManager.runData.currentResources -= offer.Cost;
                    }
                }

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
    }
}