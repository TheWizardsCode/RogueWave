using NeoFPS.Samples;
using NeoSaveGames.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RogueWave
{
	public class CreateNewProfilePanel : MenuPanel
    {
        [SerializeField] private InputField m_InputField = null;
        [SerializeField] private MultiInputButton m_CreateButton = null;

        public override void Initialise (BaseMenu menu)
		{
			base.Initialise (menu);

            m_CreateButton.onClick.AddListener(OnClickCreateProfile);
            m_CreateButton.interactable = false;

            m_InputField.onValueChanged.AddListener(OnInputFieldChanged);
        }

        private void OnInputFieldChanged(string arg0)
        {
            m_CreateButton.interactable = CheckNameIsValid(m_InputField.text);
            m_CreateButton.RefreshInteractable();
        }

        bool CheckNameIsValid(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return false;

            return true;
        }

        private void OnClickCreateProfile()
        {
            RogueLiteManager.CreateNewProfile(m_InputField.text);
            NeoSceneManager.LoadScene(RogueLiteManager.instance.reconstructionScene);
        }
	}
}