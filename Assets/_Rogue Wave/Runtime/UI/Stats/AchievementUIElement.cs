using ModelShark;
using RosgueWave.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RogueWave.GameStats {
    public class AchievementUIElement : RogueWaveUIElement
    {
        [SerializeField, Tooltip("The label to display the achievement name.")]
        TextMeshProUGUI m_label;
        [SerializeField, Tooltip("The icon for this achievement.")]
        Image m_Icon;
        [SerializeField, Tooltip("The tooltip trigger for this element.")]
        TooltipTrigger m_Tooltip;

        Achievement m_achievement;
        public Achievement achievement
        {
            get { return m_achievement; }
            set
            {
                m_achievement = value;
                m_Icon.sprite = m_achievement.icon;
                m_label.text = m_achievement.displayName;

                m_Tooltip.SetText("BodyText", $"{m_achievement.displayName}\n\n{m_achievement.description}");
            }
        }
     
    }
}
