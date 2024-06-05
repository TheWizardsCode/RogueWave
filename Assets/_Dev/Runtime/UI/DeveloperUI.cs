using NeoFPS;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NeoFPS.HealthDelegates;

namespace RogueWave
{
    public class DeveloperUI : MonoBehaviour
    {
        [SerializeField, Tooltip("The text element to display the version number in.")]
        TMP_Text versionText;

        protected void Awake()
        {
            string version = $"v{Application.version}";
#if UNITY_EDITOR
            version += "-Dev (Editor)";
#endif
            versionText.text = version;
        }
    }
}