using NeoFPS.Samples;
using NeoSaveGames.SceneManagement;
using System.Collections.Generic;
using UnityEngine;

namespace Playground
{
	public class SelectProfilePanel : MenuPanel
    {
        [SerializeField] private MultiInputButton m_ProfileButtonPrototype = null;

        public override void Initialise(BaseMenu menu)
        {
            base.Initialise(menu);

            int count = RogueLiteManager.availableProfiles.Length;

            // Create selector buttons
            List<MultiInputButton> profileSelectors = new List<MultiInputButton>(count) { m_ProfileButtonPrototype };
            for (int i = 1; i < count; ++i)
            {
                var duplicate = Instantiate(m_ProfileButtonPrototype, m_ProfileButtonPrototype.transform.parent);
                profileSelectors.Add(duplicate);
            }

            // Set up selectors
            for (int i = 0; i < count; ++i)
            {
                int index = i;
                profileSelectors[i].label = RogueLiteManager.GetProfileName(i);
                profileSelectors[i].description = string.Empty; // Could make this number of runs etc
                profileSelectors[i].onClick.AddListener(() => { OnClickSelectProfile(index); });
            }
        }

        void OnClickSelectProfile(int index)
        {
            RogueLiteManager.LoadProfile(index);
            NeoSceneManager.LoadScene(RogueLiteManager.hubScene);
        }
    }
}