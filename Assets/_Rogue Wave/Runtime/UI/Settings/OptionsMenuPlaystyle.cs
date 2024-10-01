using NeoFPS;
using NeoFPS.Samples;
using UnityEngine;

namespace RogueWave
{
    public class OptionsMenuPlaystyle : OptionsMenuPanel
    {
        [SerializeField] private MultiInputSlider m_DifficultySlider = null;

        override public void Initialise(BaseMenu menu)
        {
            base.Initialise(menu);

            m_DifficultySlider.onValueChanged.AddListener(OnDifficultyChanged);
        }

        public void OnDifficultyChanged(int value)
        {
            FpsSettings.playstyle.difficulty = value / 100f;
        }

        protected override void SaveOptions()
        {
            FpsSettings.playstyle.Save();
        }

        override protected void ResetOptions()
        {
            if (m_DifficultySlider != null)
            {
                int current = Mathf.RoundToInt(FpsSettings.playstyle.difficulty * 100f);
                m_DifficultySlider.value = current;
            }
        }
    }
}
