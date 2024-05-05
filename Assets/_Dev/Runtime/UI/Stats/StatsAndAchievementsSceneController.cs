using NeoFPS;
using NeoSaveGames.SceneManagement;
using RogueWave;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RogueWave.GameStats
{ 
    public class StatsAndAchievementsSceneController : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField, Tooltip("The container to put the player stats in.")]
        RectTransform m_PlayerContainer;
        [SerializeField, Tooltip("The prefab to use for the stat elements.")]
        StatUIElement m_StatElementPrefab;
        [SerializeField, Tooltip("The container to put the enemy stats in.")]
        RectTransform m_EnemiesContainer;
        [SerializeField, Tooltip("The stats to track in the enemy category.")]
        GameStat[] m_EnemyStats;
        [SerializeField, Tooltip("The stats to track in the player category.")]
        GameStat[] m_PlayerStats;

        [Header("Achievements")]
        [SerializeField, Tooltip("The container to put the achievements in.")]
        RectTransform m_AchievementsContainer;
        [SerializeField, Tooltip("The prefab to use for the achievement elements.")]
        AchievementUIElement m_AchievementElementPrefab;

        private bool isInitialized;

        void OnEnable()
        {
            NeoFpsInputManager.captureMouseCursor = false;
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
                GameStatsManager.ResetStats();
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
                    AchievementUIElement element = Instantiate(m_AchievementElementPrefab, transform);
                    element.achievement = achievement;

                    element.transform.SetParent(m_AchievementsContainer);
                    element.gameObject.SetActive(true);
                }
            }

            isInitialized = true;
        }

        private void CreateStatElements(GameStat[] stats, Transform parent)
        {
            foreach (GameStat stat in stats)
            {
                StatUIElement element = Instantiate(m_StatElementPrefab, transform);
                element.stat = stat;

                element.transform.SetParent(parent);
                element.gameObject.SetActive(true);
            }
        }

        public static void LoadHubScene()
        {
            NeoSceneManager.LoadScene(RogueLiteManager.hubScene);
        }
    }
}