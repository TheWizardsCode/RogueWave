using ModelShark;
using NaughtyAttributes;
using RosgueWave.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RogueWave.GameStats {
    public class AchievementUIElement : RogueWaveUIElement
    {
        [SerializeField, Tooltip("The label to display the achievement name."), Required]
        TextMeshProUGUI m_NameLabel;
        [SerializeField, Tooltip("The icon for this achievement."), Required]
        Image m_Icon;
        [SerializeField, Tooltip("The description of the achievement. Leave as none if you don't want to display the description.")]
        TextMeshProUGUI m_DescriptionLabel;
        [SerializeField, Tooltip("The tooltip trigger for this element.")]
        TooltipTrigger m_Tooltip;
#if DEMO
        [SerializeField, Tooltip("The demo locked indicator"), Required]
        RectTransform m_AchievementIsDemoLocked;
#endif

        Achievement m_achievement;
        public Achievement Achievement
        {
            get { return m_achievement; }
            set
            {
                if (m_achievement == value) { return; }

                m_achievement = value;
                if (m_achievement == null) return;
                
                m_Icon.sprite = m_achievement.icon;
                m_NameLabel.text = m_achievement.displayName;
                if (m_DescriptionLabel != null) m_DescriptionLabel.text = m_achievement.description;

                m_Tooltip.SetText("BodyText", $"{m_achievement.displayName}\n\n{m_achievement.description}");

#if DEMO
                m_AchievementIsDemoLocked.gameObject.SetActive(m_achievement.isDemoLocked);
#endif
            }
        }

    }
}
