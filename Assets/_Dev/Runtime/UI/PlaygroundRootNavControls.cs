﻿using NeoFPS.Samples;
using NeoSaveGames.SceneManagement;
using Playground;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Playground
{
    public class PlaygroundRootNavControls : MenuNavControls
    {
        [SerializeField, Tooltip("The continue game button (disable if no profiles exist)")]
        private MultiInputButton m_ContinueButton = null;
        [SerializeField, Tooltip("The new game button (default selection if no profiles exist)")]
        private MultiInputButton m_NewGameButton = null;
        [SerializeField, Tooltip("The select profile button (disable if no profiles exist)")]
        private MultiInputButton m_SelectProfileButton = null;
        [SerializeField, Tooltip("")]
        private CreateNewProfilePanel m_NewGamePanel = null;
        [SerializeField, Tooltip("")]
        private SelectProfilePanel m_SelectProfilePanel = null;

        public override void Show()
        {
            base.Show();

            bool selectedSet = false;
            bool validHubScene = true;// !string.IsNullOrWhiteSpace(RogueLiteManager.hubScene) && NeoSceneManager.isSceneValid(RogueLiteManager.hubScene);

            if (m_ContinueButton != null)
            {
                // Check if can continue (this can block so do it intermittently)
                if (validHubScene && RogueLiteManager.availableProfiles.Length > 0)
                {
                    EventSystem.current.SetSelectedGameObject(m_ContinueButton.gameObject);
                    selectedSet = true;
                    m_ContinueButton.onClick.AddListener(OnClickContinue);
                    m_ContinueButton.gameObject.SetActive(true);
                    m_ContinueButton.interactable = true;
                    m_ContinueButton.description = string.Format("Profile = <b>{0}</b>", RogueLiteManager.GetProfileName(0));
                }
                else
                {
                    m_ContinueButton.interactable = false;
                    m_ContinueButton.gameObject.SetActive(false);
                }
            }

            if (m_SelectProfileButton != null)
            {
                if (validHubScene && RogueLiteManager.availableProfiles.Length > 1)
                {
                    m_SelectProfileButton.onClick.AddListener(OnClickSelectProfile);
                    m_SelectProfileButton.gameObject.SetActive(true);
                    m_SelectProfileButton.interactable = true;
                }
                else
                {
                    m_SelectProfileButton.interactable = false;
                    m_SelectProfileButton.gameObject.SetActive(false);
                }
            }

            if (m_NewGameButton != null)
            {
                if (validHubScene)
                {
                    m_NewGameButton.onClick.AddListener(OnClickNewGame);
                    m_NewGameButton.interactable = true;

                    if (!selectedSet)
                    {
                        EventSystem.current.SetSelectedGameObject(m_NewGameButton.gameObject);
                        selectedSet = true;
                    }
                }
                else
                {
                    m_NewGameButton.interactable = false;
                }
            }

            // Reset navigation (in case buttons disabled)
            widgetList.ResetWidgetNavigation();
        }

        public override void Hide()
        {
            // Remove continue event listener
            //if (m_ContinueButton != null)
            //    m_ContinueButton.onClick.RemoveListener(OnClickContinue);
            //if (m_NewGameButton != null)
            //    m_ContinueButton.onClick.RemoveListener(OnClickNewGame);

            base.Hide();
        }

        public void OnClickSelectProfile()
        {
            menu.ShowPanel(m_SelectProfilePanel);
        }

        public void OnClickNewGame()
        {
            menu.ShowPanel(m_NewGamePanel);
        }

        public void OnClickContinue()
        {
            RogueLiteManager.LoadProfile(0);
            NeoSceneManager.LoadScene(RogueLiteManager.hubScene);
        }
    }
}