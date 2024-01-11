using NeoFPS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NeoFPS.HealthDelegates;

namespace Playground
{
    public class DeveloperUI : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The text element to display the version number in.")]
        TMP_Text versionText;

        private void Awake()
        {
            versionText.text = $"v{Application.version}";
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            
        }
    }
}