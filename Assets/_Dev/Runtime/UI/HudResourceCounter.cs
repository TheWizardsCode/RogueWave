using NeoFPS;
using UnityEngine;
using UnityEngine.UI;
using static NeoFPS.HealthDelegates;

namespace Playground
{
	public class HudResourceCounter : PlayerCharacterHudBase
    {
		[SerializeField, Tooltip("The text readout for the current characters resources.")]
		private Text m_ResourcesText = null;

        private NanobotManager nanobotManager = null;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (nanobotManager != null)
                nanobotManager.onResourcesChanged -= OnResourcesChanged;
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
			if (nanobotManager != null)
                nanobotManager.onResourcesChanged -= OnResourcesChanged;

            if (character as Component != null)
                nanobotManager = character.GetComponent<NanobotManager>();
            else
                nanobotManager = null;

            if (nanobotManager != null)
			{
                nanobotManager.onResourcesChanged += OnResourcesChanged;
				OnResourcesChanged(0f, nanobotManager.resources);
				gameObject.SetActive (true);
			}
			else
                gameObject.SetActive (false);
		}

		protected virtual void OnResourcesChanged (float from, float to)
        {
            m_ResourcesText.text = ((int)to).ToString ();
		}
	}
}