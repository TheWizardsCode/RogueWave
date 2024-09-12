using NeoFPS.Samples;
using NeoSaveGames.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using WizardsCode.RogueWave;

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

            string name = NameGenerator.GenerateName();
            while (!IsValidName(name))
            {
                name = NameGenerator.GenerateName();
            }
            m_InputField.text = name;
        }

        private void OnInputFieldChanged(string arg0)
        {
            m_CreateButton.interactable = IsValidName(m_InputField.text);
            m_CreateButton.RefreshInteractable();
        }

        bool IsValidName(string profileName)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(profileName))
                isValid = false;

            System.IO.FileInfo[] files = RogueLiteManager.availableProfiles;
            foreach (System.IO.FileInfo file in files)
            {
                if (file.Name == profileName + ".profileData")
                {
                    isValid = false;
                }
            }

            // test to see if it is a valif filename
            char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                if (profileName.Contains(c))
                {
                    isValid = false;
                }
            }
            
            if (isValid)
            {
                m_InputField.textComponent.color = Color.white;
                m_CreateButton.gameObject.SetActive(true);
            }
            else
            {
                m_InputField.textComponent.color = Color.red;
                m_CreateButton.gameObject.SetActive(false);
            }

            return isValid;
        }

        private void OnClickCreateProfile()
        {
            RogueLiteManager.CreateNewProfile(m_InputField.text);
            NeoSceneManager.LoadScene(RogueLiteManager.combatScene);
        }
	}
}