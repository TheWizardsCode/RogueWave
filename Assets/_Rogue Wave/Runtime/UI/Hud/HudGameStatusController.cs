using NeoFPS;
using UnityEngine;
using UnityEngine.UI;
using WizardsCode.RogueWave;

namespace RogueWave
{
    public class HudGameStatusController : PlayerCharacterHudBase
    {
        [SerializeField, Tooltip("The resources UI section. This will be shown and hidden at appropriate times.")]
        private RectTransform m_ResourcesUI = null;
		[SerializeField, Tooltip("The text readout for the current characters resources.")]
		private Text m_ResourcesText = null;
        [SerializeField, Tooltip("The text readout for the number of remaining spawners.")]
        private Text m_SpawnersText = null;
        [SerializeField, Tooltip("The text readout for the number of remaining enemies.")]
        private Text m_EnemiesText = null;
        [SerializeField, Tooltip("The text readout for the current game level number.")]
        private TMPro.TMP_Text m_GameLevelNumberText = null;
        [SerializeField, Tooltip("The text readout for the current players Nanobot level number.")]
        private TMPro.TMP_Text m_NanobotLevelNumberText = null;
        [SerializeField, Tooltip("The panel that contains the level status information.")]
        private RectTransform m_LevelStatusPanel = null;

        private NanobotManager nanobotManager = null;

        protected override void Start()
        {
            base.Start();

            if (m_GameLevelNumberText != null)
            {
                m_GameLevelNumberText.text = (RogueLiteManager.persistentData.currentGameLevel + 1).ToString();
            }

            if (m_NanobotLevelNumberText != null)
            {
                m_NanobotLevelNumberText.text = (RogueLiteManager.persistentData.currentNanobotLevel + 1).ToString();
            }

            m_LevelStatusPanel.gameObject.SetActive(false);
        }

        internal void UpdateSpawnerCount(int count)
        {
            if (m_SpawnersText != null)
            {
                m_SpawnersText.text = count.ToString();
            }
        }

        internal void UpdateEnemyCount(int count)
        {
            if (m_EnemiesText != null)
            {
                m_EnemiesText.text = count.ToString();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnPlayerCharacterChanged(ICharacter character)
        {
            if (nanobotManager != null)
            {
                m_LevelStatusPanel.gameObject.SetActive(true);
            } else
            {
                m_LevelStatusPanel.gameObject.SetActive(false);
            }

            if (character as Component != null)
            {
                nanobotManager = character.GetComponent<NanobotManager>();
            }
            else
            {
                nanobotManager = null;
            }

            if (nanobotManager != null)
            {
                nanobotManager.onNanobotLevelUp += OnNanobotLevelUp;
                OnNanobotLevelUp(RogueLiteManager.persistentData.currentNanobotLevel, 150);
                
                m_ResourcesUI.gameObject.SetActive(true);
            }
            else
            {
                m_ResourcesUI.gameObject.SetActive(false);
            }

            if (m_GameLevelNumberText != null)
            {
                m_GameLevelNumberText.text = (RogueLiteManager.persistentData.currentGameLevel + 1).ToString();
            }
        }

        protected void OnNanobotLevelUp(int level, int resourcesForNextLevel)
        {
            if (m_NanobotLevelNumberText != null)
            {
                m_NanobotLevelNumberText.text = (level + 1).ToString();
            }
        }

        public void OnResourcesChanged(IParameterizedGameEvent<int> e, int parameters)
        {
            m_ResourcesText.text = ((IntGameEvent)e).stat.ToString();
        }
    }
}