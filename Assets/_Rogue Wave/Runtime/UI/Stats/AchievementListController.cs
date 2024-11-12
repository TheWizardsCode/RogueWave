using RogueWave.GameStats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WizardsCode.RogueWave
{
    /// <summary>
    /// This cotroller is responsible for displaying a categorized list of achievements in the game.
    /// It is self configuring, simply plave it on a RectTransform, set the prototype fiels
    /// and it will populate itself with the categirues and achievements.
    /// </summary>
    public class AchievementListController : MonoBehaviour
    {
        [Header("Achievements")]
        [SerializeField, Tooltip("The button to hide the achievements list.")]
        Button m_HideAchievementsListButton;
        [SerializeField, Tooltip("The parent container for the achievement categories.")]
        RectTransform m_AchievementCategoryContainer;
        [SerializeField, Tooltip("The prototype for individual categories of achievements.")]
        AchievementCategoryController m_AchievementCategoryPrototype;

        private bool isInitialized;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        void OnEnable()
        {
            m_HideAchievementsListButton.onClick.AddListener(HideAchievementList);
        }

        private void OnGUI()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                Initialize();
            }
        }

        private void Initialize()
        {
            foreach (Achievement.Category category in Enum.GetValues(typeof(Achievement.Category)))
            {
                List<Achievement> achievements = GameStatsManager.Instance.AllAchievementsInCategory(category);
                if (achievements.Count == 0) continue;

                AchievementCategoryController categoryElement = Instantiate(m_AchievementCategoryPrototype, m_AchievementCategoryContainer);
                categoryElement.Category = category;
                categoryElement.gameObject.SetActive(true);
            }
        }

        private void HideAchievementList()
        {
            gameObject.SetActive(false);
        }
    }
}
