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
        private IRecipe[] offers;

        void Start()
        {
            NeoFpsInputManager.captureMouseCursor = false;

            m_Data = RogueLiteManager.persistentData;

            RefreshAvailableRecipes();
            IRecipe[] offers = GetOffers(3);

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

        private void RefreshAvailableRecipes()
        {
            weaponRecipes = Resources.LoadAll<WeaponPickupRecipe>("Recipes/");
        }

        void OnGUI()
        {
            if (m_Data != null)
            {
                GUILayout.BeginArea(new Rect(10f, 10f, 300f, 1000f));

                foreach (IRecipe offer in offers)
                {
                    if (GUILayout.Button(offer.DisplayName))
                    {
                        m_Data.Add(offer);
                    }
                }

                GUILayout.EndArea();
            }   
        }

        /// <summary>
        /// Gets a number of upgrade recipes that can be offered to the player.
        /// </summary>
        /// <param name="quantity">The number of upgrades to offer.</param>
        /// <returns>An array of recipes that can be offered to the player.</returns>
        private IRecipe[] GetOffers(int quantity)
        {
            offers = new IRecipe[quantity];

            int count = weaponRecipes.Length;

            if (count < quantity)
            {
                Debug.Log("TODO: handle the situation where there are not enough recipes to offer.");
                return offers;
            }

            List<WeaponPickupRecipe> candidates = weaponRecipes.ToList<WeaponPickupRecipe>(); 
            for (int i = 0; i < quantity; i++)
            {
                int index = Random.Range(0, candidates.Count);
                offers[i] = candidates[index];
                candidates.RemoveAt(index);
            }

            return offers;
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