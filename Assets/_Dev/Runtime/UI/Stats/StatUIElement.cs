using RosgueWave.UI;

namespace RogueWave.GameStats
{
    public class StatUIElement : RogueWaveUIElement
    {
        public TMPro.TextMeshProUGUI label;
        public TMPro.TextMeshProUGUI value;

        GameStat m_stat;
        public GameStat stat
        {
            get { return m_stat; }
            set
            {
                m_stat = value;
                SetLabel(m_stat.displayName);
                SetValue(m_stat.GetValueAsString());
            }
        }

        public void SetLabel(string text)
        {
            label.text = text;
        }

        public void SetValue(string text)
        {
            value.text = text;
        }
    }
}
