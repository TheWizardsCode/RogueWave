using NeoFPS;
using NeoSaveGames.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        [SerializeField, Tooltip("The container to put the achievements in.")]
        RectTransform m_AchievementsContainer;
        [SerializeField, Tooltip("The prefab to use for the achievement elements.")]
        AchievementUIElement m_AchievementElementPrefab;

        private bool isInitialized;

        void OnEnable()
        {
            NeoFpsInputManager.captureMouseCursor = false;
            RogueLiteManager.persistentData.isDirty = true; // Set to true as a security in case we fogot to set it somewhere
            RogueLiteManager.SaveProfile();
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
                    AchievementUIElement element = Instantiate(m_AchievementElementPrefab, transform);
                    element.achievement = achievement;

                    element.transform.SetParent(m_AchievementsContainer);
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
                element.stat = stat;

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
    }
}