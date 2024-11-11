using ModelShark;
using RosgueWave.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueWave.GameStats
{
    public class StatUIElement : RogueWaveUIElement
    {
        [SerializeField, Tooltip("The label to display the name of the stat.")]
        TextMeshProUGUI nameLabel;
        [SerializeField, Tooltip("The label to display the value of the stat.")]
        TextMeshProUGUI achievementLabel;
        [SerializeField, Tooltip("The icon to display the achievement.")]
        Image achievementIcon;
        [SerializeField, Tooltip("The label to display the value of the stat.")]
        TextMeshProUGUI valueLabel;
        [SerializeField, Tooltip("The tooltip trigger for this element.")]
        TooltipTrigger m_tooltipTrigger;

        string m_bodyText = string.Empty;

        IntGameStat m_stat;
        public IntGameStat stat
        {
            get { return m_stat; }
            set
            {
                m_stat = value;
                nameLabel.text = m_stat.displayName;
                valueLabel.text = m_stat.ToString();
                SetTooltipText();
            }
        }

        Achievement m_achievement;
        public Achievement achievement
        {
            get { return m_achievement; }
            set
            {
                m_achievement = value;
                achievementLabel.transform.parent.gameObject.SetActive(true);
                achievementLabel.text = $"{m_achievement.displayName} @ {m_achievement.targetValue}";
                achievementIcon.sprite = m_achievement.icon;

                SetTooltipText();
            }
        }

        void SetTooltipText()
        {
            if (stat == null)
            {
                return;
            }

            m_bodyText = $"{stat.displayName} = {stat.value}.";

            if (m_achievement != null)
            {
                m_bodyText += $"\n\n{m_achievement.description} to unlock \"{m_achievement.displayName}\".";
            }
            m_tooltipTrigger.SetText("BodyText", m_bodyText);
        }
    }
}
