using NeoFPS;
using NeoSaveGames.SceneManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using WizardsCode.RogueWave;

namespace RogueWave.GameStats
{ 
    public class ReconstructionController : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField, Tooltip("The container to put the player stats in.")]
        RectTransform m_PlayerContainer;
        [SerializeField, Tooltip("The prefab to use for the stat elements.")]
        StatUIElement m_StatElementPrefab;
        [SerializeField, Tooltip("The container to put the enemy stats in.")]
        RectTransform m_EnemiesContainer;
        [SerializeField, Tooltip("The stats to track in the enemy category.")]
        IntGameStat[] m_EnemyStats;
        [SerializeField, Tooltip("The stats to track in the player category.")]
        IntGameStat[] m_PlayerStats;

        [Header("Achievements")]
        [SerializeField, Tooltip("The container to put the achievement summaries in on the main page.")]
        RectTransform m_AchievementsSummaryContainer;
        [SerializeField, Tooltip("The prefab to use for the achievement summary elements on the main page.")]
        AchievementUIElement m_AchievementSummaryElementPrefab;
        [SerializeField, Tooltip("The button to show the achievements list.")]
        Button m_ShowAchievementsListButton;
        [SerializeField, Tooltip("The container for the complete achievements list.")]
        RectTransform m_AchievementsListContainer;

        private bool isInitialized;

        void OnEnable()
        {
            NeoFpsInputManager.captureMouseCursor = false;
            RogueLiteManager.persistentData.isDirty = true; // Set to true as a security in case we fogot to set it somewhere
            RogueLiteManager.SaveProfile();

            m_ShowAchievementsListButton.onClick.AddListener(ShowAchievementList);
        }

        void OnDisable()
        {
            NeoFpsInputManager.captureMouseCursor = true;
        }

        private void OnGUI()
        {

#if UNITY_EDITOR
            if (GUI.Button(new Rect(10, 10, 150, 100), "Clear Stats\n(Editor Only)"))
            {
                GameStatsManager.Instance.ResetStats();
                isInitialized = false;

                for (int i = 0; i < m_PlayerContainer.childCount; i++)
                {
                    Destroy(m_PlayerContainer.GetChild(i).gameObject);
                }

                for (int i = 0; i < m_EnemiesContainer.childCount; i++)
                {
                    Destroy(m_EnemiesContainer.GetChild(i).gameObject);
                }
            }
#endif

            if (isInitialized)
            {
                return;
            }

            CreateStatElements(m_EnemyStats, m_EnemiesContainer.transform);
            CreateStatElements(m_PlayerStats, m_PlayerContainer.transform);

            List<Achievement> achievements = GameStatsManager.Instance?.unlockedAchievements;
            if (achievements != null && achievements.Count > 0)
            {
                foreach (Achievement achievement in achievements.OrderByDescending(a => a.timeOfUnlock))
                {
                    AchievementUIElement element = Instantiate(m_AchievementSummaryElementPrefab, transform);
                    element.Achievement = achievement;

                    element.transform.SetParent(m_AchievementsSummaryContainer);
                    element.gameObject.SetActive(true);
                }
            }

            isInitialized = true;
        }

        private void CreateStatElements(IntGameStat[] stats, Transform parent)
        {
            foreach (IntGameStat stat in stats)
            {
                StatUIElement element = Instantiate(m_StatElementPrefab, transform);
                element.name = stat.name;
                element.stat = stat;

                // Get all the achievements in GameStatsManager.instance.Achievements that are not unlocked and have the same stat as the one we are looking at
                List<Achievement> achievements = GameStatsManager.Instance?.Achievements.Where(a => a.stat == stat && !a.isUnlocked).ToList();
                float target = float.MaxValue;
                Achievement tracked = null;
                foreach(Achievement achievement in achievements)
                {
                    if (achievement.targetValue < target)
                    {
                        target = achievement.targetValue;
                        element.achievement = achievement;
                    }
                }

                if (tracked != null)
                {
                    Debug.Log("Tracking achievement: " + tracked.name);
                }

                element.transform.SetParent(parent);
                element.gameObject.SetActive(true);
            }
        }

        public static void LoadNextScene()
        {
            if (RogueLiteManager.hasProfile)
            {
                NeoSceneManager.LoadScene(RogueLiteManager.hubScene);
            } else
            {
                NeoSceneManager.LoadScene(RogueLiteManager.mainMenuScene);
            }
        }

        public void ShowAchievementList()
        {
            // REFACTOR: The element names should not be hard coded. Move the population logic to specialist controllers.

            m_AchievementsListContainer.gameObject.SetActive(true);
            // for each category of achievement create a new category element
            //foreach (Achievement.Category category in Enum.GetValues(typeof(Achievement.Category)))
            //{
            //    RectTransform categoryElement = Instantiate(m_AchievementCategoryPrototype, m_AchievementsListContainer);   
            //    categoryElement.gameObject.SetActive(true);
            //    categoryElement.Find("Title").GetComponent<Text>().text = category.ToString();

            //    // for each achievement in the category create a new achievement element
            //    foreach (Achievement achievement in GameStatsManager.Instance.Achievements.Where(a => a.category == category))
            //    {
            //        RectTransform achievementElement = Instantiate(m_AchievementPrototype, categoryElement);
            //        achievementElement.gameObject.SetActive(true);
            //        achievementElement.Find("Name").GetComponent<Text>().text = achievement.displayName;
            //        achievementElement.Find("Description").GetComponent<Text>().text = achievement.description;
            //        achievementElement.Find("Icon").GetComponent<Image>().sprite = achievement.icon;
            //    }
            //}
        }

        public void HideAcievementList()
        {
            m_AchievementsListContainer.gameObject.SetActive(false);
        }
    }
}