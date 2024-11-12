using RogueWave.GameStats;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Achievement = RogueWave.GameStats.Achievement;

namespace WizardsCode.RogueWave
{
    public class AchievementCategoryController : MonoBehaviour
    {
        [SerializeField, Tooltip("The title for this category of achievements.")]
        TextMeshProUGUI m_Title;
        [SerializeField, Tooltip("The prototoype for a single category in the achievement list.")]
        RectTransform m_AchievementCategoryPrototype;
        [SerializeField, Tooltip("The container to put the achievements in.")]
        RectTransform m_AchievementContainer;

        private bool isInitialized;

        Achievement.Category m_Category;
        public Achievement.Category Category { 
            get
            {
                return m_Category;
            } 
            set
            {
                if (m_Category != value)
                {
                    m_Category = value;
                    Title = m_Category.ToString();
                    m_achievements = Achievement.AllInCategory(m_Category);

                    foreach (Achievement achievement in m_achievements)
                    {
                        AchievementUIElement achievementElement = Instantiate(m_AchievementCategoryPrototype, m_AchievementContainer).GetComponent<AchievementUIElement>();
                        achievementElement.Achievement = achievement;
                        achievementElement.gameObject.SetActive(true);
                    }
                }
            }
        }

        string Title
        {
            get { return m_Title.text; }
            set { m_Title.text = value; }
        }

        private List<Achievement> m_achievements;
    }
}
