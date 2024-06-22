#if UNITY_STANDALONE 
// Should other platforms use Json text files saved to disk?
#define SETTINGS_USES_JSON
#endif

using NeoFPS;
using UnityEngine;

namespace RogueWave
{
    [CreateAssetMenu(fileName = "FpsSettings_Playstyle", menuName = "Rogue Wave/Playstyle Settings")]
    public class FpsPlaystyleSettings : SettingsContext<FpsPlaystyleSettings>
    {
        protected override string contextName { get { return "Playstyle"; } }

        public override string displayTitle { get { return "Rogue Wave Playstyle Settings"; } }

        public override string tocName { get { return "Playstyle Settings"; } }

        public override string tocID { get { return "settings_playstyle"; } }

        [SerializeField, Tooltip("The difficulty multiplier. This is used to increase the difficulty of the game as the player. It impacts things like the total challenge rating of squads sent to attack a hiding player, resources gathered/needed and more."), Range(0.1f, 1f)]
        internal float m_DifficultyMultiplier = 0.4f;

        public float difficulty
        {
            get { return m_DifficultyMultiplier; }
            set { SetValue<float>(ref m_DifficultyMultiplier, value); }
        }

        protected override bool CheckIfCurrent()
        {
            return FpsSettings.playstyle == this;
        }
    }
}
